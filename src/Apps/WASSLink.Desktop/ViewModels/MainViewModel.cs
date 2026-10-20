using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WASSLink.Abstractions;
using WASSLink.Configuration;
using WASSLink.Desktop.Models;
using WASSLink.Desktop.Services;
using WASSLink.Download;

namespace WASSLink.Desktop.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly IDownloadProvider _downloadProvider;
    private readonly DirectHttpDownloadService? _directHttpProvider;
    private readonly IClipboardService _clipboardService;
    private readonly IFolderPickerService? _folderPickerService;
    private readonly MediaMetadataService _metadataService;
    private readonly DownloadQueueService _downloadQueue;
    private readonly IUserDataStore? _userDataStore;
    private readonly Dictionary<Guid, DownloadHistoryItem> _historyItemsByJob = [];
    private readonly Dictionary<Guid, DownloadJob> _jobsById = [];
    private CancellationTokenSource? _metadataCts;
    private CancellationTokenSource? _toastCts;

    public MainViewModel(
        IDownloadProvider downloadProvider,
        IClipboardService clipboardService,
        IFolderPickerService? folderPickerService = null,
        DownloadQueueService? downloadQueue = null,
        IUserDataStore? userDataStore = null)
    {
        _downloadProvider = downloadProvider ?? throw new ArgumentNullException(nameof(downloadProvider));
        _directHttpProvider = downloadProvider switch
        {
            CompositeDownloadProvider composite => composite.DirectHttpProvider,
            DirectHttpDownloadService directHttpProvider => directHttpProvider,
            _ => null
        };
        _clipboardService = clipboardService ?? throw new ArgumentNullException(nameof(clipboardService));
        _folderPickerService = folderPickerService;
        _metadataService = new MediaMetadataService();
        _downloadQueue = downloadQueue ?? new DownloadQueueService();
        _userDataStore = userDataStore;
        _downloadQueue.MaximumConcurrency = MaximumConcurrency;
        _downloadQueue.QueueChanged += OnDownloadQueueChanged;

        OutputPath = ResolveDefaultOutputPath();
        LoadUserData();
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
    private string selectedSection = "Descargar";

    public bool IsDownloaderSelected => SelectedSection == "Descargar";
    public bool IsSearchSelected => SelectedSection == "Buscador";
    public bool IsLibrarySelected => SelectedSection == "Biblioteca";
    public bool IsDownloadsSelected => SelectedSection == "Historial";
    public bool IsSettingsSelected => SelectedSection == "Configuración";
    public bool IsDownloaderFormVisible =>
        IsDownloaderSelected && !IsDownloadConfirmation && !IsDirectFilePreview;

    partial void OnSelectedSectionChanged(string value)
    {
        if (value != "Descargar")
        {
            IsDownloadConfirmation = false;
            IsDirectFilePreview = false;
            IsDuplicatePromptOpen = false;
            IsFormatModalOpen = false;
            _metadataCts?.Cancel();
        }
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
        OnPropertyChanged(nameof(CanDownloadSearchQuery));
        OnPropertyChanged(nameof(BingSearchUrl));
        CanSearchOnline = false;
        LocalSearchResults.Clear();
        OnPropertyChanged(nameof(HasSearchResults));
        SearchResultMessage = string.IsNullOrWhiteSpace(value)
            ? "Busca un archivo en tu carpeta de descargas."
            : CanDownloadSearchQuery
                ? "Enlace compatible. Pulsa «Consultar enlace» para analizarlo."
                : "Pulsa «Buscar archivos» para consultar la carpeta local.";
    }

    [ObservableProperty]
    private string searchResultMessage = "Busca un archivo en tu carpeta de descargas.";

    [ObservableProperty]
    private bool isSearching;

    [ObservableProperty]
    private bool canSearchOnline;

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
    private int maximumConcurrency = 2;

    [ObservableProperty]
    private string accentColor = "#4F46E5";

    [ObservableProperty]
    private string accentColorName = "Violeta";

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
    private bool isDirectFilePreview;

    [ObservableProperty]
    private string directFileUrl = string.Empty;

    [ObservableProperty]
    private string directFileName = string.Empty;

    [ObservableProperty]
    private string directFileType = string.Empty;

    [ObservableProperty]
    private string directFileSize = string.Empty;

    [ObservableProperty]
    private string directFileContentType = string.Empty;

    [ObservableProperty]
    private string confirmationTitle = string.Empty;

    [ObservableProperty]
    private string confirmationDetails = string.Empty;

    [ObservableProperty]
    private string confirmationOutput = string.Empty;

    [ObservableProperty]
    private bool isDuplicatePromptOpen;

    [ObservableProperty]
    private string duplicateFilePath = string.Empty;

    [ObservableProperty]
    private string settingsMessage = string.Empty;

    public bool HasDownloadHistory => DownloadHistory.Count > 0;
    public bool ShowNoFilteredDownloads => HasDownloadHistory && !HasFilteredDownloads;
    public bool HasLibraryFiles => LibraryFiles.Count > 0;
    public bool HasSearchResults => LocalSearchResults.Count > 0;
    public int SelectedDownloadCount => DownloadHistory.Count(item => item.IsSelected);
    public bool CanDownloadSearchQuery => GetProviderForUrl(SearchQuery.Trim()) is not null;
    public bool HasSelectedDownloads => SelectedDownloadCount > 0;
    public bool HasFilteredDownloads => FilteredDownloadHistory.Count > 0;
    public bool CanPauseSelectedDownloads => DownloadHistory.Any(item =>
        item.IsSelected && item.Engine == "HTTP Directo (Streaming)" &&
        (item.IsPaused || item.Status == "Iniciando" || item.Status == "Descargando por HTTP Directo..."));
    public IReadOnlyList<string> DownloadTypeFilters { get; } = ["Todos", "Multimedia", "Archivos"];
    public IReadOnlyList<string> DownloadDateFilters { get; } = ["Todas", "Hoy", "Últimos 7 días"];
    public IReadOnlyList<string> AccentColorOptions { get; } = ["Violeta", "Cian", "Esmeralda", "Ámbar"];
    public IReadOnlyList<int> ConcurrencyOptions { get; } = [1, 2, 3];
    public string BingSearchUrl => BuildBingSearchUrl();

    partial void OnDownloadTypeFilterChanged(string value) => RefreshFilteredDownloads();
    partial void OnDownloadDateFilterChanged(string value) => RefreshFilteredDownloads();
    partial void OnAccentColorNameChanged(string value)
    {
        AccentColor = value switch
        {
            "Cian" => "#0891B2",
            "Esmeralda" => "#059669",
            "Ámbar" => "#D97706",
            _ => "#4F46E5"
        };
    }
    partial void OnDownloadHistoryChanged(ObservableCollection<DownloadHistoryItem> value)
    {
        RefreshFilteredDownloads();
        OnPropertyChanged(nameof(HasDownloadHistory));
        OnPropertyChanged(nameof(ShowNoFilteredDownloads));
    }
    partial void OnIsDownloadConfirmationChanged(bool value) =>
        OnPropertyChanged(nameof(IsDownloaderFormVisible));
    partial void OnIsDirectFilePreviewChanged(bool value) =>
        OnPropertyChanged(nameof(IsDownloaderFormVisible));
    partial void OnSearchImagesChanged(bool value) => RefreshSearchFilters();
    partial void OnSearchAudioChanged(bool value) => RefreshSearchFilters();
    partial void OnSearchVideoChanged(bool value) => RefreshSearchFilters();
    partial void OnSearchCompressedChanged(bool value) => RefreshSearchFilters();
    partial void OnSearchIsoChanged(bool value) => RefreshSearchFilters();
    partial void OnDownloadSpeedLimitKbpsChanged(int value)
    {
        if (value < 0)
            DownloadSpeedLimitKbps = 0;
        else if (value > 1_000_000)
            DownloadSpeedLimitKbps = 1_000_000;
    }
    partial void OnMaximumConcurrencyChanged(int value)
    {
        var normalized = Math.Clamp(value, 1, 3);
        if (normalized != value)
            MaximumConcurrency = normalized;
        _downloadQueue.MaximumConcurrency = normalized;
        OnPropertyChanged(nameof(QueueSummary));
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

            if (url == AnalyzedDirectFileUrl && !string.IsNullOrEmpty(AnalyzedDirectFileEngine))
                return $"Motor: {AnalyzedDirectFileEngine}";

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
            if (url == AnalyzedDirectFileUrl && !string.IsNullOrEmpty(AnalyzedDirectFileEngine))
                return "Archivo directo analizado";
            return string.IsNullOrWhiteSpace(url)
                ? "Esperando un enlace"
                : _downloadProvider.CanHandle(url)
                    ? "Proveedor seleccionado para este enlace"
                    : "URL no compatible";
        }
    }

    partial void OnDownloadUrlChanged(string value)
    {
        if (!string.Equals(AnalyzedDirectFileUrl, value, StringComparison.Ordinal))
        {
            IsDirectFilePreview = false;
            AnalyzedDirectFileUrl = string.Empty;
            AnalyzedDirectFileEngine = string.Empty;
        }
        OnPropertyChanged(nameof(ActiveEngineLabel));
        OnPropertyChanged(nameof(EngineStatus));
        OnPropertyChanged(nameof(IsDownloaderFormVisible));
    }

    [ObservableProperty]
    private string analyzedDirectFileUrl = string.Empty;

    [ObservableProperty]
    private string analyzedDirectFileEngine = string.Empty;

    private void OnDownloadHistoryItemPropertyChanged(object? sender, PropertyChangedEventArgs args)
    {
        if (args.PropertyName == nameof(DownloadHistoryItem.IsSelected))
        {
            OnPropertyChanged(nameof(SelectedDownloadCount));
            OnPropertyChanged(nameof(HasSelectedDownloads));
            OnPropertyChanged(nameof(CanPauseSelectedDownloads));
        }
    }

    private void OnDownloadQueueChanged(object? sender, EventArgs args)
    {
        Dispatcher.UIThread.Post(() =>
        {
            IsDownloading = _downloadQueue.ActiveCount > 0;
            OnPropertyChanged(nameof(QueuedDownloadCount));
            OnPropertyChanged(nameof(QueueSummary));
        });
    }

    public int QueuedDownloadCount => DownloadHistory.Count(item =>
        item.Status.Equals("En cola", StringComparison.OrdinalIgnoreCase));
    public string QueueSummary =>
        $"Activas: {_downloadQueue.ActiveCount} · en cola: {QueuedDownloadCount} · límite: {MaximumConcurrency}";

    private void OnDownloadJobChanged(object? sender, EventArgs args)
    {
        if (sender is not DownloadJob job)
            return;

        Dispatcher.UIThread.Post(() =>
        {
            if (!_historyItemsByJob.TryGetValue(job.Id, out var item))
                return;

            item.Status = job.State switch
            {
                DownloadJobState.Pending => "En cola",
                DownloadJobState.Downloading when item.IsPaused => "Pausada",
                DownloadJobState.Downloading => job.LastProgress?.StatusMessage ?? "Descargando",
                DownloadJobState.Paused => "Pausada",
                DownloadJobState.Retrying => $"Reintentando ({job.Attempt}/3)",
                DownloadJobState.Completed => "Completada",
                DownloadJobState.Skipped => "Omitida: el archivo ya existía",
                DownloadJobState.Cancelled => "Cancelada",
                _ => item.Status
            };

            if (job.LastProgress is { } progress)
            {
                if (progress.Percentage >= 0)
                    item.Progress = progress.Percentage;

                var details = new List<string>();
                if (!string.IsNullOrWhiteSpace(progress.Speed))
                    details.Add(progress.Speed);
                if (!string.IsNullOrWhiteSpace(progress.Eta))
                    details.Add($"ETA {progress.Eta}");
                if (!string.IsNullOrWhiteSpace(progress.TotalSize))
                    details.Add(progress.TotalSize);
                item.ProgressDetails = string.Join(" · ", details);
            }

            OnPropertyChanged(nameof(QueuedDownloadCount));
            OnPropertyChanged(nameof(QueueSummary));
            RefreshFilteredDownloads();
        });
    }

    private IDownloadProvider? GetProviderForUrl(string url)
    {
        return _downloadProvider is CompositeDownloadProvider composite
            ? composite.GetProviderForUrl(url)
            : _downloadProvider.CanHandle(url) ? _downloadProvider : null;
    }

    private DirectHttpDownloadService GetDirectHttpProvider() =>
        _directHttpProvider ?? throw new InvalidOperationException("El proveedor HTTP Directo no está disponible.");

    [ObservableProperty]
    private string outputPath;

    [ObservableProperty]
    private string statusMessage = "Listo para descargar.";

    [ObservableProperty]
    private bool isDownloading;

    [ObservableProperty]
    private string lastDownloadedFile = string.Empty;

    // ─── Fases de progreso (para animaciones) ────────────────────────────────────

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
        CanSearchOnline = false;
        if (CanDownloadSearchQuery)
        {
            SearchResultMessage = "Este enlace es compatible. Pulsa «Consultar enlace» para analizarlo.";
            return;
        }

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
            if (LocalSearchResults.Count > 0)
            {
                CanSearchOnline = false;
                SearchResultMessage = $"{LocalSearchResults.Count} archivo(s) encontrados en la carpeta local.";
            }
            else if (!string.IsNullOrWhiteSpace(term) || HasSelectedSearchCategories)
            {
                CanSearchOnline = true;
                SearchResultMessage = "No encontré archivos parecidos en tu carpeta local. ¿Quieres abrir Bing y buscar en línea?";
            }
            else
            {
                CanSearchOnline = false;
                SearchResultMessage = "Tu carpeta de descargas todavía no contiene archivos.";
            }

            OnPropertyChanged(nameof(HasSearchResults));
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            SearchResultMessage = $"No se pudo buscar en la biblioteca: {ex.Message}";
            LocalSearchResults.Clear();
            CanSearchOnline = false;
            OnPropertyChanged(nameof(HasSearchResults));
        }
    }

    private bool HasSelectedSearchCategories =>
        SearchImages || SearchAudio || SearchVideo || SearchCompressed || SearchIso;

    private string BuildBingSearchUrl()
    {
        var query = SearchQuery.Trim();
        var categories = new List<string>();

        if (SearchImages)
            categories.AddRange(["images", "photos", "pictures"]);
        if (SearchAudio)
            categories.AddRange(["audio", "music", "MP3", "FLAC"]);
        if (SearchVideo)
            categories.AddRange(["video", "MP4", "MKV"]);
        if (SearchCompressed)
            categories.AddRange(["archive", "ZIP", "RAR", "7z"]);
        if (SearchIso)
            categories.AddRange(["ISO", "disk image"]);

        if (categories.Count > 0)
            query = string.IsNullOrWhiteSpace(query)
                ? $"({string.Join(" OR ", categories)})"
                : $"{query} ({string.Join(" OR ", categories)})";

        var imageOnly = SearchImages && !(SearchAudio || SearchVideo || SearchCompressed || SearchIso);
        var searchPath = imageOnly ? "images/search" : "search";
        return $"https://www.bing.com/{searchPath}?q={Uri.EscapeDataString(query)}";
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

    private void RefreshSearchFilters()
    {
        OnPropertyChanged(nameof(BingSearchUrl));
        RefreshLocalSearchResults();
    }

    [RelayCommand]
    private void SearchOnline()
    {
        if (!CanSearchOnline || (string.IsNullOrWhiteSpace(SearchQuery) && !HasSelectedSearchCategories))
        {
            SearchResultMessage = "Escribe qué quieres buscar o selecciona una categoría.";
            CanSearchOnline = false;
            return;
        }

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = BingSearchUrl,
                UseShellExecute = true
            });
            CanSearchOnline = false;
            SearchResultMessage = "Bing se abrió en tu navegador predeterminado.";
        }
        catch (Exception ex)
        {
            SearchResultMessage = $"No se pudo abrir Bing: {ex.Message}";
        }
    }

    [RelayCommand]
    private void DismissOnlineSearch()
    {
        CanSearchOnline = false;
        SearchResultMessage = "No se encontraron archivos locales. Puedes cambiar el término o los filtros.";
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
        var selectedItems = DownloadHistory.Where(item => item.IsSelected).ToList();
        var activeItems = selectedItems.Where(item =>
            item.JobId is { } jobId && _jobsById.ContainsKey(jobId)).ToList();

        foreach (var item in selectedItems.Except(activeItems))
        {
            item.PropertyChanged -= OnDownloadHistoryItemPropertyChanged;
            DownloadHistory.Remove(item);
        }

        if (activeItems.Count > 0)
            StatusMessage = "Las descargas activas o pendientes no se quitaron del historial.";

        RefreshFilteredDownloads();
        OnPropertyChanged(nameof(HasDownloadHistory));
        OnPropertyChanged(nameof(ShowNoFilteredDownloads));
        OnPropertyChanged(nameof(SelectedDownloadCount));
        OnPropertyChanged(nameof(HasSelectedDownloads));
        PersistHistory();
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
        {
            try
            {
                _userDataStore?.SaveSettings(new GowLinkSettings(
                    OutputPath,
                    DownloadSpeedLimitKbps,
                    AccentColorName,
                    MaximumConcurrency));
                SettingsMessage = DownloadSpeedLimitKbps <= 0
                    ? $"Preferencias guardadas. Sin límite de velocidad; máximo {MaximumConcurrency} descargas simultáneas."
                    : $"Preferencias guardadas. Límite: {DownloadSpeedLimitKbps} KB/s; máximo {MaximumConcurrency} descargas simultáneas.";
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or System.Text.Json.JsonException)
            {
                SettingsMessage = $"No se pudieron guardar las preferencias: {ex.Message}";
            }
        }
    }

    private void LoadUserData()
    {
        if (_userDataStore is null)
            return;

        try
        {
            if (_userDataStore.LoadSettings() is { } settings)
            {
                if (Path.IsPathFullyQualified(settings.OutputPath))
                    OutputPath = settings.OutputPath;
                DownloadSpeedLimitKbps = Math.Max(0, settings.DownloadSpeedLimitKbps);
                AccentColorName = AccentColorOptions.Contains(settings.AccentColorName)
                    ? settings.AccentColorName
                    : "Violeta";
                MaximumConcurrency = Math.Clamp(settings.MaximumConcurrency, 1, 3);
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or System.Text.Json.JsonException)
        {
            SettingsMessage = $"No se pudieron leer las preferencias; se usarán valores predeterminados: {ex.Message}";
        }

        try
        {
            foreach (var record in _userDataStore.LoadHistory())
            {
                DownloadHistory.Add(new DownloadHistoryItem
                {
                    SourceUrl = record.SourceUrl,
                    Engine = record.Engine,
                    Status = record.Status,
                    Progress = record.Progress,
                    Output = record.Output,
                    CreatedAt = record.CreatedAt
                });
            }

            RefreshFilteredDownloads();
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or System.Text.Json.JsonException)
        {
            SettingsMessage = $"No se pudo leer el historial guardado: {ex.Message}";
        }
    }

    private void PersistHistory()
    {
        if (_userDataStore is null)
            return;

        var terminalStatuses = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Completada",
            "Omitida: el archivo ya existía",
            "Ya existía",
            "Cancelada"
        };
        var records = DownloadHistory
            .Where(item => terminalStatuses.Contains(item.Status) ||
                           item.Status.StartsWith("Error:", StringComparison.OrdinalIgnoreCase))
            .Select(item => new DownloadHistoryRecord(
                SanitizeHistoryUrl(item.SourceUrl),
                item.Engine,
                item.Status,
                item.Progress,
                item.Output,
                item.CreatedAt))
            .ToList();

        try
        {
            _userDataStore.SaveHistory(records);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or System.Text.Json.JsonException)
        {
            SettingsMessage = $"No se pudo guardar el historial: {ex.Message}";
        }
    }

    private static string SanitizeHistoryUrl(string value)
    {
        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri))
            return value;

        var sanitized = new UriBuilder(uri)
        {
            Query = string.Empty,
            Fragment = string.Empty
        };
        return sanitized.Uri.AbsoluteUri.TrimEnd('/');
    }

    [RelayCommand]
    private void PasteAnotherLink()
    {
        DownloadUrl = string.Empty;
        SelectedFormatOption = null;
        IsDirectFilePreview = false;
        IsDownloadConfirmation = false;
        SelectedSection = "Descargar";
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

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            SearchLocalFiles();
            return;
        }

        DownloadUrl = url;
        SelectedSection = "Descargar";
        await AnalyzeAsync();
        SearchResultMessage = StatusMessage;
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
        SelectedSection = "Descargar";
    }

    [RelayCommand]
    private void OpenDownloads() => SelectedSection = "Historial";

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
        IsDirectFilePreview = false;
        DirectFileUrl = string.Empty;
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
            if (SelectedFormatOption is not null)
                LoggerService.Write(
                    $"MEDIA FORMAT SELECTED URL=\"{DownloadUrl}\" ID=\"{SelectedFormatOption.FormatId}\" " +
                    $"TYPE=\"{SelectedFormatOption.MediaType}\" LABEL=\"{SelectedFormatOption.QualityLabel}\"");
        }
        catch (OperationCanceledException)
        {
            FormatLoadError = "Tiempo de espera agotado al obtener los metadatos.";
            StatusMessage = "Análisis cancelado.";
            LoggerService.Write($"MEDIA ANALYZE CANCELLED URL=\"{DownloadUrl}\"");
        }
        catch (Exception ex)
        {
            FormatLoadError = $"Error al analizar: {ex.Message}";
            StatusMessage = "No se pudieron obtener los formatos.";
            LoggerService.WriteException(ex, $"MEDIA ANALYZE FAILED URL=\"{DownloadUrl}\"");
        }
        finally
        {
            IsLoadingFormats = false;
        }
    }

    [RelayCommand]
    private async Task AnalyzeAsync()
    {
        var url = DownloadUrl.Trim();
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            StatusMessage = "Ingresa un enlace HTTP o HTTPS válido (por ejemplo, https://sitio.com/archivo.zip).";
            LoggerService.Write($"ANALYZE REJECTED URL=\"{DownloadUrl}\" REASON=\"Invalid HTTP/HTTPS URL\"");
            return;
        }

        IsDirectFilePreview = false;
        IsAnalyzing = true;
        LoggerService.Write($"ANALYZE START URL=\"{uri}\"");
        try
        {
            await Task.Delay(120);
            var knownDirectLink = _directHttpProvider?.CanHandle(url) == true;
            if (_directHttpProvider is not null)
            {
                try
                {
                    var fileInfo = await _directHttpProvider.AnalyzeAsync(url);
                    LoggerService.Write(
                        $"ANALYZE HTTP RESULT URL=\"{uri}\" FILE=\"{fileInfo.FileName}\" " +
                        $"TYPE=\"{fileInfo.ContentType}\" KIND=\"{fileInfo.FileType}\" " +
                        $"SIZE={fileInfo.ContentLength?.ToString() ?? "unknown"} DOWNLOADABLE={fileInfo.IsDownloadable}");

                    if (fileInfo.IsDownloadable)
                    {
                        DirectFileUrl = url;
                        DirectFileName = fileInfo.FileName;
                        DirectFileType = fileInfo.FileType;
                        DirectFileSize = fileInfo.FormattedSize;
                        DirectFileContentType = fileInfo.ContentType;
                        AnalyzedDirectFileUrl = url;
                        AnalyzedDirectFileEngine = "HTTP Directo (Streaming)";
                        IsDirectFilePreview = true;
                        StatusMessage = $"Enlace verificado: {DirectFileName} · {DirectFileType}. Confirma para descargar.";
                        return;
                    }

                    if (knownDirectLink)
                    {
                        StatusMessage = "El servidor devolvió una página web en lugar del archivo esperado.";
                        LoggerService.Write($"ANALYZE REJECTED URL=\"{uri}\" REASON=\"Expected file but received HTML\"");
                        return;
                    }
                }
                catch (Exception ex) when (ex is HttpRequestException or IOException or TaskCanceledException)
                {
                    LoggerService.WriteException(ex, $"ANALYZE HTTP PROBE FAILED URL=\"{uri}\"");
                    if (knownDirectLink)
                    {
                        StatusMessage = $"No se pudo verificar el enlace del archivo: {ex.Message}";
                        return;
                    }
                }
            }

            if (GetProviderForUrl(url) is null)
            {
                StatusMessage = "El enlace no es válido o el tipo de contenido no está soportado.";
                LoggerService.Write($"ANALYZE REJECTED URL=\"{uri}\" REASON=\"No compatible provider\"");
                return;
            }

            await OpenFormatModalAsync();
        }
        catch (Exception ex)
        {
            LoggerService.WriteException(ex, $"ANALYZE FAILED URL=\"{uri}\"");
            StatusMessage = $"No se pudo analizar el enlace: {ex.Message}";
        }
        finally
        {
            IsAnalyzing = false;
        }
    }

    [RelayCommand]
    private async Task ConfirmDirectFileDownloadAsync()
    {
        if (!IsDirectFilePreview || string.IsNullOrWhiteSpace(DirectFileUrl))
            return;

        var existingPath = Path.Combine(OutputPath, DirectFileName);
        if (File.Exists(existingPath))
        {
            DuplicateFilePath = existingPath;
            IsDuplicatePromptOpen = true;
            return;
        }

        IsDirectFilePreview = false;
        await StartDownloadWithFormatAsync(null, forceDirectHttp: true, FileConflictAction.CreateCopy);
    }

    [RelayCommand]
    private async Task SaveDuplicateAsCopyAsync() =>
        await ResolveDirectDuplicateAsync(FileConflictAction.CreateCopy);

    [RelayCommand]
    private async Task OverwriteDuplicateAsync() =>
        await ResolveDirectDuplicateAsync(FileConflictAction.Overwrite);

    [RelayCommand]
    private async Task SkipDuplicateAsync() =>
        await ResolveDirectDuplicateAsync(FileConflictAction.Skip);

    [RelayCommand]
    private void CloseDuplicatePrompt() => IsDuplicatePromptOpen = false;

    private async Task ResolveDirectDuplicateAsync(FileConflictAction action)
    {
        if (!IsDuplicatePromptOpen || string.IsNullOrWhiteSpace(DirectFileUrl))
            return;

        IsDuplicatePromptOpen = false;
        IsDirectFilePreview = false;
        if (action == FileConflictAction.Skip)
        {
            var historyItem = new DownloadHistoryItem
            {
                SourceUrl = SanitizeHistoryUrl(DirectFileUrl),
                Engine = "HTTP Directo (Streaming)",
                Status = "Omitida: el archivo ya existía",
                Output = DuplicateFilePath
            };
            DownloadHistory.Insert(0, historyItem);
            StatusMessage = $"Descarga omitida; se conservó {Path.GetFileName(DuplicateFilePath)}.";
            ConfirmationTitle = Path.GetFileName(DuplicateFilePath);
            ConfirmationDetails = "No se descargó el archivo y el elemento existente se conservó.";
            ConfirmationOutput = DuplicateFilePath;
            IsDownloadConfirmation = SelectedSection == "Descargar";
            PersistHistory();
            RefreshFilteredDownloads();
            return;
        }

        await StartDownloadWithFormatAsync(null, forceDirectHttp: true, action);
    }

    [RelayCommand]
    private void CancelDirectFilePreview()
    {
        IsDuplicatePromptOpen = false;
        IsDirectFilePreview = false;
        StatusMessage = "Descarga no iniciada.";
    }

    [RelayCommand]
    private void CloseDownloadConfirmation() => IsDownloadConfirmation = false;

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

    private async Task StartDownloadWithFormatAsync(
        MediaFormatOption? format,
        bool forceDirectHttp = false,
        FileConflictAction conflictAction = FileConflictAction.CreateCopy)
    {
        if (string.IsNullOrWhiteSpace(DownloadUrl))
        {
            StatusMessage = "Ingresa una URL válida.";
            return;
        }

        var selectedProvider = forceDirectHttp
            ? _directHttpProvider
            : GetProviderForUrl(DownloadUrl);
        if (selectedProvider is null)
        {
            StatusMessage = "La URL ingresada no es válida o no está soportada.";
            return;
        }

        IsToastVisible = false;
        StatusMessage = "Iniciando descarga...";

        var isDirectFile = selectedProvider == _directHttpProvider;
        var mediaType = isDirectFile
            ? DownloadMediaType.File
            : format?.MediaType ?? DownloadMediaType.Video;
        var ext = isDirectFile
            ? "bin"
            : format?.MediaType == DownloadMediaType.Audio
                ? format.Extension.ToLowerInvariant()
                : "mp4";

        var request = new DownloadRequest(
            Url: forceDirectHttp ? DirectFileUrl.Trim() : DownloadUrl.Trim(),
            OutputPath: OutputPath,
            MediaType: mediaType,
            Format: ext,
            FormatId: format?.FormatId,
            VideoFormatHasAudio: format?.HasAudio ?? false,
            MaxSpeedBytesPerSecond: DownloadSpeedLimitKbps * 1024L,
            ConflictAction: conflictAction
        );

        var historyItem = new DownloadHistoryItem
        {
            SourceUrl = SanitizeHistoryUrl(request.Url),
            Engine = selectedProvider.Name == "yt-dlp" ? "yt-dlp & FFmpeg" : selectedProvider.Name,
            Status = "En cola",
            Output = OutputPath
        };
        var job = new DownloadJob(selectedProvider, request);
        historyItem.JobId = job.Id;
        _historyItemsByJob[job.Id] = historyItem;
        _jobsById[job.Id] = job;
        job.Changed += OnDownloadJobChanged;
        historyItem.PauseDownload = job.SetPaused;
        LoggerService.Write(
            $"DOWNLOAD REQUEST URL=\"{request.Url}\" PROVIDER=\"{selectedProvider.Name}\" " +
            $"MEDIA=\"{request.MediaType}\" FORMAT=\"{request.FormatId ?? request.Format}\" OUTPUT=\"{request.OutputPath}\"");
        historyItem.PropertyChanged += OnDownloadHistoryItemPropertyChanged;
        DownloadHistory.Insert(0, historyItem);
        RefreshFilteredDownloads();
        OnPropertyChanged(nameof(HasDownloadHistory));
        OnPropertyChanged(nameof(ShowNoFilteredDownloads));

        try
        {
            _downloadQueue.Enqueue(job);
            var result = await job.Completion;

            if (result.Success)
            {
                LastDownloadedFile = result.OutputPath ?? OutputPath;
                var fileName = Path.GetFileName(LastDownloadedFile);
                var displayName = string.IsNullOrWhiteSpace(fileName) ? "Archivo descargado" : fileName;

                StatusMessage = result.WasAlreadyPresent
                    ? $"Descarga omitida; se conservó el archivo existente: {displayName}"
                    : $"¡Completado!: {displayName}";
                historyItem.Status = result.WasAlreadyPresent
                    ? "Omitida: el archivo ya existía"
                    : "Completada";
                if (!result.WasAlreadyPresent)
                    historyItem.Progress = 100;
                historyItem.Output = LastDownloadedFile;
                ConfirmationTitle = displayName;
                ConfirmationDetails = result.WasAlreadyPresent
                    ? $"El archivo ya estaba en destino y no se descargó de nuevo ({historyItem.Engine})."
                    : $"Descarga completada con {historyItem.Engine}.";
                ConfirmationOutput = LastDownloadedFile;
                var showConfirmation = _downloadQueue.ActiveCount == 0 &&
                                       QueuedDownloadCount == 0 &&
                                       IsDownloaderSelected;
                IsDownloadConfirmation = showConfirmation;
                if (showConfirmation)
                    _ = ShowToastAsync(displayName, ConfirmationDetails);
                PersistHistory();
                RefreshFilteredDownloads();
                OnPropertyChanged(nameof(HasDownloadHistory));
                OnPropertyChanged(nameof(ShowNoFilteredDownloads));
            }
            else
            {
                var errorMessage = result.ErrorMessage ?? "Fallo desconocido en la descarga.";
                if (job.State == DownloadJobState.Cancelled)
                {
                    StatusMessage = "Descarga cancelada.";
                    historyItem.Status = "Cancelada";
                    LoggerService.Write(
                        $"DOWNLOAD CANCELLED URL=\"{request.Url}\" PROVIDER=\"{selectedProvider.Name}\" " +
                        $"MEDIA=\"{request.MediaType}\" FORMAT=\"{request.FormatId ?? request.Format}\"");
                }
                else
                {
                    StatusMessage = $"Error: {errorMessage}";
                    historyItem.Status = $"Error: {errorMessage}";
                    LoggerService.Write(
                        $"DOWNLOAD FAILED URL=\"{request.Url}\" PROVIDER=\"{selectedProvider.Name}\" " +
                        $"MEDIA=\"{request.MediaType}\" FORMAT=\"{request.FormatId ?? request.Format}\" ERROR=\"{errorMessage}\"");
                }
                PersistHistory();
            }
        }
        catch (Exception ex)
        {
            LoggerService.WriteException(
                ex,
                $"DOWNLOAD UNEXPECTED FAILURE URL=\"{request.Url}\" PROVIDER=\"{selectedProvider.Name}\" " +
                $"MEDIA=\"{request.MediaType}\" FORMAT=\"{request.FormatId ?? request.Format}\"");
            StatusMessage = $"Error inesperado: {ex.Message}";
            historyItem.Status = $"Error: {ex.Message}";
            PersistHistory();
        }
        finally
        {
            _historyItemsByJob.Remove(job.Id);
            _jobsById.Remove(job.Id);
            job.Changed -= OnDownloadJobChanged;
            OnPropertyChanged(nameof(QueuedDownloadCount));
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
        var jobs = _jobsById.Values
            .Where(job => job.State is DownloadJobState.Pending or DownloadJobState.Downloading or
                DownloadJobState.Paused or DownloadJobState.Retrying)
            .ToList();
        if (jobs.Count > 0)
        {
            StatusMessage = "Cancelando las descargas activas y pendientes...";
            foreach (var job in jobs)
                _downloadQueue.Cancel(job);
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
