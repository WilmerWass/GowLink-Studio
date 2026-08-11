using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace WASSLink.Shared.Diagnostics;

public sealed class DiagnosticReportGenerator
{
    private readonly string _logDirectory;

    public DiagnosticReportGenerator()
    {
        _logDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "WASSLink",
            "Logs");
    }

    public string GenerateReport()
    {
        Directory.CreateDirectory(_logDirectory);

        var timestamp = DateTime.Now;
        var reportPath = Path.Combine(
            _logDirectory,
            $"WASSLink-Diagnostic-{timestamp:yyyy-MM-dd-HHmmss}.txt");

        var assembly = Assembly.GetEntryAssembly();

        var report = new StringBuilder();

        report.AppendLine("WASSLink Studio - Diagnostic Report");
        report.AppendLine("===================================");
        report.AppendLine();

        report.AppendLine($"Generated: {timestamp:O}");
        report.AppendLine($"Application: {assembly?.GetName().Name ?? "Unknown"}");
        report.AppendLine($"Version: {assembly?.GetName().Version?.ToString() ?? "Unknown"}");
        report.AppendLine();

        report.AppendLine("Environment");
        report.AppendLine("-----------");
        report.AppendLine($"OS: {Environment.OSVersion}");
        report.AppendLine($"Architecture: {RuntimeInformation.OSArchitecture}");
        report.AppendLine($"Process Architecture: {RuntimeInformation.ProcessArchitecture}");
        report.AppendLine($".NET: {Environment.Version}");
        report.AppendLine();

        report.AppendLine();

        report.AppendLine("Recent Logs");
        report.AppendLine("-----------");

        if (Directory.Exists(_logDirectory))
        {
            var logFiles = Directory
                .GetFiles(_logDirectory, "*.log")
                .OrderByDescending(File.GetLastWriteTime)
                .Take(3);

            foreach (var logFile in logFiles)
            {
                report.AppendLine();
                report.AppendLine($"--- {Path.GetFileName(logFile)} ---");

                try
                {
                    report.AppendLine(File.ReadAllText(logFile));
                }
                catch (Exception ex)
                {
                    report.AppendLine($"Unable to read log: {ex.Message}");
                }
            }
        }
        else
        {
            report.AppendLine("No log directory found.");
        }

        File.WriteAllText(reportPath, report.ToString(), Encoding.UTF8);

        return reportPath;
    }
}
