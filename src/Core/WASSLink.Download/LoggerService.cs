using System;
using System.IO;

namespace WASSLink.Download;

public static class LoggerService
{
    public static string LogsDirectory { get; } = ResolveLogsDirectory();

    public static string LogFilePath => Path.Combine(LogsDirectory, "app.log");

    private static string ResolveLogsDirectory()
    {
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        return Path.Combine(localAppData, "GowLink", "Logs");
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
