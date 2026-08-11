using CommunityToolkit.Mvvm.ComponentModel;

namespace WASSLink.Desktop.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    private string downloadUrl = string.Empty;

    [ObservableProperty]
    private string statusMessage = "Listo para descargar.";

    [ObservableProperty]
    private double downloadProgress;

    [ObservableProperty]
    private bool isDownloading;
}