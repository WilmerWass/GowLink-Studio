using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WASSLink.Abstractions;

namespace WASSLink.Download;

/// <summary>
/// Proveedor compuesto que enruta la descarga al primer provider compatible.
/// </summary>
public sealed class CompositeDownloadProvider : IDownloadProvider
{
    private readonly IReadOnlyList<IDownloadProvider> _providers;

    public CompositeDownloadProvider(IEnumerable<IDownloadProvider> providers)
    {
        if (providers is null)
            throw new ArgumentNullException(nameof(providers));

        _providers = providers.ToList();
        if (_providers.Count == 0)
            throw new InvalidOperationException("CompositeDownloadProvider requiere al menos un provider.");
    }

    public string Name => "Composite";

    public bool CanHandle(string url)
    {
        return _providers.Any(p => p.CanHandle(url));
    }

    public Task<DownloadResult> DownloadAsync(
        DownloadRequest request,
        IProgress<DownloadProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        if (request is null)
            throw new ArgumentNullException(nameof(request));

        var provider = _providers.FirstOrDefault(p => p.CanHandle(request.Url));
        if (provider is null)
            throw new InvalidOperationException($"No existe provider para la URL: {request.Url}");

        return provider.DownloadAsync(request, progress, cancellationToken);
    }
}
