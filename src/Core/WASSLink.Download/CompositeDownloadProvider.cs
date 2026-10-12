using System;
using System.Threading;
using System.Threading.Tasks;
using WASSLink.Abstractions;

namespace WASSLink.Download;

/// <summary>
/// Enruta URLs de archivos directos a HTTP y el resto de enlaces compatibles a yt-dlp.
/// </summary>
public sealed class CompositeDownloadProvider(
    DirectHttpDownloadService directHttpProvider,
    YtDlpDownloadService ytDlpProvider) : IDownloadProvider
{
    public string Name => "Composite";
    public DirectHttpDownloadService DirectHttpProvider => directHttpProvider;

    public bool CanHandle(string url) =>
        directHttpProvider.CanHandle(url) || ytDlpProvider.CanHandle(url);

    public IDownloadProvider? GetProviderForUrl(string url)
    {
        if (directHttpProvider.CanHandle(url))
            return directHttpProvider;

        return ytDlpProvider.CanHandle(url) ? ytDlpProvider : null;
    }

    public Task<DownloadResult> DownloadAsync(
        DownloadRequest request,
        IProgress<DownloadProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var provider = GetProviderForUrl(request.Url);
        return provider is null
            ? Task.FromResult(new DownloadResult(false, null, "La URL ingresada no es compatible."))
            : provider.DownloadAsync(request, progress, cancellationToken);
    }
}
