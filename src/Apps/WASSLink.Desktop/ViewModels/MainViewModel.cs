using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
    public bool IsLibrarySelected => SelectedSection == "Biblioteca";
    public bool IsDownloadsSelected => SelectedSection == "Descargas";
    public bool IsSettingsSelected => SelectedSection == "Configuración";
    public bool IsDownloaderFormVisible => IsDownloaderSelected && !IsDownloadConfirmation;

    partial void OnSelectedSectionChanged(string value)
    {
        OnPropertyChanged(nameof(IsDownloaderSelected));
        OnPropertyChanged(nameof(IsSearchSelected));
        OnPropertyChanged(nameof(IsLibrarySelected));
        OnPropertyChanged(nameof(IsDownloadsSelected));
        OnPropertyChanged(nameof(IsSettingsSelected));
        OnPropertyChanged(nameof(IsDownloaderFormVisible));
        OnPropertyChanged(nameof(ActiveEngineLabel));
        OnPropertyChanged(nameof(EngineStatus));
        if (value == "Biblioteca")
            RefreshLibrary();
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

    [ObservableProperty]
    private ObservableCollection<DownloadHistoryItem> filteredDownloadHistory = new();

    [ObservableProperty]
    private string downloadTypeFilter = "Todos";

    [ObservableProperty]
    private string downloadDateFilter = "Todas";

    [ObservableProperty]
    private int downloadSpeedLimitKbps;

    [ObservableProperty]
    private string accentColor = "#4F46E5";

    [ObservableProperty]
    private string libraryStatus = "Contenido físico de la carpeta de descargas.";

    [ObservableProperty]
    private ObservableCollection<LocalFileItem> libraryFiles = new();

    [ObservableProperty]
    private ObservableCollection<LocalFileItem> localSearchResults = new();

    [ObservableProperty]
    private bool searchImages;

    [ObservableProperty]
    private bool searchAudio;

    [ObservableProperty]
    private bool searchVideo;

    [ObservableProperty]
    private bool searchCompressed;

    [ObservableProperty]
    private bool searchIso;

    [ObservableProperty]
    private bool isAnalyzing;

    [ObservableProperty]
    private bool isDownloadConfirmation;

    [ObservableProperty]
    private string confirmationTitle = string.Empty;

    [ObservableProperty]
    private string confirmationDetails = string.Empty;

    [ObservableProperty]
    private string confirmationOutput = string.Empty;

    [ObservableProperty]
    private string settingsMessage = string.Empty;

    public bool HasDownloadHistory => DownloadHistory.Count > 0;
    public bool ShowNoFilteredDownloads => HasDownloadHistory && !HasFilteredDownloads;
    public bool HasLibraryFiles => LibraryFiles.Count > 0;
    public bool HasSearchResults => LocalSearchResults.Count > 0;
    public int SelectedDownloadCount => DownloadHistory.Count(item => item.IsSelected);
    public bool HasSelectedDownloads => SelectedDownloadCount > 0;
    public bool HasFilteredDownloads => FilteredDownloadHistory.Count > 0;
    public bool CanPauseSelectedDownloads => DownloadHistory.Any(item =>
        item.IsSelected && item.Engine == "HTTP Directo (Streaming)" &&
        (item.IsPaused || item.Status == "Iniciando" || item.Status == "Descargando por HTTP Directo..."));
    public IReadOnlyList<string> DownloadTypeFilters { get; } = ["Todos", "Multimedia", "Archivos"];
    public IReadOnlyList<string> DownloadDateFilters { get; } = ["Todas", "Hoy", "Últimos 7 días"];
    public IReadOnlyList<string> AccentColorOptions { get; } = ["#4F46E5", "#0891B2", "#059669", "#D97706"];

    partial void OnDownloadTypeFilterChanged(string value) => RefreshFilteredDownloads();
    partial void OnDownloadDateFilterChanged(string value) => RefreshFilteredDownloads();
    partial void OnDownloadHistoryChanged(ObservableCollection<DownloadHistoryItem> value)
    {
        RefreshFilteredDownloads();
        OnPropertyChanged(nameof(HasDownloadHistory));
        OnPropertyChanged(nameof(ShowNoFilteredDownloads));
    }
    partial void OnIsDownloadConfirmationChanged(bool value) =>
        OnPropertyChanged(nameof(IsDownloaderFormVisible));
    partial void OnSearchImagesChanged(bool value) => RefreshLocalSearchResults();
    partial void OnSearchAudioChanged(bool value) => RefreshLocalSearchResults();
    partial void OnSearchVideoChanged(bool value) => RefreshLocalSearchResults();
    partial void OnSearchCompressedChanged(bool value) => RefreshLocalSearchResults();
    partial void OnSearchIsoChanged(bool value) => RefreshLocalSearchResults();
    partial void OnDownloadSpeedLimitKbpsChanged(int value)
    {
        if (value < 0)
            DownloadSpeedLimitKbps = 0;
        else if (value > 1_000_000)
            DownloadSpeedLimitKbps = 1_000_000;
    }

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

    private void OnDownloadHistoryItemPropertyChanged(object? sender, PropertyChangedEventArgs args)
    {
        if (args.PropertyName == nameof(DownloadHistoryItem.IsSelected))
        {
            OnPropertyChanged(nameof(SelectedDownloadCount));
            OnPropertyChanged(nameof(HasSelectedDownloads));
            OnPropertyChanged(nameof(CanPauseSelectedDownloads));
        }
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
    private void OpenLibrary() => SelectedSection = "Biblioteca";

    [RelayCommand]
    private void RefreshLibrary()
    {
        try
        {
            Directory.CreateDirectory(OutputPath);
            LibraryFiles = new ObservableCollection<LocalFileItem>(
                Directory.EnumerateFiles(OutputPath, "*", SearchOption.TopDirectoryOnly)
                    .Select(CreateLocalFileItem)
                    .OrderByDescending(file => file.Modified));
            LibraryStatus = $"{LibraryFiles.Count} archivo(s) en {OutputPath}";
            OnPropertyChanged(nameof(HasLibraryFiles));
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            LibraryStatus = $"No se pudo leer la biblioteca: {ex.Message}";
            LibraryFiles.Clear();
            OnPropertyChanged(nameof(HasLibraryFiles));
        }
    }

    [RelayCommand]
    private void SearchLocalFiles()
    {
        var term = SearchQuery.Trim();
        try
        {
            Directory.CreateDirectory(OutputPath);
            var files = Directory.EnumerateFiles(OutputPath, "*", SearchOption.TopDirectoryOnly)
                .Select(CreateLocalFileItem)
                .Where(file => string.IsNullOrWhiteSpace(term) ||
                               file.Name.Contains(term, StringComparison.OrdinalIgnoreCase))
                .Where(MatchesSelectedCategories)
                .OrderByDescending(file => file.Modified);
            LocalSearchResults = new ObservableCollection<LocalFileItem>(files);
            SearchResultMessage = $"{LocalSearchResults.Count} archivo(s) encontrados en la carpeta local.";
            OnPropertyChanged(nameof(HasSearchResults));
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            SearchResultMessage = $"No se pudo buscar en la biblioteca: {ex.Message}";
            LocalSearchResults.Clear();
            OnPropertyChanged(nameof(HasSearchResults));
        }
    }

    private bool MatchesSelectedCategories(LocalFileItem file)
    {
        if (!(SearchImages || SearchAudio || SearchVideo || SearchCompressed || SearchIso))
            return true;

        return file.Category switch
        {
            "Imagen" => SearchImages,
            "Audio" => SearchAudio,
            "Vídeo" => SearchVideo,
            "Archivo comprimido" => SearchCompressed,
            "ISO" => SearchIso,
            _ => false
        };
    }

    private static LocalFileItem CreateLocalFileItem(string path)
    {
        var info = new FileInfo(path);
        var category = GetFileCategory(info.Extension);
        return new LocalFileItem
        {
            Name = info.Name,
            Path = info.FullName,
            Category = category,
            Size = FormatFileSize(info.Length),
            Modified = info.LastWriteTime
        };
    }

    private static string GetFileCategory(string extension) => extension.ToLowerInvariant() switch
    {
        ".png" or ".jpg" or ".jpeg" or ".gif" or ".webp" or ".bmp" => "Imagen",
        ".mp3" or ".m4a" or ".wav" or ".flac" or ".aac" or ".opus" => "Audio",
        ".mp4" or ".mkv" or ".avi" or ".mov" or ".webm" => "Vídeo",
        ".zip" or ".rar" or ".7z" or ".tar" or ".gz" => "Archivo comprimido",
        ".iso" => "ISO",
        _ => "Archivo"
    };

    private static string FormatFileSize(long size)
    {
        string[] units = ["B", "KB", "MB", "GB"];
        var value = (double)size;
        var unit = 0;
        while (value >= 1024 && unit < units.Length - 1)
        {
            value /= 1024;
            unit++;
        }
        return $"{value:0.##} {units[unit]}";
    }

    private void RefreshLocalSearchResults()
    {
        if (!Uri.TryCreate(SearchQuery, UriKind.Absolute, out _))
            SearchLocalFiles();
    }

    private void RefreshFilteredDownloads()
    {
        var now = DateTime.Now;
        FilteredDownloadHistory = new ObservableCollection<DownloadHistoryItem>(
            DownloadHistory.Where(item =>
            {
                var extension = Path.GetExtension(item.Output).ToLowerInvariant();
                var isMultimedia = extension is ".mp3" or ".m4a" or ".wav" or ".flac" or
                    ".aac" or ".opus" or ".mp4" or ".mkv" or ".webm" or ".avi" or ".mov";
                var typeMatches = DownloadTypeFilter == "Todos" ||
                                  (DownloadTypeFilter == "Multimedia" ? isMultimedia : !isMultimedia);
                var dateMatches = DownloadDateFilter switch
                {
                    "Hoy" => item.CreatedAt.Date == now.Date,
                    "Últimos 7 días" => item.CreatedAt >= now.AddDays(-7),
                    _ => true
                };
                return typeMatches && dateMatches;
            }));
        OnPropertyChanged(nameof(HasFilteredDownloads));
        OnPropertyChanged(nameof(ShowNoFilteredDownloads));
    }

    [RelayCommand]
    private void DeleteSelectedDownloads()
    {
        foreach (var item in DownloadHistory.Where(item => item.IsSelected &&
                     !item.Status.Equals("Iniciando", StringComparison.OrdinalIgnoreCase) &&
                     !item.Status.StartsWith("Descargando", StringComparison.OrdinalIgnoreCase) &&
                     item.Status != "Procesando con FFmpeg...").ToList())
        {
            item.PropertyChanged -= OnDownloadHistoryItemPropertyChanged;
            DownloadHistory.Remove(item);
        }

        RefreshFilteredDownloads();
        OnPropertyChanged(nameof(HasDownloadHistory));
        OnPropertyChanged(nameof(ShowNoFilteredDownloads));
        OnPropertyChanged(nameof(SelectedDownloadCount));
        OnPropertyChanged(nameof(HasSelectedDownloads));
    }

    [RelayCommand]
    private void PauseSelectedDownloads()
    {
        var pausable = DownloadHistory.Where(item => item.IsSelected &&
            item.Engine == "HTTP Directo (Streaming)" &&
            (item.IsPaused || item.Status == "Iniciando" || item.Status == "Descargando por HTTP Directo...")).ToList();
        foreach (var item in pausable)
            item.IsPaused = !item.IsPaused;

        StatusMessage = pausable.Count > 0
            ? "Se alternó la pausa de las descargas HTTP seleccionadas. yt-dlp no admite pausa por lote."
            : "No hay descargas HTTP activas seleccionadas que puedan pausarse.";
        OnPropertyChanged(nameof(CanPauseSelectedDownloads));
    }

    [RelayCommand]
    private void OpenDownloadedLocation(DownloadHistoryItem item)
    {
        var path = File.Exists(item.Output) ? Path.GetDirectoryName(item.Output) : item.Output;
        if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
        {
            StatusMessage = "La ubicación de este elemento ya no existe.";
            return;
        }

        try
        {
            Process.Start(new ProcessStartInfo { FileName = path, UseShellExecute = true });
        }
        catch (Exception ex)
        {
            StatusMessage = $"No se pudo abrir la ubicación: {ex.Message}";
        }
    }

    [RelayCommand]
    private void OpenDownloadedWith(DownloadHistoryItem item)
    {
        if (!File.Exists(item.Output))
        {
            StatusMessage = "El archivo descargado no está disponible.";
            return;
        }

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "rundll32.exe",
                Arguments = $"shell32.dll,OpenAs_RunDLL \"{item.Output}\"",
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            StatusMessage = $"No se pudo abrir el selector de aplicaciones: {ex.Message}";
        }
    }

    [RelayCommand]
    private void OpenLocalFile(LocalFileItem file)
    {
        if (!File.Exists(file.Path))
        {
            LibraryStatus = "El archivo ya no existe.";
            return;
        }

        try
        {
            Process.Start(new ProcessStartInfo { FileName = file.Path, UseShellExecute = true });
        }
        catch (Exception ex)
        {
            LibraryStatus = $"No se pudo abrir el archivo: {ex.Message}";
        }
    }

    [RelayCommand]
    private void SaveSettings()
    {
        if (string.IsNullOrWhiteSpace(OutputPath))
            SettingsMessage = "La carpeta de destino no puede estar vacía.";
        else if (!Path.IsPathFullyQualified(OutputPath))
            SettingsMessage = "La carpeta de destino debe ser una ruta absoluta.";
        else
            SettingsMessage = DownloadSpeedLimitKbps <= 0
                ? "Preferencias aplicadas. Límite de velocidad: sin límite."
                : $"Preferencias aplicadas. Límite: {DownloadSpeedLimitKbps} KB/s.";
    }

    [RelayCommand]
    private void PasteAnotherLink()
    {
        DownloadUrl = string.Empty;
        SelectedFormatOption = null;
        IsDownloadConfirmation = false;
        SelectedSection = "Downloader";
    }

    [RelayCommand]
    private async Task SearchAsync()
    {
        var url = SearchQuery.Trim();
        if (string.IsNullOrWhiteSpace(url))
        {
            SearchResultMessage = "Ingresa un enlace para iniciar la consulta.";
            return;
        }

        if (!Uri.TryCreate(url, UriKind.Absolute, out _))
        {
            SearchLocalFiles();
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

    [RelayCommand]
    private async Task AnalyzeAsync()
    {
        if (string.IsNullOrWhiteSpace(DownloadUrl))
        {
            StatusMessage = "Ingresa una URL antes de analizar.";
            return;
        }

        var selectedProvider = GetProviderForUrl(DownloadUrl);
        if (selectedProvider is null)
        {
            StatusMessage = "La URL ingresada no es válida o no está soportada.";
            return;
        }

        IsAnalyzing = true;
        try
        {
            await Task.Delay(120);
            if (selectedProvider is DirectHttpDownloadService)
            {
                SelectedFormatOption = null;
                IsAnalyzing = false;
                await StartDownloadWithFormatAsync(null);
                return;
            }

            await OpenFormatModalAsync();
        }
        finally
        {
            IsAnalyzing = false;
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
            VideoFormatHasAudio: format?.HasAudio ?? false,
            MaxSpeedBytesPerSecond: DownloadSpeedLimitKbps * 1024L
        );

        var selectedProvider = GetProviderForUrl(request.Url)!;
        var historyItem = new DownloadHistoryItem
        {
            SourceUrl = request.Url,
            Engine = selectedProvider.Name == "yt-dlp" ? "yt-dlp & FFmpeg" : selectedProvider.Name,
            Status = "Iniciando",
            Output = OutputPath
        };
        var resumeSignal = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        resumeSignal.TrySetResult();
        historyItem.PauseDownload = pause =>
        {
            if (pause)
            {
                Interlocked.Exchange(
                    ref resumeSignal,
                    new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously));
                historyItem.Status = "Pausada";
            }
            else
            {
                Volatile.Read(ref resumeSignal).TrySetResult();
                historyItem.Status = "Descargando por HTTP Directo...";
            }
        };
        request = request with
        {
            WaitIfPausedAsync = token => Volatile.Read(ref resumeSignal).Task.WaitAsync(token)
        };
        historyItem.PropertyChanged += OnDownloadHistoryItemPropertyChanged;
        DownloadHistory.Insert(0, historyItem);
        RefreshFilteredDownloads();
        OnPropertyChanged(nameof(HasDownloadHistory));
        OnPropertyChanged(nameof(ShowNoFilteredDownloads));

        IProgress<DownloadProgress> progress = new Progress<DownloadProgress>(p =>
        {
            if (!historyItem.IsPaused)
                historyItem.Status = p.StatusMessage ?? "Descargando";
            if (p.Percentage >= 0)
                historyItem.Progress = p.Percentage;

            var isProcessingPhase = p.StatusMessage != null &&
                p.StatusMessage.Contains("FFmpeg", StringComparison.OrdinalIgnoreCase);

            if (isProcessingPhase)
            {
                historyItem.Status = p.StatusMessage!;
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
            var result = selectedProvider is DirectHttpDownloadService
                ? await selectedProvider.DownloadAsync(
                    request,
                    new Progress<DownloadProgress>(p =>
                    {
                        progress.Report(p);
                    }),
                    _cts.Token)
                : await selectedProvider.DownloadAsync(request, progress, _cts.Token);

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
                ConfirmationTitle = displayName;
                ConfirmationDetails = $"Descarga completada con {historyItem.Engine}.";
                ConfirmationOutput = LastDownloadedFile;
                IsDownloadConfirmation = true;
                SelectedSection = "Downloader";
                RefreshFilteredDownloads();
                OnPropertyChanged(nameof(HasDownloadHistory));
                OnPropertyChanged(nameof(ShowNoFilteredDownloads));
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