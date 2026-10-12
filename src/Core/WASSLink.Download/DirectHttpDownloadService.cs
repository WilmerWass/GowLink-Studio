using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using WASSLink.Abstractions;

namespace WASSLink.Download;

/// <summary>
/// Descarga archivos expuestos directamente por HTTP o HTTPS.
/// </summary>
public sealed class DirectHttpDownloadService : IDownloadProvider
{
    private static readonly HashSet<string> SupportedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".7z", ".apk", ".avi", ".csv", ".dmg", ".exe", ".flac", ".gif", ".gz",
        ".iso", ".jpeg", ".jpg", ".json", ".m4a", ".mkv", ".mov", ".mp3", ".mp4",
        ".msi", ".pdf", ".png", ".rar", ".tar", ".wav", ".webm", ".webp", ".zip"
    };

    private readonly HttpClient _httpClient;

    public DirectHttpDownloadService(HttpClient? httpClient = null)
    {
        _httpClient = httpClient ?? new HttpClient();
    }

    public string Name => "HTTP Directo (Streaming)";

    public bool CanHandle(string url)
    {
        return Uri.TryCreate(url?.Trim(), UriKind.Absolute, out var uri) &&
               (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps) &&
               SupportedExtensions.Contains(Path.GetExtension(uri.AbsolutePath));
    }

    public async Task<DownloadResult> DownloadAsync(
        DownloadRequest request,
        IProgress<DownloadProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!CanHandle(request.Url))
            return new DownloadResult(false, null, "La URL no apunta a un archivo HTTP/HTTPS compatible.");

        Directory.CreateDirectory(request.OutputPath);

        try
        {
            using var response = await _httpClient.GetAsync(
                request.Url.Trim(),
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);
            response.EnsureSuccessStatusCode();

            var fileName = GetFileName(response.Content.Headers.ContentDisposition, new Uri(request.Url));
            var outputFile = GetUniqueFilePath(request.OutputPath, fileName);
            var totalBytes = response.Content.Headers.ContentLength;
            var downloadedBytes = 0L;
            var stopwatch = Stopwatch.StartNew();

            progress?.Report(new DownloadProgress(0, "Descargando por HTTP Directo...", Title: Path.GetFileNameWithoutExtension(outputFile)));

            await using var source = await response.Content.ReadAsStreamAsync(cancellationToken);
            await using var destination = new FileStream(
                outputFile,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 8192,
                useAsync: true);

            var buffer = new byte[8192];
            int bytesRead;
            while (true)
            {
                if (request.WaitIfPausedAsync is not null)
                    await request.WaitIfPausedAsync(cancellationToken);

                bytesRead = await source.ReadAsync(buffer, cancellationToken);
                if (bytesRead == 0)
                    break;

                if (request.WaitIfPausedAsync is not null)
                    await request.WaitIfPausedAsync(cancellationToken);

                await destination.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
                downloadedBytes += bytesRead;

                progress?.Report(new DownloadProgress(
                    totalBytes is > 0 ? (double)downloadedBytes / totalBytes.Value * 100 : -1,
                    "Descargando por HTTP Directo...",
                    Title: Path.GetFileNameWithoutExtension(outputFile)));

                if (request.MaxSpeedBytesPerSecond > 0)
                {
                    var targetElapsed = TimeSpan.FromSeconds(
                        (double)downloadedBytes / request.MaxSpeedBytesPerSecond);
                    var remainingDelay = targetElapsed - stopwatch.Elapsed;
                    if (remainingDelay > TimeSpan.Zero)
                        await Task.Delay(remainingDelay, cancellationToken);
                }

                var percentage = totalBytes is > 0
                    ? (double)downloadedBytes / totalBytes.Value * 100
                    : -1;
                var speed = stopwatch.Elapsed.TotalSeconds > 0
                    ? FormatSpeed(downloadedBytes / stopwatch.Elapsed.TotalSeconds)
                    : null;
                var eta = totalBytes is > 0 && downloadedBytes > 0 && stopwatch.Elapsed.TotalSeconds > 0
                    ? TimeSpan.FromSeconds((totalBytes.Value - downloadedBytes) /
                                           (downloadedBytes / stopwatch.Elapsed.TotalSeconds)).ToString(@"hh\:mm\:ss")
                    : null;

                progress?.Report(new DownloadProgress(
                    percentage,
                    "Descargando por HTTP Directo...",
                    speed,
                    eta,
                    Path.GetFileNameWithoutExtension(outputFile),
                    totalBytes is > 0 ? FormatSize(totalBytes.Value) : null));
            }

            progress?.Report(new DownloadProgress(100, "Descarga HTTP Directa completada."));
            return new DownloadResult(true, outputFile);
        }
        catch (HttpRequestException ex)
        {
            return new DownloadResult(false, null, $"Error de descarga HTTP: {ex.Message}");
        }
        catch (IOException ex)
        {
            return new DownloadResult(false, null, $"Error al guardar el archivo: {ex.Message}");
        }
    }

    private static string GetFileName(ContentDispositionHeaderValue? disposition, Uri uri)
    {
        var name = disposition?.FileNameStar ?? disposition?.FileName;
        if (!string.IsNullOrWhiteSpace(name))
            name = name.Trim().Trim('"');
        else
            name = Uri.UnescapeDataString(Path.GetFileName(uri.AbsolutePath));

        name = Path.GetFileName(name.Replace('\\', '/'));
        var invalidCharacters = Path.GetInvalidFileNameChars();
        var safeName = new string(name.Where(character => !invalidCharacters.Contains(character)).ToArray());
        return string.IsNullOrWhiteSpace(safeName) ? "download" : safeName;
    }

    private static string GetUniqueFilePath(string directory, string fileName)
    {
        var path = Path.Combine(directory, fileName);
        if (!File.Exists(path))
            return path;

        var name = Path.GetFileNameWithoutExtension(fileName);
        var extension = Path.GetExtension(fileName);
        for (var index = 1; ; index++)
        {
            path = Path.Combine(directory, $"{name} ({index}){extension}");
            if (!File.Exists(path))
                return path;
        }
    }

    private static string FormatSpeed(double bytesPerSecond) =>
        $"{FormatSize((long)bytesPerSecond)}/s";

    private static string FormatSize(long bytes)
    {
        string[] units = ["B", "KB", "MB", "GB"];
        var size = (double)bytes;
        var unit = 0;
        while (size >= 1024 && unit < units.Length - 1)
        {
            size /= 1024;
            unit++;
        }

        return $"{size:0.##} {units[unit]}";
    }
}
