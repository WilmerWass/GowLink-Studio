using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using WASSLink.Abstractions;
using WASSLink.Desktop.Models;
using WASSLink.Download;

namespace WASSLink.Desktop.Services;

/// <summary>
/// Servicio de inspección de metadatos de medios usando yt-dlp --dump-json.
/// Retorna listas tipadas de opciones de Audio y Vídeo para el Quality Grid.
/// </summary>
public class MediaMetadataService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.WriteAsString,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    static MediaMetadataService()
    {
        JsonOptions.Converters.Add(new NullableLongConverter());
        JsonOptions.Converters.Add(new NullableDoubleConverter());
        JsonOptions.Converters.Add(new NullableIntConverter());
    }

    /// <summary>
    /// Analiza los formatos disponibles de una URL vía yt-dlp --dump-json.
    /// </summary>
    public async Task<List<MediaFormatOption>> InspectFormatsAsync(
        string url,
        CancellationToken cancellationToken = default)
    {
        var ytDlpPath = YtDlpDownloadService.ResolveToolPath("yt-dlp", "yt-dlp.exe");

        var startInfo = new ProcessStartInfo
        {
            FileName = ytDlpPath,
            Arguments = $"\"{url.Trim()}\" --dump-json --no-playlist",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process { StartInfo = startInfo };
        process.Start();

        var jsonOutput = await process.StandardOutput.ReadToEndAsync(cancellationToken);
        await process.WaitForExitAsync(cancellationToken);

        if (process.ExitCode != 0 || string.IsNullOrWhiteSpace(jsonOutput))
        {
            var err = await process.StandardError.ReadToEndAsync(cancellationToken);
            throw new InvalidOperationException(
                string.IsNullOrWhiteSpace(err)
                    ? "No se pudieron obtener los metadatos del contenido."
                    : err.Trim());
        }

        return ParseFormats(jsonOutput);
    }

    private static List<MediaFormatOption> ParseFormats(string json)
    {
        var result = new List<MediaFormatOption>();
        var payload = JsonSerializer.Deserialize<YtDlpMetadataPayload>(json, JsonOptions);

        if (payload?.Formats == null || payload.Formats.Count == 0)
            return result;

        var allFormats = payload.Formats;

        var audioGroups = allFormats
            .Where(f =>
                string.Equals(f.Vcodec, "none", StringComparison.OrdinalIgnoreCase) &&
                !string.IsNullOrWhiteSpace(f.Acodec) &&
                !string.Equals(f.Acodec, "none", StringComparison.OrdinalIgnoreCase) &&
                f.Abr.HasValue)
            .GroupBy(f => (int)(f.Abr!.Value / 32) * 32)
            .OrderByDescending(g => g.Key)
            .Select(g => g.OrderByDescending(f => f.Filesize ?? f.FilesizeApprox ?? 0).First())
            .ToList();

        MediaFormatOption? bestAudio = null;

        foreach (var f in audioGroups)
        {
            var abr = (int)Math.Round(f.Abr!.Value);
            var ext = (f.Ext ?? "m4a").ToUpperInvariant();
            var acodec = f.Acodec ?? "AAC";
            var filesize = f.Filesize ?? f.FilesizeApprox;

            var option = new MediaFormatOption
            {
                FormatId = f.FormatId ?? "?",
                Extension = ext,
                Codec = SimplifyCodec(acodec),
                QualityLabel = $"{abr} kbps",
                EstimatedSizeBytes = filesize.HasValue ? filesize.Value : null,
                HasAudio = true,
                MediaType = DownloadMediaType.Audio,
                IsRecommended = false
            };

            result.Add(option);
            bestAudio ??= option;
        }

        if (bestAudio != null)
        {
            var idx = result.IndexOf(bestAudio);
            result[idx] = new MediaFormatOption
            {
                FormatId = bestAudio.FormatId,
                Extension = bestAudio.Extension,
                Codec = bestAudio.Codec,
                QualityLabel = bestAudio.QualityLabel,
                EstimatedSizeBytes = bestAudio.EstimatedSizeBytes,
                HasAudio = true,
                MediaType = DownloadMediaType.Audio,
                IsRecommended = true
            };
        }

        var videoGroups = allFormats
            .Where(f =>
                f.Height.HasValue && f.Height.Value >= 360 &&
                !string.IsNullOrWhiteSpace(f.Vcodec) &&
                !string.Equals(f.Vcodec, "none", StringComparison.OrdinalIgnoreCase))
            .GroupBy(f => f.Height!.Value)
            .OrderByDescending(g => g.Key)
            .Select(g => g.OrderByDescending(f => f.Tbr ?? 0).First())
            .ToList();

        foreach (var f in videoGroups)
        {
            var height = f.Height ?? 0;
            var fps = f.Fps ?? 30;
            var ext = (f.Ext ?? "mp4").ToUpperInvariant();
            var vcodec = f.Vcodec ?? "H.264";
            var filesize = f.Filesize ?? f.FilesizeApprox;

            var label = height switch
            {
                >= 2160 => fps > 30 ? $"4K {fps:F0}fps" : "4K",
                >= 1440 => fps > 30 ? $"1440p {fps:F0}fps" : "1440p",
                >= 1080 => fps > 30 ? $"1080p {fps:F0}fps" : "1080p",
                >= 720 => fps > 30 ? $"720p {fps:F0}fps" : "720p",
                >= 480 => "480p",
                _ => $"{height}p"
            };

            result.Add(new MediaFormatOption
            {
                FormatId = f.FormatId ?? "?",
                Extension = ext,
                Codec = SimplifyCodec(vcodec),
                QualityLabel = label,
                EstimatedSizeBytes = filesize.HasValue ? filesize.Value : null,
                HasAudio = !string.IsNullOrWhiteSpace(f.Acodec) &&
                    !string.Equals(f.Acodec, "none", StringComparison.OrdinalIgnoreCase),
                MediaType = DownloadMediaType.Video,
                IsRecommended = height == 1080
            });
        }

        return result;
    }

    private static string SimplifyCodec(string raw)
    {
        if (raw.StartsWith("mp4a", StringComparison.OrdinalIgnoreCase)) return "AAC";
        if (raw.StartsWith("opus", StringComparison.OrdinalIgnoreCase)) return "Opus";
        if (raw.StartsWith("vorbis", StringComparison.OrdinalIgnoreCase)) return "Vorbis";
        if (raw.StartsWith("avc", StringComparison.OrdinalIgnoreCase) ||
            raw.StartsWith("h264", StringComparison.OrdinalIgnoreCase)) return "H.264";
        if (raw.StartsWith("vp9", StringComparison.OrdinalIgnoreCase)) return "VP9";
        if (raw.StartsWith("av01", StringComparison.OrdinalIgnoreCase)) return "AV1";
        if (raw.StartsWith("hev", StringComparison.OrdinalIgnoreCase)) return "H.265";
        return raw.Length > 12 ? raw[..12] : raw;
    }

    private sealed class YtDlpMetadataPayload
    {
        [JsonPropertyName("formats")]
        public List<YtDlpFormat>? Formats { get; set; }
    }

    private sealed class YtDlpFormat
    {
        [JsonPropertyName("format_id")]
        public string? FormatId { get; set; }

        [JsonPropertyName("ext")]
        public string? Ext { get; set; }

        [JsonPropertyName("vcodec")]
        public string? Vcodec { get; set; }

        [JsonPropertyName("acodec")]
        public string? Acodec { get; set; }

        [JsonPropertyName("abr")]
        public double? Abr { get; set; }

        [JsonPropertyName("filesize")]
        public long? Filesize { get; set; }

        [JsonPropertyName("filesize_approx")]
        public long? FilesizeApprox { get; set; }

        [JsonPropertyName("tbr")]
        public double? Tbr { get; set; }

        [JsonPropertyName("fps")]
        public double? Fps { get; set; }

        [JsonPropertyName("width")]
        public int? Width { get; set; }

        [JsonPropertyName("height")]
        public int? Height { get; set; }
    }

    private sealed class NullableLongConverter : JsonConverter<long?>
    {
        public override long? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
                return null;

            if (reader.TokenType == JsonTokenType.String)
            {
                var raw = reader.GetString();
                if (string.IsNullOrWhiteSpace(raw))
                    return null;

                return long.TryParse(raw, out var value) ? value : null;
            }

            if (reader.TokenType == JsonTokenType.Number && reader.TryGetInt64(out var numberValue))
                return numberValue;

            return null;
        }

        public override void Write(Utf8JsonWriter writer, long? value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value?.ToString());
        }
    }

    private sealed class NullableDoubleConverter : JsonConverter<double?>
    {
        public override double? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
                return null;

            if (reader.TokenType == JsonTokenType.String)
            {
                var raw = reader.GetString();
                if (string.IsNullOrWhiteSpace(raw))
                    return null;

                return double.TryParse(raw, out var value) ? value : null;
            }

            if (reader.TokenType == JsonTokenType.Number && reader.TryGetDouble(out var doubleValue))
                return doubleValue;

            return null;
        }

        public override void Write(Utf8JsonWriter writer, double? value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value?.ToString());
        }
    }

    private sealed class NullableIntConverter : JsonConverter<int?>
    {
        public override int? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
                return null;

            if (reader.TokenType == JsonTokenType.String)
            {
                var raw = reader.GetString();
                if (string.IsNullOrWhiteSpace(raw))
                    return null;

                return int.TryParse(raw, out var value) ? value : null;
            }

            if (reader.TokenType == JsonTokenType.Number && reader.TryGetInt32(out var intValue))
                return intValue;

            return null;
        }

        public override void Write(Utf8JsonWriter writer, int? value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value?.ToString());
        }
    }
}
