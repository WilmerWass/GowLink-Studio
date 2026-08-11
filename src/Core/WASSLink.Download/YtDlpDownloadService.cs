using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace WASSLink.Download;

public class YtDlpDownloadService
{
    private static string ResolveToolPath(string toolFolder, string exeName)
    {
        var baseDir = AppDomain.CurrentDomain.BaseDirectory;
        var currentDir = Directory.GetCurrentDirectory();

        var possiblePaths = new[]
        {
            Path.Combine(baseDir, "tools", toolFolder, exeName),
            Path.Combine(currentDir, "tools", toolFolder, exeName),
            Path.Combine(baseDir, toolFolder, exeName),
            Path.Combine(currentDir, toolFolder, exeName),
            Path.Combine(baseDir, exeName),
            Path.Combine(currentDir, exeName)
        };

        foreach (var path in possiblePaths)
        {
            if (File.Exists(path))
            {
                return path;
            }
        }

        return exeName;
    }

    public async Task DownloadAsync(
        string url, 
        string outputPath, 
        Action<double, string> onProgress, 
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("La URL no puede estar vacía.", nameof(url));

        var ytDlpPath = ResolveToolPath("yt-dlp", "yt-dlp.exe");
        var ffmpegPath = ResolveToolPath("ffmpeg", "ffmpeg.exe");

        Directory.CreateDirectory(outputPath);

        var ffmpegArg = File.Exists(ffmpegPath) ? $"--ffmpeg-location \"{ffmpegPath}\"" : "";
        var arguments = $"\"{url}\" {ffmpegArg} -o \"{Path.Combine(outputPath, "%(title)s.%(ext)s")}\" --newline";

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
            throw new FileNotFoundException(
                "No se encontró 'yt-dlp.exe'. Verifica que esté ubicado en 'tools/yt-dlp/yt-dlp.exe'."
            );
        }

        var progressRegex = new Regex(@"\[download\]\s+(\d+(?:\.\d+)?)%");

        using var registration = cancellationToken.Register(() =>
        {
            try { process.Kill(); } catch { }
        });

        while (!process.StandardOutput.EndOfStream)
        {
            var line = await process.StandardOutput.ReadLineAsync();
            if (line == null) continue;

            var match = progressRegex.Match(line);
            if (match.Success && double.TryParse(match.Groups[1].Value, out double percentage))
            {
                onProgress(percentage, line);
            }
            else if (!string.IsNullOrWhiteSpace(line))
            {
                onProgress(-1, line);
            }
        }

        await process.WaitForExitAsync(cancellationToken);

        if (process.ExitCode != 0)
        {
            var error = await process.StandardError.ReadToEndAsync();
            throw new Exception($"Error en yt-dlp: {error}");
        }
    }
}