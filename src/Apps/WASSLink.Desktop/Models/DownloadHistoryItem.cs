using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace WASSLink.Desktop.Models;

public partial class DownloadHistoryItem : ObservableObject
{
    [ObservableProperty]
    private string sourceUrl = string.Empty;

    [ObservableProperty]
    private string engine = string.Empty;

    [ObservableProperty]
    private string status = string.Empty;

    [ObservableProperty]
    private double progress;

    [ObservableProperty]
    private string output = string.Empty;

    [ObservableProperty]
    private DateTime createdAt = DateTime.Now;

    [ObservableProperty]
    private bool isSelected;

    [ObservableProperty]
    private bool isPaused;

    public Action<bool>? PauseDownload { get; set; }

    partial void OnIsPausedChanged(bool value) => PauseDownload?.Invoke(value);
}
