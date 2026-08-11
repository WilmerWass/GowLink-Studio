using System;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WASSLink.Download;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Input;
using WASSLink.Download;

namespace WASSLink.Desktop.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly YtDlpDownloadService _downloadService = new();
    private CancellationTokenSource? _cts;

    [ObservableProperty]
    private string downloadUrl = string.Empty;

    [ObservableProperty]
    private string statusMessage = "Listo para descargar.";

    [ObservableProperty]
    private double downloadProgress;

    [ObservableProperty]
    private bool isDownloading;

    [RelayCommand]
    private async Task DownloadAsync()
    {
        if (string.IsNullOrWhiteSpace(DownloadUrl))
        {
            StatusMessage = "Ingresa una URL válida.";
            return;
        }

        IsDownloading = true;
        DownloadProgress = 0;
        StatusMessage = "Iniciando descarga...";
        _cts = new CancellationTokenSource();

        var downloadFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), 
            "Downloads", 
            "WASSLink"
        );

        try
        {
            await _downloadService.DownloadAsync(
                DownloadUrl,
                downloadFolder,
                (progress, line) =>
                {
                    if (progress >= 0)
                    {
                        DownloadProgress = progress;
                        StatusMessage = $"Descargando... {progress:F1}%";
                    }
                    else
                    {
                        StatusMessage = line;
                    }
                },
                _cts.Token
            );

            StatusMessage = $"¡Descarga completada! Guardado en: {downloadFolder}";
            DownloadProgress = 100;
        }
        catch (OperationCanceledException)
        {
            StatusMessage = "Descarga cancelada.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsDownloading = false;
            _cts = null;
        }
    }
}