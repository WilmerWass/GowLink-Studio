using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using WASSLink.Abstractions;

namespace WASSLink.Download;

/// <summary>
/// Proveedor de descargas multimedia utilizando yt-dlp y FFmpeg.
/// </summary>
public class YtDlpDownloadService : IDownloadProvider
{
    private static readonly Regex ProgressRegex = new(
        @"\[download\]\s+(?<pct>\d+(?:\.\d+)?)%(?:\s+of\s+~?(?<size>\S+))?(?:\s+at\s+(?<speed>\S+))?(?:\s+ETA\s+(?<eta>\S+))?",
        RegexOptions.Compiled);

    private static readonly Regex DestinationRegex = new(
        @"\[(?:download|ExtractAudio)\]\s+Destination:\s+(?<dest>.+)",
        RegexOptions.Compiled);

    private static readonly Regex AlreadyDownloadedRegex = new(
        @"\[download\]\s+(?<dest>.+)\s+has already been downloaded",
        RegexOptions.Compiled);

    private static readonly Regex MergedDestinationRegex = new(
        @"\[Merger\]\s+Merging formats into\s+""(?<dest>.+)""",
        RegexOptions.Compiled);

    public static string GetLogsDirectory() => LoggerService.LogsDirectory;

    public static string BuildVideoFormatSelector(
        string? formatId = null,
        bool formatHasAudio = false,
        int maxHeight = 1080)
    {
        if (!string.IsNullOrWhiteSpace(formatId))
        {
            var selectedFormat = formatId.Trim();
            return formatHasAudio ? selectedFormat : $"{selectedFormat}+bestaudio/{selectedFormat}";
        }

        var targetHeight = maxHeight > 0 ? maxHeight : 1080;
        return $"bestvideo[height<={targetHeight}]+bestaudio/best[height<={targetHeight}]/best";
    }

    public static string GetProgressState(string? line)
    {
        if (string.IsNullOrWhiteSpace(line))
            return "Descargando...";

        if (line.Contains("[Merger]", StringComparison.OrdinalIgnoreCase) ||
            line.Contains("[ffmpeg]", StringComparison.OrdinalIgnoreCase) ||
            line.Contains("[ExtractAudio]", StringComparison.OrdinalIgnoreCase))
        {
            return "Procesando con FFmpeg...";
        }

        return line.Contains("[download]", StringComparison.OrdinalIgnoreCase)
            ? "Descargando..."
            : "Procesando...";
    }

