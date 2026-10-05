using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WASSLink.Abstractions;
using WASSLink.Desktop.Models;
using WASSLink.Desktop.Services;
using WASSLink.Download;

namespace WASSLink.Desktop.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly IDownloadProvider _downloadProvider;
    private readonly IClipboardService _clipboardService;
    private readonly MediaMetadataService _metadataService;
    private CancellationTokenSource? _cts;
    private CancellationTokenSource? _metadataCts;

    public MainViewModel(IDownloadProvider downloadProvider, IClipboardService clipboardService)
    {
        _downloadProvider = downloadProvider ?? throw new ArgumentNullException(nameof(downloadProvider));
        _clipboardService = clipboardService ?? throw new ArgumentNullException(nameof(clipboardService));
        _metadataService = new MediaMetadataService();

        OutputPath = ResolveDefaultOutputPath();
    }

    /// <summary>
    /// Constructor de diseño para previewer de Avalonia.
    /// </summary>
    public MainViewModel() : this(new YtDlpDownloadService(), new AvaloniaClipboardService())
    {
    }

    private static string ResolveDefaultOutputPath()
    {
        var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var preferred = Path.Combine(userProfile, "Downloads", "GowLink");
        var legacy = Path.Combine(userProfile, "Downloads", "WASSLink");

        return Directory.Exists(legacy) && !Directory.Exists(preferred)
            ? legacy
            : preferred;
    }

    // ─── Navegación ──────────────────────────────────────────────────────────────

    [ObservableProperty]
    private string selectedSection = "Downloader";

    // ─── Descarga principal ───────────────────────────────────────────────────────

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
    private string currentSpeed = string.Empty;

    [ObservableProperty]
    private string currentEta = string.Empty;

    [ObservableProperty]
    private string currentTitle = string.Empty;

    [ObservableProperty]
    private string currentSize = string.Empty;

    [ObservableProperty]
    private string lastDownloadedFile = string.Empty;

    // ─── Quality Grid / Modal de Formatos ────────────────────────────────────────

    [ObservableProperty]
    private bool isFormatModalOpen;

    [ObservableProperty]
    private bool isLoadingFormats;

    [ObservableProperty]
    private string formatLoadError = string.Empty;

    [ObservableProperty]
    private ObservableCollection<MediaFormatOption> audioOptions = new();

    [ObservableProperty]
    private ObservableCollection<MediaFormatOption> videoOptions = new();

    [ObservableProperty]
    private MediaFormatOption? selectedFormatOption;

    /// <summary>
    /// Indica si hay formatos cargados para mostrar el grid.
    /// </summary>
    public bool HasFormats => AudioOptions.Count > 0 || VideoOptions.Count > 0;

    /// <summary>
    /// Label descriptivo de la opción seleccionada para el botón principal.
    /// </summary>
    public string SelectedFormatLabel =>
        SelectedFormatOption != null
            ? $"{SelectedFormatOption.Extension} · {SelectedFormatOption.QualityLabel} · {SelectedFormatOption.FormattedSize}"
            : "Seleccionar formato...";

    // ─── Comandos ─────────────────────────────────────────────────────────────────

    [RelayCommand]
    private void SelectSection(string section) => SelectedSection = section;

    [RelayCommand]
    private void ClearDownloadUrl()
    {
        _metadataCts?.Cancel();
        _metadataCts?.Dispose();
        _metadataCts = null;

        DownloadUrl = string.Empty;
        AudioOptions.Clear();
        VideoOptions.Clear();
        SelectedFormatOption = null;
        FormatLoadError = string.Empty;
        IsLoadingFormats = false;
        IsFormatModalOpen = false;
        StatusMessage = "Enlace limpiado.";

        OnPropertyChanged(nameof(HasFormats));
        OnPropertyChanged(nameof(SelectedFormatLabel));
    }

    /// <summary>
    /// Abre el modal Quality Grid y lanza la inspección asíncrona de formatos.
    /// </summary>
    [RelayCommand]
    private async Task OpenFormatModalAsync()
    {
        if (string.IsNullOrWhiteSpace(DownloadUrl) || !_downloadProvider.CanHandle(DownloadUrl))
        {
            StatusMessage = "Ingresa una URL válida antes de inspeccionar formatos.";
            return;
        }

        IsFormatModalOpen = true;
        IsLoadingFormats = true;
        FormatLoadError = string.Empty;
        AudioOptions.Clear();
        VideoOptions.Clear();
        SelectedFormatOption = null;
        OnPropertyChanged(nameof(HasFormats));
        OnPropertyChanged(nameof(SelectedFormatLabel));

        _metadataCts?.Cancel();
        _metadataCts?.Dispose();
        _metadataCts = new CancellationTokenSource(TimeSpan.FromSeconds(60));

        try
        {
            StatusMessage = "Analizando formatos disponibles...";
            var formats = await _metadataService.InspectFormatsAsync(DownloadUrl, _metadataCts.Token);

            foreach (var f in formats.Where(f => f.MediaType == DownloadMediaType.Audio))
                AudioOptions.Add(f);

            foreach (var f in formats.Where(f => f.MediaType == DownloadMediaType.Video))
                VideoOptions.Add(f);

            // Pre-seleccionar la opción recomendada si existe
            SelectedFormatOption = VideoOptions.FirstOrDefault(f => f.IsRecommended)
                ?? VideoOptions.FirstOrDefault()
                ?? AudioOptions.FirstOrDefault(f => f.IsRecommended)
                ?? AudioOptions.FirstOrDefault();

            if (SelectedFormatOption != null)
                SelectedFormatOption.IsSelected = true;

            OnPropertyChanged(nameof(HasFormats));
            OnPropertyChanged(nameof(SelectedFormatLabel));
            StatusMessage = $"Se encontraron {formats.Count} formatos disponibles.";
        }
        catch (OperationCanceledException)
        {
            FormatLoadError = "Tiempo de espera agotado al obtener los metadatos.";
            StatusMessage = "Análisis cancelado.";
        }
        catch (Exception ex)
        {
            FormatLoadError = $"Error al analizar: {ex.Message}";
            StatusMessage = "No se pudieron obtener los formatos.";
        }
        finally
        {
            IsLoadingFormats = false;
        }
    }

    /// <summary>
    /// Selecciona visualmente una tarjeta de formato y actualiza la opción activa.
    /// </summary>
    [RelayCommand]
    private void SelectFormat(MediaFormatOption option)
    {
        foreach (var item in AudioOptions.Concat(VideoOptions))
            item.IsSelected = false;

        option.IsSelected = true;
        SelectedFormatOption = option;
        OnPropertyChanged(nameof(SelectedFormatLabel));
    }

    /// <summary>
    /// Confirma la selección y cierra el modal; luego inicia la descarga con el formato elegido.
    /// </summary>
    [RelayCommand]
    private async Task ConfirmSelectionAsync()
    {
        if (SelectedFormatOption == null)
        {
            FormatLoadError = "Debes seleccionar un formato antes de continuar.";
            return;
        }

        IsFormatModalOpen = false;
        await StartDownloadWithFormatAsync(SelectedFormatOption);
    }

    /// <summary>
    /// Cierra el modal sin cambiar la selección actual.
    /// </summary>
    [RelayCommand]
    private void CloseModal()
    {
        _metadataCts?.Cancel();
        IsFormatModalOpen = false;
    }

    /// <summary>
    /// Descarga directa (botón clásico). Usa el formato seleccionado si existe, o los defaults.
    /// </summary>
    [RelayCommand]
    private async Task DownloadAsync()
    {
        if (SelectedFormatOption != null)
        {
            IsFormatModalOpen = false;
            await StartDownloadWithFormatAsync(SelectedFormatOption);
        }
        else
        {
            await StartDownloadWithFormatAsync(null);
        }
    }

    private async Task StartDownloadWithFormatAsync(MediaFormatOption? format)
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

        var mediaType = format?.MediaType ?? DownloadMediaType.Video;
        var ext = format?.MediaType == DownloadMediaType.Audio
            ? format.Extension.ToLowerInvariant()
            : "mp4";

        var request = new DownloadRequest(
            Url: DownloadUrl.Trim(),
            OutputPath: OutputPath,
            MediaType: mediaType,
            Format: ext,
            FormatId: format?.FormatId,
            VideoFormatHasAudio: format?.HasAudio ?? false
        );

        var progress = new Progress<DownloadProgress>(p =>
        {
            if (p.StatusMessage != null && p.StatusMessage.Contains("FFmpeg", StringComparison.OrdinalIgnoreCase))
            {
                DownloadProgress = 0;
                CurrentSpeed = string.Empty;
                CurrentEta = string.Empty;
                StatusMessage = p.StatusMessage;
                return;
            }

            if (p.Percentage >= 0)
            {
                DownloadProgress = p.Percentage;
                if (!string.IsNullOrEmpty(p.Speed)) CurrentSpeed = p.Speed;
                if (!string.IsNullOrEmpty(p.Eta)) CurrentEta = p.Eta;
                if (!string.IsNullOrEmpty(p.Title)) CurrentTitle = p.Title;
                if (!string.IsNullOrEmpty(p.TotalSize)) CurrentSize = p.TotalSize;

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
                LastDownloadedFile = result.OutputPath ?? OutputPath;
                var fileName = Path.GetFileName(LastDownloadedFile);
                StatusMessage = string.IsNullOrWhiteSpace(fileName)
                    ? $"¡Descarga completada! Guardado en: {OutputPath}"
                    : $"¡Completado!: {fileName}";

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
    private async Task PasteFromClipboardAsync()
    {
        try
        {
            var clipboardText = await _clipboardService.GetTextAsync();
            if (!string.IsNullOrWhiteSpace(clipboardText))
            {
                var candidate = clipboardText.Trim();
                if (_downloadProvider.CanHandle(candidate))
                {
                    DownloadUrl = candidate;
                    StatusMessage = "Enlace pegado desde el portapapeles.";
                }
                else
                {
                    StatusMessage = "El contenido del portapapeles no es un enlace válido.";
                }
            }
            else
            {
                StatusMessage = "El portapapeles está vacío.";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error al leer el portapapeles: {ex.Message}";
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

    [RelayCommand]
    private void OpenLogsFolder()
    {
        var logsDirectory = YtDlpDownloadService.GetLogsDirectory();
        Directory.CreateDirectory(logsDirectory);

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "explorer.exe",
                Arguments = $"\"{logsDirectory}\"",
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            StatusMessage = $"No se pudo abrir la carpeta de logs: {ex.Message}";
        }
    }
}