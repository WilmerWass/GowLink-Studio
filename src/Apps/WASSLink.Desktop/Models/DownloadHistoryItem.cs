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
}
