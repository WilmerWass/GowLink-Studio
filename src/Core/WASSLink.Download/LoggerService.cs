using System;
using System.IO;

namespace WASSLink.Download;

public static class LoggerService
{
    public static string LogsDirectory { get; } = ResolveLogsDirectory();

    public static string LogFilePath => Path.Combine(LogsDirectory, "app.log");

    private static string ResolveLogsDirectory()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var preferred = Path.Combine(appData, "GowLink", "logs");
        var legacy = Path.Combine(appData, "WASSLink", "logs");

        return Directory.Exists(legacy) && !Directory.Exists(preferred)
            ? legacy
            : preferred;
    }

    public static void Write(string message)
    {
        try
        {
            Directory.CreateDirectory(LogsDirectory);
            File.AppendAllText(
                LogFilePath,
                $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {message}{Environment.NewLine}");
        }
        catch
        {
            // no-op: no debe romper la descarga ni la UI.
        }
    }

    public static void WriteException(Exception ex, string? context = null)
    {
        var message = context is null ? ex.ToString() : $"{context}: {ex}";
        Write(message);
    }
}
