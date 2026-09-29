using System;
using System.IO;

namespace WASSLink.Download;

public static class LoggerService
{
    public static string LogsDirectory { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "WASSLink",
        "logs");

    public static string LogFilePath => Path.Combine(LogsDirectory, "app.log");

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