    public static bool TryParseProgressPercentage(string? value, out double percentage)
    {
        return double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out percentage);
    }

    private static string ResolveAudioFormat(string? format)
    {
        if (string.IsNullOrWhiteSpace(format))
            return "best";

        var normalized = format.Trim().ToLowerInvariant();
        return normalized switch
        {
            "webm" => "opus",
            "m4a" => "m4a",
            "mp3" => "mp3",
            "aac" => "aac",
            "opus" => "opus",
            "flac" => "flac",
            "wav" => "wav",
            "best" => "best",
            _ => normalized
        };
    }

    public string Name => "yt-dlp";

    /// <summary>
    /// Verifica si la URL es válida y puede ser procesada.
    /// </summary>
    public bool CanHandle(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return false;
        }

        return Uri.TryCreate(url.Trim(), UriKind.Absolute, out var uri) &&
               (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }

    /// <summary>
    /// Resuelve la ruta al ejecutable de la herramienta buscando en ubicaciones relativas estándar.
    /// </summary>
    public static string ResolveToolPath(string toolFolder, string exeName)
    {
        var baseDir = AppDomain.CurrentDomain.BaseDirectory;
        var currentDir = Directory.GetCurrentDirectory();

        var directPaths = new[]
        {
            Path.Combine(baseDir, "tools", toolFolder, exeName),
            Path.Combine(currentDir, "tools", toolFolder, exeName),
            Path.Combine(baseDir, toolFolder, exeName),
            Path.Combine(currentDir, toolFolder, exeName),
            Path.Combine(baseDir, exeName),
            Path.Combine(currentDir, exeName)
        };

        foreach (var path in directPaths)
        {
            if (File.Exists(path))
            {
                return path;
            }
        }

        // Buscar en directorios superiores (ej. bin/Debug/net10.0 -> raíz del repositorio)
        var searchRoots = new[] { baseDir, currentDir };
        foreach (var root in searchRoots)
        {
            var dir = new DirectoryInfo(root);
            while (dir != null)
            {
                var candidate = Path.Combine(dir.FullName, "tools", toolFolder, exeName);
                if (File.Exists(candidate))
                {
                    return candidate;
                }

                dir = dir.Parent;
            }
        }

        return exeName;
    }

    /// <summary>
    /// Ejecuta la descarga implementando el contrato IDownloadProvider.
    /// </summary>
    public async Task<DownloadResult> DownloadAsync(
        DownloadRequest request,
        IProgress<DownloadProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        if (string.IsNullOrWhiteSpace(request.Url))
            return new DownloadResult(false, null, "La URL no puede estar vacía.");

        var ytDlpPath = ResolveToolPath("yt-dlp", "yt-dlp.exe");
        var ffmpegPath = ResolveToolPath("ffmpeg", "ffmpeg.exe");

        Directory.CreateDirectory(request.OutputPath);

        var ffmpegArg = File.Exists(ffmpegPath) ? $"--ffmpeg-location \"{ffmpegPath}\"" : "";

        // Configuración de formato y medio
        string mediaArgs;
        if (request.MediaType == DownloadMediaType.Audio)
        {
            if (!string.IsNullOrWhiteSpace(request.FormatId))
            {
                mediaArgs = $"-f \"{request.FormatId}\"";
            }
            else
            {
                var audioFormat = ResolveAudioFormat(request.Format);
                mediaArgs = $"-x --audio-format {audioFormat} --audio-quality 0";
            }
        }
        else
        {
            var videoFormat = string.IsNullOrWhiteSpace(request.Format) ? "mp4" : request.Format.ToLowerInvariant();
            var videoSelector = BuildVideoFormatSelector(request.FormatId, request.VideoFormatHasAudio);
            mediaArgs = $"-f \"{videoSelector}\" --merge-output-format {videoFormat}";
        }

        // --windows-filenames evita caracteres inválidos en Windows
        // --no-part para evitar archivos temporales residuales en fallos
        var outputTemplate = Path.Combine(request.OutputPath, "%(title)s.%(ext)s");
        var arguments = $"\"{request.Url.Trim()}\" {mediaArgs} {ffmpegArg} --windows-filenames --encoding UTF-8 -o \"{outputTemplate}\" --newline --no-playlist";

        LoggerService.Write($"URL={request.Url}");
        LoggerService.Write($"COMMAND={ytDlpPath} {arguments}");

        var startInfo = new ProcessStartInfo
        {
            FileName = ytDlpPath,
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process { StartInfo = startInfo };

        try
        {
            process.Start();
        }
        catch (Win32Exception)
        {
            return new DownloadResult(
                false,
                null,
                "No se encontró 'yt-dlp.exe'. Verifica que esté instalado en 'tools/yt-dlp/yt-dlp.exe'."
            );
        }

        using var registration = cancellationToken.Register(() =>
        {
            try
            {
                if (!process.HasExited)
                {
                    process.Kill(entireProcessTree: true);
                }
            }
            catch
            {
                // Silencioso al cancelar
            }
        });

        string? downloadedFilePath = null;
        var stderrTask = process.StandardError.ReadToEndAsync(cancellationToken);

        string? line;
        while ((line = await process.StandardOutput.ReadLineAsync(cancellationToken)) != null)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            LoggerService.Write($"STDOUT {line}");

            var phase = GetProgressState(line);
            if (phase == "Procesando con FFmpeg...")
            {
                progress?.Report(new DownloadProgress(0, phase));
            }

            if (TryParseOutputPath(line, out var outputPath))
                downloadedFilePath = outputPath;

            var match = ProgressRegex.Match(line);
            if (match.Success && TryParseProgressPercentage(match.Groups["pct"].Value, out var pct))
            {
                var speed = match.Groups["speed"].Success ? match.Groups["speed"].Value : null;
                var eta = match.Groups["eta"].Success ? match.Groups["eta"].Value : null;
                var size = match.Groups["size"].Success ? match.Groups["size"].Value : null;
                var title = downloadedFilePath != null ? Path.GetFileNameWithoutExtension(downloadedFilePath) : null;

                progress?.Report(new DownloadProgress(pct, "Descargando...", speed, eta, title, size));
            }
            else if (phase == "Descargando...")
            {
                progress?.Report(new DownloadProgress(-1, phase));
            }
            else if (phase == "Procesando...")
            {
                progress?.Report(new DownloadProgress(-1, "Procesando con FFmpeg..."));
            }
        }

        await process.WaitForExitAsync(cancellationToken);
        var stderr = await stderrTask;
        if (!string.IsNullOrWhiteSpace(stderr))
        {
            LoggerService.Write($"STDERR {stderr.Trim()}");
        }

        LoggerService.Write($"EXIT CODE={process.ExitCode}");

        if (cancellationToken.IsCancellationRequested)
        {
            return new DownloadResult(false, null, "Descarga cancelada por el usuario.");
        }

        if (process.ExitCode != 0)
        {
            var errorMsg = string.IsNullOrWhiteSpace(stderr)
                ? $"yt-dlp finalizó con código de error {process.ExitCode}"
                : stderr.Trim();
            LoggerService.Write($"ERROR {errorMsg}");
            return new DownloadResult(false, null, errorMsg);
        }

        var finalOutputPath = downloadedFilePath ?? request.OutputPath;
        LoggerService.Write($"DOWNLOAD COMPLETED OutputPath=\"{finalOutputPath}\"");
        return new DownloadResult(true, finalOutputPath, null);
    }

    private static bool TryParseOutputPath(string line, out string? outputPath)
    {
        var mergedMatch = MergedDestinationRegex.Match(line);
        if (mergedMatch.Success)
        {
            outputPath = mergedMatch.Groups["dest"].Value.Trim();
            return true;
        }

        var destinationMatch = DestinationRegex.Match(line);
        if (destinationMatch.Success)
        {
            outputPath = destinationMatch.Groups["dest"].Value.Trim();
            return true;
        }

        var alreadyDownloadedMatch = AlreadyDownloadedRegex.Match(line);
        if (alreadyDownloadedMatch.Success)
        {
            outputPath = alreadyDownloadedMatch.Groups["dest"].Value.Trim();
            return true;
        }

        outputPath = null;
        return false;
    }

    /// <summary>
    /// Sobrecarga directa compatible con versiones anteriores.
    /// </summary>
    public async Task DownloadAsync(
        string url,
        string outputPath,
        Action<double, string>? onProgress = null,
        CancellationToken cancellationToken = default)
    {
        var progressHandler = onProgress != null
            ? new Progress<DownloadProgress>(p => onProgress(p.Percentage, p.StatusMessage ?? string.Empty))
            : null;

        var request = new DownloadRequest(url, outputPath);
        var result = await DownloadAsync(request, progressHandler, cancellationToken);

        if (!result.Success && !cancellationToken.IsCancellationRequested)
        {
            throw new Exception(result.ErrorMessage ?? "Error en la descarga");
        }
    }
}