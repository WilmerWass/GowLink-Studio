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
    private readonly IFolderPickerService? _folderPickerService;
    private readonly MediaMetadataService _metadataService;
    private CancellationTokenSource? _cts;
    private CancellationTokenSource? _metadataCts;
    private CancellationTokenSource? _toastCts;

    public MainViewModel(
        IDownloadProvider downloadProvider,
        IClipboardService clipboardService,
        IFolderPickerService? folderPickerService = null)
    {
        _downloadProvider = downloadProvider ?? throw new ArgumentNullException(nameof(downloadProvider));
        _clipboardService = clipboardService ?? throw new ArgumentNullException(nameof(clipboardService));
        _folderPickerService = folderPickerService;
        _metadataService = new MediaMetadataService();

        OutputPath = ResolveDefaultOutputPath();
    }

    /// <summary>
    /// Constructor de diseño para previewer de Avalonia.
    /// </summary>
    public MainViewModel() : this(
        new CompositeDownloadProvider(new DirectHttpDownloadService(), new YtDlpDownloadService()),
        new AvaloniaClipboardService())
    {
    }

    private static string ResolveDefaultOutputPath()
    {
        var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        return Path.Combine(userProfile, "Downloads", "GowLink");
    }

    // ─── Navegación ──────────────────────────────────────────────────────────────

    [ObservableProperty]
    private string selectedSection = "Downloader";

    public bool IsDownloaderSelected => SelectedSection == "Downloader";
    public bool IsSearchSelected => SelectedSection == "Buscador";
    public bool IsDownloadsSelected => SelectedSection == "Descargas";
    public bool IsSettingsSelected => SelectedSection == "Configuración";

    partial void OnSelectedSectionChanged(string value)
    {
        OnPropertyChanged(nameof(IsDownloaderSelected));
        OnPropertyChanged(nameof(IsSearchSelected));
        OnPropertyChanged(nameof(IsDownloadsSelected));
        OnPropertyChanged(nameof(IsSettingsSelected));
        OnPropertyChanged(nameof(ActiveEngineLabel));
        OnPropertyChanged(nameof(EngineStatus));
    }

    [ObservableProperty]
    private string searchQuery = string.Empty;

    partial void OnSearchQueryChanged(string value)
    {
        OnPropertyChanged(nameof(ActiveEngineLabel));
        OnPropertyChanged(nameof(EngineStatus));
    }

    [ObservableProperty]
    private string searchResultMessage = "Ingresa un enlace para consultar el contenido y el motor compatible.";

    [ObservableProperty]
    private bool isSearching;

    [ObservableProperty]
    private ObservableCollection<DownloadHistoryItem> downloadHistory = new();

    public bool HasDownloadHistory => DownloadHistory.Count > 0;

    // ─── Descarga principal ───────────────────────────────────────────────────────

    [ObservableProperty]
    private string downloadUrl = string.Empty;

    public string ActiveEngineLabel
    {
        get
        {
            var url = SelectedSection == "Buscador" ? SearchQuery : DownloadUrl;
            if (string.IsNullOrWhiteSpace(url))
                return "Motor: esperando enlace";

            var provider = GetProviderForUrl(url);
            if (provider is null)
                return "Motor: no compatible";

            var engineName = provider.Name.Equals("yt-dlp", StringComparison.OrdinalIgnoreCase)
                ? "yt-dlp & FFmpeg"
                : provider.Name;
            return $"Motor: {engineName}";
        }
    }

    public string EngineStatus
    {
        get
        {
            var url = SelectedSection == "Buscador" ? SearchQuery : DownloadUrl;
            return string.IsNullOrWhiteSpace(url)
                ? "Esperando un enlace"
                : _downloadProvider.CanHandle(url)
                    ? "Proveedor seleccionado para este enlace"
                    : "URL no compatible";
        }
    }

    partial void OnDownloadUrlChanged(string value)
    {
        OnPropertyChanged(nameof(ActiveEngineLabel));
        OnPropertyChanged(nameof(EngineStatus));
    }

    private IDownloadProvider? GetProviderForUrl(string url)
    {
        return _downloadProvider is CompositeDownloadProvider composite
            ? composite.GetProviderForUrl(url)
            : _downloadProvider.CanHandle(url) ? _downloadProvider : null;
    }

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

    // ─── Fases de progreso (para animaciones) ────────────────────────────────────

    /// <summary>
    /// True durante la descarga pura (yt-dlp descargando segmentos).
    /// </summary>
    [ObservableProperty]
    private bool isPhaseDownloading;

    /// <summary>
    /// True durante el procesamiento FFmpeg (muxing / conversión de audio).
    /// La barra pasa a modo indeterminado en la UI.
    /// </summary>
    [ObservableProperty]
    private bool isPhaseProcessing;

    // ─── Toast de completado ──────────────────────────────────────────────────────

    /// <summary>
    /// True cuando se debe mostrar el toast de descarga completada.
    /// Se oculta automáticamente tras 4 segundos.
    /// </summary>
    [ObservableProperty]
    private bool isToastVisible;

    /// <summary>
    /// Texto de la primera línea del toast (nombre del archivo).
    /// </summary>
    [ObservableProperty]
    private string toastTitle = string.Empty;

    /// <summary>
    /// Texto de la segunda línea del toast (ruta de destino).
    /// </summary>
    [ObservableProperty]
    private string toastSubtitle = string.Empty;

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
    private async Task SearchAsync()
    {
        var url = SearchQuery.Trim();
        if (string.IsNullOrWhiteSpace(url))
        {
            SearchResultMessage = "Ingresa un enlace para iniciar la consulta.";
            return;
        }

        var selectedProvider = GetProviderForUrl(url);
        if (selectedProvider is null)
        {
            SearchResultMessage = "El enlace no es válido o no está soportado.";
            return;
        }

        DownloadUrl = url;
        if (selectedProvider is DirectHttpDownloadService)
        {
            SearchResultMessage = $"Archivo directo detectado. Motor: {selectedProvider.Name}.";
            return;
        }

        _metadataCts?.Cancel();
        _metadataCts?.Dispose();
        _metadataCts = new CancellationTokenSource(TimeSpan.FromSeconds(60));
        IsSearching = true;
        SearchResultMessage = "Consultando contenido y formatos con yt-dlp...";

        try
        {
            var formats = await _metadataService.InspectFormatsAsync(url, _metadataCts.Token);
            AudioOptions.Clear();
            VideoOptions.Clear();
            foreach (var format in formats.Where(format => format.MediaType == DownloadMediaType.Audio))
                AudioOptions.Add(format);
            foreach (var format in formats.Where(format => format.MediaType == DownloadMediaType.Video))
                VideoOptions.Add(format);

            SelectedFormatOption = VideoOptions.FirstOrDefault(format => format.IsRecommended)
                ?? VideoOptions.FirstOrDefault()
                ?? AudioOptions.FirstOrDefault(format => format.IsRecommended)
                ?? AudioOptions.FirstOrDefault();
            OnPropertyChanged(nameof(HasFormats));
            OnPropertyChanged(nameof(SelectedFormatLabel));
            SearchResultMessage = $"Contenido compatible con {selectedProvider.Name}: {formats.Count} formatos disponibles.";
        }
        catch (OperationCanceledException)
        {
            SearchResultMessage = "La consulta agotó el tiempo de espera.";
        }
        catch (Exception ex)
        {
            SearchResultMessage = $"No se pudo consultar el enlace: {ex.Message}";
        }
        finally
        {
            IsSearching = false;
        }
    }

    [RelayCommand]
    private void DownloadSearchedLink()
    {
        if (GetProviderForUrl(SearchQuery) is null)
        {
            SearchResultMessage = "Primero ingresa y consulta un enlace compatible.";
            return;
        }

        DownloadUrl = SearchQuery.Trim();
        SelectedSection = "Downloader";
    }

    [RelayCommand]
    private void OpenDownloads() => SelectedSection = "Descargas";

    /// <summary>
    /// Abre el diálogo de selección de carpeta y actualiza OutputPath.
    /// </summary>
    [RelayCommand]
    private async Task BrowseFolderAsync()
    {
        if (_folderPickerService is null) return;

        var chosen = await _folderPickerService.PickFolderAsync(OutputPath);
        if (!string.IsNullOrWhiteSpace(chosen))
        {
            OutputPath = chosen;
            StatusMessage = $"Carpeta de destino: {chosen}";
        }
    }

    /// <summary>
    /// Abre la carpeta de destino actual en el explorador de archivos.
    /// </summary>
    [RelayCommand]
    private void OpenOutputFolder()
    {
        var path = OutputPath;
        if (string.IsNullOrWhiteSpace(path)) return;

        try
        {
            Directory.CreateDirectory(path);
            Process.Start(new ProcessStartInfo
            {
                FileName = "explorer.exe",
                Arguments = $"\"{path}\"",
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            StatusMessage = $"No se pudo abrir la carpeta: {ex.Message}";
        }
    }

    /// <summary>
    /// Cierra el toast manualmente.
    /// </summary>
    [RelayCommand]
    private void DismissToast()
    {
        _toastCts?.Cancel();
        IsToastVisible = false;
    }

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
        if (string.IsNullOrWhiteSpace(DownloadUrl))
        {
            StatusMessage = "Ingresa una URL válida antes de inspeccionar formatos.";
            return;
        }

        var selectedProvider = GetProviderForUrl(DownloadUrl);
        if (selectedProvider is null)
        {
            StatusMessage = "La URL ingresada no es válida o no está soportada.";
            return;
        }

        if (selectedProvider is DirectHttpDownloadService)
        {
            StatusMessage = "Archivo directo detectado. Se descargará con HTTP Directo (Streaming).";
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

        if (GetProviderForUrl(DownloadUrl) is null)
        {
            StatusMessage = "La URL ingresada no es válida o no está soportada.";
            return;
        }

        IsDownloading = true;
        IsPhaseDownloading = false;
        IsPhaseProcessing = false;
        IsToastVisible = false;
        DownloadProgress = 0;
        CurrentSpeed = string.Empty;
        CurrentEta = string.Empty;
        CurrentTitle = string.Empty;
        CurrentSize = string.Empty;
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

        var selectedProvider = GetProviderForUrl(request.Url)!;
        var historyItem = new DownloadHistoryItem
        {
            SourceUrl = request.Url,
            Engine = selectedProvider.Name == "yt-dlp" ? "yt-dlp & FFmpeg" : selectedProvider.Name,
            Status = "Iniciando",
            Output = OutputPath
        };
        DownloadHistory.Insert(0, historyItem);
        OnPropertyChanged(nameof(HasDownloadHistory));

        var progress = new Progress<DownloadProgress>(p =>
        {
            historyItem.Status = p.StatusMessage ?? "Descargando";
            if (p.Percentage >= 0)
                historyItem.Progress = p.Percentage;

            var isProcessingPhase = p.StatusMessage != null &&
                p.StatusMessage.Contains("FFmpeg", StringComparison.OrdinalIgnoreCase);

            if (isProcessingPhase)
            {
                IsPhaseDownloading = false;
                IsPhaseProcessing = true;
                DownloadProgress = 0;
                CurrentSpeed = string.Empty;
                CurrentEta = string.Empty;
                StatusMessage = p.StatusMessage!;
                return;
            }

            if (p.Percentage >= 0)
            {
                IsPhaseDownloading = true;
                IsPhaseProcessing = false;
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
                IsPhaseDownloading = true;
                IsPhaseProcessing = false;
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
                var displayName = string.IsNullOrWhiteSpace(fileName) ? "Archivo descargado" : fileName;

                StatusMessage = $"¡Completado!: {displayName}";
                DownloadProgress = 100;
                historyItem.Status = "Completada";
                historyItem.Progress = 100;
                historyItem.Output = LastDownloadedFile;
                SelectedSection = "Descargas";

                // Mostrar toast de completado
                await ShowToastAsync(displayName, OutputPath);
            }
            else
            {
                var errorMessage = result.ErrorMessage ?? "Fallo desconocido en la descarga.";
                StatusMessage = $"Error: {errorMessage}";
                historyItem.Status = $"Error: {errorMessage}";
            }
        }
        catch (OperationCanceledException)
        {
            StatusMessage = "Descarga cancelada por el usuario.";
            historyItem.Status = "Cancelada";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error inesperado: {ex.Message}";
            historyItem.Status = $"Error: {ex.Message}";
        }
        finally
        {
            IsDownloading = false;
            IsPhaseDownloading = false;
            IsPhaseProcessing = false;
            _cts?.Dispose();
            _cts = null;
        }
    }

    /// <summary>
    /// Muestra el toast de completado y lo oculta automáticamente tras 4 segundos.
    /// </summary>
    private async Task ShowToastAsync(string title, string subtitle)
    {
        _toastCts?.Cancel();
        _toastCts?.Dispose();
        _toastCts = new CancellationTokenSource();

        ToastTitle = title;
        ToastSubtitle = subtitle;
        IsToastVisible = true;

        try
        {
            await Task.Delay(4000, _toastCts.Token);
            IsToastVisible = false;
        }
        catch (OperationCanceledException)
        {
            // Descartado manualmente — no hacer nada
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