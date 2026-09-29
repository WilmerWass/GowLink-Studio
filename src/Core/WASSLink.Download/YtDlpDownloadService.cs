using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
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
            var audioFormat = string.IsNullOrWhiteSpace(request.Format) ? "mp3" : request.Format.ToLowerInvariant();
            mediaArgs = $"-x --audio-format {audioFormat} --audio-quality 0";
        }
        else
        {
            var videoFormat = string.IsNullOrWhiteSpace(request.Format) ? "mp4" : request.Format.ToLowerInvariant();
            mediaArgs = videoFormat == "mp4"
                ? "-f \"bestvideo[ext=mp4]+bestaudio[ext=m4a]/best[ext=mp4]/best\""
                : $"-f \"bestvideo+bestaudio/best\" --merge-output-format {videoFormat}";
        }

        var outputTemplate = Path.Combine(request.OutputPath, "%(title)s.%(ext)s");
        var arguments = $"\"{request.Url.Trim()}\" {mediaArgs} {ffmpegArg} -o \"{outputTemplate}\" --newline --no-playlist";

        var startInfo = new ProcessStartInfo
        {
            FileName = ytDlpPath,
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
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

        string? line;
        while ((line = await process.StandardOutput.ReadLineAsync(cancellationToken)) != null)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            var match = ProgressRegex.Match(line);
            if (match.Success && double.TryParse(match.Groups["pct"].Value, out var pct))
            {
                var speed = match.Groups["speed"].Success ? match.Groups["speed"].Value : null;
                var eta = match.Groups["eta"].Success ? match.Groups["eta"].Value : null;
                progress?.Report(new DownloadProgress(pct, line, speed, eta));
            }
            else
            {
                progress?.Report(new DownloadProgress(-1, line));
            }
        }

        await process.WaitForExitAsync(cancellationToken);

        if (cancellationToken.IsCancellationRequested)
        {
            return new DownloadResult(false, null, "Descarga cancelada por el usuario.");
        }

        if (process.ExitCode != 0)
        {
            var error = await process.StandardError.ReadToEndAsync(cancellationToken);
            var errorMsg = string.IsNullOrWhiteSpace(error) ? $"yt-dlp finalizó con código de error {process.ExitCode}" : error.Trim();
            return new DownloadResult(false, null, errorMsg);
        }

        return new DownloadResult(true, request.OutputPath, null);
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