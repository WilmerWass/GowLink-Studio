using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using WASSLink.Abstractions;

namespace WASSLink.Download;

/// <summary>
/// Proveedor de descargas directas vía HTTP(S) en modo streaming.
/// </summary>
public sealed class DirectHttpDownloadService : IDownloadProvider
{
    private static readonly int BufferSizeBytes = 8 * 1024; // 8 KB
    private readonly HttpClient _httpClient;

    public DirectHttpDownloadService(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    public string Name => "HTTP";

    public bool CanHandle(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return false;

        // Solo http/https
        return Uri.TryCreate(url.Trim(), UriKind.Absolute, out var uri) &&
               (uri.Scheme.Equals(Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) ||
                uri.Scheme.Equals(Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<DownloadResult> DownloadAsync(
        DownloadRequest request,
        IProgress<DownloadProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        if (request is null)
            throw new ArgumentNullException(nameof(request));

        if (!CanHandle(request.Url))
            return new DownloadResult(false, null, "URL no soportada por el proveedor HTTP.");

        if (string.IsNullOrWhiteSpace(request.OutputPath))
            return new DownloadResult(false, null, "OutputPath no puede estar vacío.");

        try
        {
            // Asegurar carpeta destino
            var directory = Path.GetDirectoryName(request.OutputPath);
            if (!string.IsNullOrWhiteSpace(directory))
                Directory.CreateDirectory(directory);

            using var response = await _httpClient.SendAsync(
                new HttpRequestMessage(HttpMethod.Get, request.Url),
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return new DownloadResult(
                    false,
                    null,
                    $"HTTP error {(int)response.StatusCode} ({response.ReasonPhrase}).");
            }

            var totalBytes = response.Content.Headers.ContentLength;

            progress?.Report(new DownloadProgress(
                Percentage: 0,
                StatusMessage: "Iniciando descarga..."
            ));

            await using var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken);
            await using var fileStream = new FileStream(
                request.OutputPath,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None,
                bufferSize: BufferSizeBytes,
                useAsync: true);

            var buffer = new byte[BufferSizeBytes];
            long bytesDownloaded = 0;

            while (true)
            {
                var bytesRead = await responseStream.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken);
                if (bytesRead == 0)
                    break;

                await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
                bytesDownloaded += bytesRead;

                if (totalBytes.HasValue && totalBytes.Value > 0)
                {
                    var percentage = (double)bytesDownloaded / totalBytes.Value * 100d;
                    progress?.Report(new DownloadProgress(
                        Percentage: Math.Clamp(percentage, 0d, 100d),
                        StatusMessage: "Descargando...",
                        TotalSize: totalBytes.Value.ToString()));
                }
                else
                {
                    progress?.Report(new DownloadProgress(
                        Percentage: 0,
                        StatusMessage: "Descargando...",
                        TotalSize: null));
                }
            }

            progress?.Report(new DownloadProgress(
                Percentage: 100,
                StatusMessage: "Descarga finalizada.",
                TotalSize: totalBytes?.ToString()));

            return new DownloadResult(true, request.OutputPath, null);
        }
        catch (OperationCanceledException)
        {
            return new DownloadResult(false, null, "Descarga cancelada.");
        }
        catch (Exception ex)
        {
            return new DownloadResult(false, null, ex.Message);
        }
    }
}
