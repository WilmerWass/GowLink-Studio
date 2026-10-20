namespace WASSLink.Abstractions;

/// <summary>
/// Tipo de medio a descargar.
/// </summary>
public enum DownloadMediaType
{
    Audio,
    Video,
    File
}

public enum FileConflictAction
{
    CreateCopy,
    Skip,
    Overwrite
}

/// <summary>
/// Parámetros de una solicitud de descarga.
/// </summary>
public record DownloadRequest(
    string Url,
    string OutputPath,
    DownloadMediaType MediaType = DownloadMediaType.Audio,
    string Format = "mp3",
    string? FormatId = null,
    bool VideoFormatHasAudio = false,
    long MaxSpeedBytesPerSecond = 0,
    Func<CancellationToken, Task>? WaitIfPausedAsync = null,
    FileConflictAction ConflictAction = FileConflictAction.CreateCopy
);
