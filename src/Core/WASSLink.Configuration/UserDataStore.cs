using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace WASSLink.Configuration;

public sealed record GowLinkSettings(
    string OutputPath,
    int DownloadSpeedLimitKbps,
    string AccentColorName,
    int MaximumConcurrency);

public sealed record DownloadHistoryRecord(
    string SourceUrl,
    string Engine,
    string Status,
    double Progress,
    string Output,
    DateTime CreatedAt);

public interface IUserDataStore
{
    GowLinkSettings? LoadSettings();
    void SaveSettings(GowLinkSettings settings);
    IReadOnlyList<DownloadHistoryRecord> LoadHistory();
    void SaveHistory(IReadOnlyList<DownloadHistoryRecord> records);
}

public sealed class UserDataStore : IUserDataStore
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    private readonly string _directory;

    public UserDataStore()
        : this(Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "GowLink"))
    {
    }

    public UserDataStore(string directory)
    {
        _directory = directory ?? throw new ArgumentNullException(nameof(directory));
    }

    public GowLinkSettings? LoadSettings() =>
        Read<GowLinkSettings>("settings.json");

    public void SaveSettings(GowLinkSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        Write("settings.json", settings);
    }

    public IReadOnlyList<DownloadHistoryRecord> LoadHistory() =>
        Read<List<DownloadHistoryRecord>>("history.json") ?? [];

    public void SaveHistory(IReadOnlyList<DownloadHistoryRecord> records)
    {
        ArgumentNullException.ThrowIfNull(records);
        Write("history.json", records);
    }

    private T? Read<T>(string fileName)
    {
        var path = Path.Combine(_directory, fileName);
        if (!File.Exists(path))
            return default;

        using var stream = File.OpenRead(path);
        return JsonSerializer.Deserialize<T>(stream, JsonOptions);
    }

    private void Write<T>(string fileName, T value)
    {
        Directory.CreateDirectory(_directory);
        var path = Path.Combine(_directory, fileName);
        var temporaryPath = $"{path}.{Guid.NewGuid():N}.tmp";
        try
        {
            using (var stream = new FileStream(
                       temporaryPath,
                       FileMode.CreateNew,
                       FileAccess.Write,
                       FileShare.None,
                       bufferSize: 4096,
                       useAsync: false))
            {
                JsonSerializer.Serialize(stream, value, JsonOptions);
                stream.Flush(flushToDisk: true);
            }

            File.Move(temporaryPath, path, overwrite: true);
        }
        finally
        {
            if (File.Exists(temporaryPath))
                File.Delete(temporaryPath);
        }
    }
}
