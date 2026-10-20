using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using WASSLink.Abstractions;

namespace WASSLink.Download;

public sealed record DirectFileInfo(
    string FileName,
    string ContentType,
    long? ContentLength,
    string FileType,
    Uri FinalUri,
    bool IsDownloadable)
{
    public string FormattedSize => ContentLength is > 0
        ? FormatSize(ContentLength.Value)
        : "Tamaño no informado";

    private static string FormatSize(long bytes)
    {
        string[] units = ["B", "KB", "MB", "GB"];
        var size = (double)bytes;
        var unit = 0;
        while (size >= 1024 && unit < units.Length - 1)
        {
            size /= 1024;
            unit++;
        }

        return $"{size:0.##} {units[unit]}";
    }
}

/// <summary>
/// Descarga archivos expuestos directamente por HTTP o HTTPS.
/// </summary>
public sealed class DirectHttpDownloadService : IDownloadProvider
{
    private const int MaximumMediaFirePageCharacters = 1_048_576;
    private static readonly Regex MediaFireDownloadButtonRegex = new(
        """(?is)<a\b(?=[^>]*\bid\s*=\s*["']downloadButton["'])[^>]*>""",
        RegexOptions.Compiled);
    private static readonly Regex HtmlHrefRegex = new(
        """(?is)\bhref\s*=\s*(?:"(?<value>[^"]*)"|'(?<value>[^']*)'|(?<value>[^\s>]+))""",
        RegexOptions.Compiled);
    private static readonly HashSet<string> SupportedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".7z", ".apk", ".avi", ".csv", ".dmg", ".exe", ".flac", ".gif", ".gz",
        ".iso", ".jpeg", ".jpg", ".json", ".m4a", ".mkv", ".mov", ".mp3", ".mp4",
        ".msi", ".pdf", ".png", ".rar", ".tar", ".wav", ".webm", ".webp", ".zip"
    };

    private readonly HttpClient _httpClient;

    public DirectHttpDownloadService(HttpClient? httpClient = null)
    {
        _httpClient = httpClient ?? new HttpClient();
    }

    public string Name => "HTTP Directo (Streaming)";

    public async Task<DirectFileInfo> AnalyzeAsync(
        string url,
        CancellationToken cancellationToken = default)
    {
        if (!Uri.TryCreate(url?.Trim(), UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            throw new ArgumentException("Ingresa un enlace HTTP o HTTPS válido.", nameof(url));
        }

        LoggerService.Write($"DIRECT ANALYZE URL=\"{uri}\"");

        try
        {
            var fileUri = IsMediaFirePage(uri)
                ? await ResolveMediaFireDownloadUriAsync(uri, cancellationToken)
                : uri;
            using var request = new HttpRequestMessage(HttpMethod.Head, fileUri);
            using var response = await _httpClient.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);

            if (response.StatusCode is HttpStatusCode.MethodNotAllowed or HttpStatusCode.NotImplemented)
            {
                using var fallbackRequest = new HttpRequestMessage(HttpMethod.Get, fileUri);
                fallbackRequest.Headers.Range = new RangeHeaderValue(0, 0);
                using var fallbackResponse = await _httpClient.SendAsync(
                    fallbackRequest,
                    HttpCompletionOption.ResponseHeadersRead,
                    cancellationToken);
                return ReadFileInfo(fallbackResponse, fileUri);
            }

            return ReadFileInfo(response, fileUri);
        }
        catch (Exception ex) when (ex is HttpRequestException or IOException or TaskCanceledException)
        {
            LoggerService.WriteException(ex, $"DIRECT ANALYZE FAILED URL=\"{uri}\"");
            throw;
        }
    }

    public bool CanHandle(string url)
    {
        return Uri.TryCreate(url?.Trim(), UriKind.Absolute, out var uri) &&
               (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps) &&
               (SupportedExtensions.Contains(Path.GetExtension(uri.AbsolutePath)) || IsMediaFirePage(uri));
    }

    public async Task<DownloadResult> DownloadAsync(
        DownloadRequest request,
        IProgress<DownloadProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!Uri.TryCreate(request.Url?.Trim(), UriKind.Absolute, out var requestUri) ||
            (requestUri.Scheme != Uri.UriSchemeHttp && requestUri.Scheme != Uri.UriSchemeHttps))
            return new DownloadResult(false, null, "La URL no apunta a un archivo HTTP/HTTPS válido.");

        string? temporaryFile = null;
        try
        {
            Directory.CreateDirectory(request.OutputPath);
            LoggerService.Write($"DIRECT DOWNLOAD START URL=\"{request.Url}\" OUTPUT=\"{request.OutputPath}\"");
            var fileUri = IsMediaFirePage(requestUri)
                ? await ResolveMediaFireDownloadUriAsync(requestUri, cancellationToken)
                : requestUri;
            using var downloadRequest = new HttpRequestMessage(HttpMethod.Get, fileUri);
            using var response = await _httpClient.SendAsync(
                downloadRequest,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);
            response.EnsureSuccessStatusCode();

            var fileInfo = ReadFileInfo(response, requestUri);
            if (!fileInfo.IsDownloadable)
            {
                var message = $"El enlace respondió una página web ({fileInfo.ContentType}), no un archivo descargable.";
                LoggerService.Write($"DIRECT DOWNLOAD REJECTED URL=\"{request.Url}\" TYPE=\"{fileInfo.ContentType}\"");
                return new DownloadResult(false, null, message);
            }

            var fileName = fileInfo.FileName;
            var outputFile = Path.Combine(request.OutputPath, fileName);
            if (request.ConflictAction == FileConflictAction.Skip && File.Exists(outputFile))
            {
                LoggerService.Write($"DIRECT DOWNLOAD SKIPPED EXISTING FILE=\"{outputFile}\"");
                return new DownloadResult(true, outputFile, WasAlreadyPresent: true);
            }

            if (request.ConflictAction == FileConflictAction.CreateCopy)
                outputFile = GetUniqueFilePath(request.OutputPath, fileName);

            LoggerService.Write(
                $"DIRECT RESPONSE URL=\"{requestUri}\" STATUS={(int)response.StatusCode} " +
                $"TYPE=\"{fileInfo.ContentType}\" FILE=\"{fileName}\" SIZE={fileInfo.ContentLength?.ToString() ?? "unknown"}");
            var totalBytes = response.Content.Headers.ContentLength;
            var downloadedBytes = 0L;
            var stopwatch = Stopwatch.StartNew();

            progress?.Report(new DownloadProgress(0, "Descargando por HTTP Directo...", Title: Path.GetFileNameWithoutExtension(outputFile)));

            await using var source = await response.Content.ReadAsStreamAsync(cancellationToken);
            temporaryFile = Path.Combine(request.OutputPath, $".gowlink-{Guid.NewGuid():N}.partial");
            await using (var destination = new FileStream(
                             temporaryFile,
                             FileMode.CreateNew,
                             FileAccess.Write,
                             FileShare.None,
                             bufferSize: 8192,
                             useAsync: true))
            {
                var buffer = new byte[8192];
                int bytesRead;
                while (true)
                {
                    if (request.WaitIfPausedAsync is not null)
                        await request.WaitIfPausedAsync(cancellationToken);

                    bytesRead = await source.ReadAsync(buffer, cancellationToken);
                    if (bytesRead == 0)
                        break;

                    if (request.WaitIfPausedAsync is not null)
                        await request.WaitIfPausedAsync(cancellationToken);

                    await destination.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
                    downloadedBytes += bytesRead;

                    progress?.Report(new DownloadProgress(
                        totalBytes is > 0 ? (double)downloadedBytes / totalBytes.Value * 100 : -1,
                        "Descargando por HTTP Directo...",
                        Title: Path.GetFileNameWithoutExtension(outputFile)));

                    if (request.MaxSpeedBytesPerSecond > 0)
                    {
                        var targetElapsed = TimeSpan.FromSeconds(
                            (double)downloadedBytes / request.MaxSpeedBytesPerSecond);
                        var remainingDelay = targetElapsed - stopwatch.Elapsed;
                        if (remainingDelay > TimeSpan.Zero)
                            await Task.Delay(remainingDelay, cancellationToken);
                    }

                    var percentage = totalBytes is > 0
                        ? (double)downloadedBytes / totalBytes.Value * 100
                        : -1;
                    var speed = stopwatch.Elapsed.TotalSeconds > 0
                        ? FormatSpeed(downloadedBytes / stopwatch.Elapsed.TotalSeconds)
                        : null;
                    var eta = totalBytes is > 0 && downloadedBytes > 0 && stopwatch.Elapsed.TotalSeconds > 0
                        ? TimeSpan.FromSeconds((totalBytes.Value - downloadedBytes) /
                                               (downloadedBytes / stopwatch.Elapsed.TotalSeconds)).ToString(@"hh\:mm\:ss")
                        : null;

                    progress?.Report(new DownloadProgress(
                        percentage,
                        "Descargando por HTTP Directo...",
                        speed,
                        eta,
                        Path.GetFileNameWithoutExtension(outputFile),
                        totalBytes is > 0 ? FormatSize(totalBytes.Value) : null));
                }
            }

            if (request.ConflictAction == FileConflictAction.Overwrite)
                File.Move(temporaryFile, outputFile, overwrite: true);
            else
                outputFile = MoveToAvailableCopyPath(temporaryFile, request.OutputPath, fileName);

            temporaryFile = null;
            progress?.Report(new DownloadProgress(100, "Descarga HTTP Directa completada."));
            LoggerService.Write($"DIRECT DOWNLOAD COMPLETED FILE=\"{outputFile}\" BYTES={downloadedBytes}");
            return new DownloadResult(true, outputFile);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return new DownloadResult(false, null, "Descarga cancelada por el usuario.");
        }
        catch (HttpRequestException ex)
        {
            LoggerService.WriteException(ex, $"DIRECT DOWNLOAD HTTP FAILED URL=\"{requestUri}\" OUTPUT=\"{request.OutputPath}\"");
            var isTransient = ex.StatusCode is null ||
                              ex.StatusCode is HttpStatusCode.RequestTimeout or
                                  HttpStatusCode.TooManyRequests ||
                              ex.StatusCode is { } statusCode && (int)statusCode >= 500;
            return new DownloadResult(false, null, $"Error de descarga HTTP: {ex.Message}", IsTransientFailure: isTransient);
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            LoggerService.WriteException(ex, $"DIRECT DOWNLOAD TIMEOUT URL=\"{requestUri}\"");
            return new DownloadResult(false, null, "Se agotó el tiempo de espera de la descarga.", IsTransientFailure: true);
        }
        catch (InvalidDataException ex)
        {
            LoggerService.WriteException(ex, $"DIRECT DOWNLOAD LINK REJECTED URL=\"{requestUri}\"");
            return new DownloadResult(false, null, ex.Message);
        }
        catch (IOException ex)
        {
            LoggerService.WriteException(ex, $"DIRECT DOWNLOAD SAVE FAILED URL=\"{requestUri}\" OUTPUT=\"{request.OutputPath}\"");
            return new DownloadResult(false, null, $"Error al guardar el archivo: {ex.Message}");
        }
        catch (UnauthorizedAccessException ex)
        {
            LoggerService.WriteException(ex, $"DIRECT DOWNLOAD ACCESS FAILED URL=\"{requestUri}\" OUTPUT=\"{request.OutputPath}\"");
            return new DownloadResult(false, null, $"No hay permisos para guardar el archivo: {ex.Message}");
        }

        finally
        {
            if (temporaryFile is not null && File.Exists(temporaryFile))
            {
                try
                {
                    File.Delete(temporaryFile);
                }
                catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
                {
                    LoggerService.WriteException(ex, $"DIRECT DOWNLOAD TEMP CLEANUP FAILED FILE=\"{temporaryFile}\"");
                }
            }
        }
    }

    private async Task<Uri> ResolveMediaFireDownloadUriAsync(
        Uri pageUri,
        CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, pageUri);
        using var response = await _httpClient.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);
        response.EnsureSuccessStatusCode();

        if (!string.Equals(
                response.Content.Headers.ContentType?.MediaType,
                "text/html",
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidDataException("El enlace de MediaFire no devolvió una página HTML válida.");
        }

        if (response.Content.Headers.ContentLength is > MaximumMediaFirePageCharacters)
            throw new InvalidDataException("La página de MediaFire excede el tamaño permitido para su análisis.");

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
        var page = new StringBuilder();
        var buffer = new char[8192];
        while (true)
        {
            var count = await reader.ReadAsync(buffer.AsMemory(), cancellationToken);
            if (count == 0)
                break;
            if (page.Length + count > MaximumMediaFirePageCharacters)
                throw new InvalidDataException("La página de MediaFire excede el tamaño permitido para su análisis.");
            page.Append(buffer, 0, count);
        }

        var effectivePageUri = response.RequestMessage?.RequestUri ?? pageUri;
        var button = MediaFireDownloadButtonRegex.Match(page.ToString());
        var href = button.Success ? HtmlHrefRegex.Match(button.Value) : Match.Empty;
        if (!href.Success)
            throw new InvalidDataException("No se encontró el botón de descarga en la página de MediaFire.");

        var link = WebUtility.HtmlDecode(href.Groups["value"].Value);
        if (!Uri.TryCreate(effectivePageUri, link, out var downloadUri) ||
            downloadUri.Scheme != Uri.UriSchemeHttps ||
            !IsMediaFireHost(downloadUri))
        {
            throw new InvalidDataException("MediaFire devolvió un destino de descarga no válido.");
        }

        return downloadUri;
    }

    private static bool IsMediaFirePage(Uri uri) =>
        IsMediaFireHost(uri) &&
        (uri.AbsolutePath.StartsWith("/file/", StringComparison.OrdinalIgnoreCase) ||
         uri.AbsolutePath.StartsWith("/file_premium/", StringComparison.OrdinalIgnoreCase));

    private static bool IsMediaFireHost(Uri uri) =>
        uri.Host.Equals("mediafire.com", StringComparison.OrdinalIgnoreCase) ||
        uri.Host.EndsWith(".mediafire.com", StringComparison.OrdinalIgnoreCase);

    private static DirectFileInfo ReadFileInfo(HttpResponseMessage response, Uri requestUri)
    {
        response.EnsureSuccessStatusCode();

        var finalUri = response.RequestMessage?.RequestUri ?? requestUri;
        var reportedContentType = response.Content.Headers.ContentType?.MediaType;
        var contentType = reportedContentType ?? "application/octet-stream";
        var fileName = GetFileName(response.Content.Headers.ContentDisposition, finalUri, contentType);
        var isHtml = contentType.Equals("text/html", StringComparison.OrdinalIgnoreCase) ||
                     contentType.Equals("application/xhtml+xml", StringComparison.OrdinalIgnoreCase);
        var isDownloadable = response.Content.Headers.ContentDisposition?.DispositionType
                                 ?.Equals("attachment", StringComparison.OrdinalIgnoreCase) == true ||
                             (!string.IsNullOrWhiteSpace(reportedContentType) && !isHtml) ||
                             SupportedExtensions.Contains(Path.GetExtension(finalUri.AbsolutePath));

        return new DirectFileInfo(
            fileName,
            contentType,
            response.Content.Headers.ContentRange?.Length ?? response.Content.Headers.ContentLength,
            GetFileType(fileName, contentType),
            finalUri,
            isDownloadable);
    }

    private static string GetFileType(string fileName, string contentType)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        var type = extension switch
        {
            ".7z" => "Archivo 7-Zip",
            ".apk" => "Paquete Android (APK)",
            ".csv" => "Archivo CSV",
            ".dmg" => "Imagen de disco macOS",
            ".exe" => "Aplicación ejecutable (EXE)",
            ".flac" => "Audio FLAC",
            ".gif" => "Imagen GIF",
            ".gz" => "Archivo GZIP",
            ".iso" => "Imagen de disco ISO",
            ".jpeg" or ".jpg" => "Imagen JPEG",
            ".json" => "Archivo JSON",
            ".m4a" => "Audio M4A",
            ".mkv" => "Vídeo Matroska",
            ".mp3" => "Audio MP3",
            ".mp4" => "Vídeo MP4",
            ".msi" => "Instalador de Windows (MSI)",
            ".pdf" => "Documento PDF",
            ".png" => "Imagen PNG",
            ".rar" => "Archivo RAR",
            ".tar" => "Archivo TAR",
            ".wav" => "Audio WAV",
            ".webm" => "Vídeo/WebM",
            ".webp" => "Imagen WebP",
            ".zip" => "Archivo ZIP",
            _ => null
        };

        if (type is not null)
            return type;

        if (contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            return $"Imagen ({contentType})";
        if (contentType.StartsWith("audio/", StringComparison.OrdinalIgnoreCase))
            return $"Audio ({contentType})";
        if (contentType.StartsWith("video/", StringComparison.OrdinalIgnoreCase))
            return $"Vídeo ({contentType})";
        return $"Archivo ({contentType})";
    }

    private static string GetFileName(
        ContentDispositionHeaderValue? disposition,
        Uri uri,
        string contentType)
    {
        var name = disposition?.FileNameStar ?? disposition?.FileName;
        if (!string.IsNullOrWhiteSpace(name))
            name = name.Trim().Trim('"');
        else
            name = Uri.UnescapeDataString(Path.GetFileName(uri.AbsolutePath));

        name = Path.GetFileName(name.Replace('\\', '/'));
        if (string.IsNullOrWhiteSpace(Path.GetExtension(name)) &&
            contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
        {
            var imageExtension = contentType switch
            {
                "image/jpeg" => ".jpg",
                "image/png" => ".png",
                "image/gif" => ".gif",
                "image/webp" => ".webp",
                _ => ".img"
            };
            name = $"{name}{imageExtension}";
        }

        if (string.IsNullOrWhiteSpace(name))
            name = $"download{GetExtensionForContentType(contentType)}";

        var invalidCharacters = Path.GetInvalidFileNameChars();
        var safeName = new string(name.Where(character => !invalidCharacters.Contains(character)).ToArray());
        return string.IsNullOrWhiteSpace(safeName) ? "download" : safeName;
    }

    private static string GetExtensionForContentType(string contentType) => contentType switch
    {
        "application/zip" => ".zip",
        "application/x-7z-compressed" => ".7z",
        "application/x-rar-compressed" => ".rar",
        "application/pdf" => ".pdf",
        "application/octet-stream" => ".bin",
        "image/jpeg" => ".jpg",
        "image/png" => ".png",
        "image/gif" => ".gif",
        "image/webp" => ".webp",
        _ => ".bin"
    };

    private static string GetUniqueFilePath(string directory, string fileName)
    {
        var path = Path.Combine(directory, fileName);
        if (!File.Exists(path))
            return path;

        var name = Path.GetFileNameWithoutExtension(fileName);
        var extension = Path.GetExtension(fileName);
        for (var index = 1; ; index++)
        {
            path = Path.Combine(directory, $"{name} ({index}){extension}");
            if (!File.Exists(path))
                return path;
        }
    }

    private static string MoveToAvailableCopyPath(string temporaryFile, string directory, string fileName)
    {
        while (true)
        {
            var targetPath = GetUniqueFilePath(directory, fileName);
            try
            {
                File.Move(temporaryFile, targetPath);
                return targetPath;
            }
            catch (IOException) when (File.Exists(targetPath))
            {
            }
        }
    }

    private static string FormatSpeed(double bytesPerSecond) =>
        $"{FormatSize((long)bytesPerSecond)}/s";

    private static string FormatSize(long bytes)
    {
        string[] units = ["B", "KB", "MB", "GB"];
        var size = (double)bytes;
        var unit = 0;
        while (size >= 1024 && unit < units.Length - 1)
        {
            size /= 1024;
            unit++;
        }

        return $"{size:0.##} {units[unit]}";
    }
}
