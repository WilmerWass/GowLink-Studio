using CommunityToolkit.Mvvm.ComponentModel;
using WASSLink.Abstractions;

namespace WASSLink.Desktop.Models;

/// <summary>
/// Representa una opción individual de formato y calidad de medio obtenida mediante inspección de metadatos.
/// </summary>
public partial class MediaFormatOption : ObservableObject
{
    public required string FormatId { get; init; }
    public required string Extension { get; init; }
    public required string Codec { get; init; }
    public required string QualityLabel { get; init; }
    public double? EstimatedSizeBytes { get; init; }
    public bool HasAudio { get; init; }
    public DownloadMediaType MediaType { get; init; }
    public bool IsRecommended { get; init; }
    public string ResolutionOrBitrate { get; init; } = string.Empty;

    public string FormattedSize => EstimatedSizeBytes.HasValue && EstimatedSizeBytes.Value > 0
        ? EstimatedSizeBytes.Value >= 1024 * 1024 * 1024
            ? $"{EstimatedSizeBytes.Value / (1024.0 * 1024.0 * 1024.0):F2} GB"
            : $"{EstimatedSizeBytes.Value / (1024.0 * 1024.0):F1} MB"
        : "Tamaño var.";

    [ObservableProperty]
    private bool isSelected;
}
