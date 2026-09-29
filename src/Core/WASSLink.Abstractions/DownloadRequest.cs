namespace WASSLink.Abstractions;

/// <summary>
/// Tipo de medio a descargar.
/// </summary>
public enum DownloadMediaType
{
    Audio,
    Video
}

/// <summary>
/// Parámetros de una solicitud de descarga.
/// </summary>
public record DownloadRequest(
    string Url,
    string OutputPath,
    DownloadMediaType MediaType = DownloadMediaType.Audio,
    string Format = "mp3"
);
