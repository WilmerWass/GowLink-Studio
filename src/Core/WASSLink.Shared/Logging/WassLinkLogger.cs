using System.Text;

namespace WASSLink.Shared.Logging;

public class GowLinkLogger
{
    private readonly string _logDirectory;
    private readonly string _logFilePath;
    private readonly object _lock = new();

    public GowLinkLogger()
    {
        _logDirectory = GetPreferredLogDirectory();
        Directory.CreateDirectory(_logDirectory);

        _logFilePath = Path.Combine(
            _logDirectory,
            $"gowlink-{DateTime.Now:yyyy-MM-dd}.log");
    }

    public string LogFilePath => _logFilePath;

    public void Info(string message)
        => Write("INFO", message);

    public void Warning(string message)
        => Write("WARN", message);

    public void Error(string message, Exception? exception = null)
    {
        var details = exception is null
            ? message
            : $"{message}{Environment.NewLine}{exception}";

        Write("ERROR", details);
    }

    private string GetPreferredLogDirectory()
    {
        var preferred = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "GowLink",
            "Logs");

        var legacy = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "WASSLink",
            "Logs");

        return Directory.Exists(legacy) && !Directory.Exists(preferred)
            ? legacy
            : preferred;
    }

    private void Write(string level, string message)
    {
        var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        var entry = new StringBuilder()
            .Append('[')
            .Append(timestamp)
            .Append("] ")
            .Append(level)
            .Append(": ")
            .AppendLine(message)
            .ToString();

        lock (_lock)
        {
            File.AppendAllText(_logFilePath, entry);
        }
    }
}

public sealed class WassLinkLogger : GowLinkLogger
{
}
