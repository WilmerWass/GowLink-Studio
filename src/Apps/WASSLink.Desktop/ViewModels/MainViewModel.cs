using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WASSLink.Abstractions;
using WASSLink.Download;

namespace WASSLink.Desktop.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly IDownloadProvider _downloadProvider;
    private CancellationTokenSource? _cts;

    public MainViewModel(IDownloadProvider downloadProvider)
    {
        _downloadProvider = downloadProvider ?? throw new ArgumentNullException(nameof(downloadProvider));
        
        OutputPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            "Downloads",
            "WASSLink"
        );
    }

    /// <summary>
    /// Constructor de diseño para previewer de Avalonia.
    /// </summary>
    public MainViewModel() : this(new YtDlpDownloadService())
    {
    }

    [ObservableProperty]
    private string selectedSection = "Downloader";

    [ObservableProperty]
    private string downloadUrl = string.Empty;

    [ObservableProperty]
    private string outputPath;

    [ObservableProperty]
    private string statusMessage = "Listo para descargar.";

    [ObservableProperty]
    private double downloadProgress;

    [ObservableProperty]
    private bool isDownloading;

    [ObservableProperty]
    private int selectedMediaTypeIndex = 0; // 0 = Audio, 1 = Video

    [ObservableProperty]
    private int selectedFormatIndex = 0; // 0 = MP3/MP4 según medio

    [ObservableProperty]
    private string currentSpeed = string.Empty;

    [ObservableProperty]
    private string currentEta = string.Empty;

    [RelayCommand]
    private void SelectSection(string section)
    {
        SelectedSection = section;
    }

    [RelayCommand]
    private async Task DownloadAsync()
    {
        if (string.IsNullOrWhiteSpace(DownloadUrl))
        {
            StatusMessage = "Ingresa una URL válida.";
            return;
        }

        if (!_downloadProvider.CanHandle(DownloadUrl))
        {
            StatusMessage = "La URL ingresada no es válida o no está soportada.";
            return;
        }

        IsDownloading = true;
        DownloadProgress = 0;
        CurrentSpeed = string.Empty;
        CurrentEta = string.Empty;
        StatusMessage = "Iniciando descarga...";
        _cts = new CancellationTokenSource();

        var mediaType = SelectedMediaTypeIndex == 0 ? DownloadMediaType.Audio : DownloadMediaType.Video;
        var format = mediaType == DownloadMediaType.Audio ? "mp3" : "mp4";

        var request = new DownloadRequest(
            Url: DownloadUrl.Trim(),
            OutputPath: OutputPath,
            MediaType: mediaType,
            Format: format
        );

        var progress = new Progress<DownloadProgress>(p =>
        {
            if (p.Percentage >= 0)
            {
                DownloadProgress = p.Percentage;
                if (!string.IsNullOrEmpty(p.Speed))
                {
                    CurrentSpeed = p.Speed;
                }
                if (!string.IsNullOrEmpty(p.Eta))
                {
                    CurrentEta = p.Eta;
                }

                var speedInfo = !string.IsNullOrEmpty(CurrentSpeed) ? $" • {CurrentSpeed}" : "";
                var etaInfo = !string.IsNullOrEmpty(CurrentEta) ? $" • ETA: {CurrentEta}" : "";
                StatusMessage = $"Descargando... {p.Percentage:F1}%{speedInfo}{etaInfo}";
            }
            else if (!string.IsNullOrWhiteSpace(p.StatusMessage))
            {
                StatusMessage = p.StatusMessage;
            }
        });

        try
        {
            var result = await _downloadProvider.DownloadAsync(request, progress, _cts.Token);

            if (result.Success)
            {
                StatusMessage = $"¡Descarga completada! Guardado en: {OutputPath}";
                DownloadProgress = 100;
            }
            else
            {
                StatusMessage = $"Error: {result.ErrorMessage ?? "Fallo desconocido en la descarga."}";
            }
        }
        catch (OperationCanceledException)
        {
            StatusMessage = "Descarga cancelada por el usuario.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error inesperado: {ex.Message}";
        }
        finally
        {
            IsDownloading = false;
            _cts?.Dispose();
            _cts = null;
        }
    }

    [RelayCommand]
    private void CancelDownload()
    {
        if (IsDownloading && _cts != null && !_cts.IsCancellationRequested)
        {
            StatusMessage = "Cancelando descarga...";
            _cts.Cancel();
        }
    }
}