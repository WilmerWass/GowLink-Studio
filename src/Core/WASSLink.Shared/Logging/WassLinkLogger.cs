using System.Text;

namespace WASSLink.Shared.Logging;

public sealed class WassLinkLogger
{
    private readonly string _logDirectory;
    private readonly string _logFilePath;
    private readonly object _lock = new();

    public WassLinkLogger()
    {
        _logDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "WASSLink",
            "Logs");

        Directory.CreateDirectory(_logDirectory);

        _logFilePath = Path.Combine(
            _logDirectory,
            $"wasslink-{DateTime.Now:yyyy-MM-dd}.log");
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
