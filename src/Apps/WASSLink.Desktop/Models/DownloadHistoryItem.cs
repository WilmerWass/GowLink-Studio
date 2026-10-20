using System;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;

namespace WASSLink.Desktop.Models;

public partial class DownloadHistoryItem : ObservableObject
{
    public Guid? JobId { get; set; }

    [ObservableProperty]
    private string sourceUrl = string.Empty;

    [ObservableProperty]
    private string engine = string.Empty;

    [ObservableProperty]
    private string status = string.Empty;

    [ObservableProperty]
    private double progress;

    [ObservableProperty]
    private string progressDetails = string.Empty;

    [ObservableProperty]
    private string output = string.Empty;

    public string DisplayName => Path.GetFileName(Output) is { Length: > 0 } name
        ? name
        : "Descarga";

    partial void OnOutputChanged(string value) => OnPropertyChanged(nameof(DisplayName));

    public bool HasProgressDetails => !string.IsNullOrWhiteSpace(ProgressDetails);

    partial void OnProgressDetailsChanged(string value) =>
        OnPropertyChanged(nameof(HasProgressDetails));

    [ObservableProperty]
    private DateTime createdAt = DateTime.Now;

    [ObservableProperty]
    private bool isSelected;

    [ObservableProperty]
    private bool isPaused;

    public Action<bool>? PauseDownload { get; set; }
    public Action? CancelDownload { get; set; }

    partial void OnIsPausedChanged(bool value) => PauseDownload?.Invoke(value);
}
