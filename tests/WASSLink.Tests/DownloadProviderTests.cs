using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using WASSLink.Abstractions;
using WASSLink.Desktop.Models;
using WASSLink.Desktop.Services;
using WASSLink.Desktop.ViewModels;
using WASSLink.Download;
using WASSLink.Download.DependencyInjection;
using Xunit;

namespace WASSLink.Tests;

public class DownloadProviderTests
{
    [Fact]
    public void YtDlpDownloadService_Implements_IDownloadProvider()
    {
        var service = new YtDlpDownloadService();
        Assert.IsAssignableFrom<IDownloadProvider>(service);
        Assert.Equal("yt-dlp", service.Name);
    }

    [Theory]
    [InlineData("https://www.youtube.com/watch?v=dQw4w9WgXcQ", true)]
    [InlineData("http://youtube.com/watch?v=test", true)]
    [InlineData("https://youtu.be/dQw4w9WgXcQ", true)]
    [InlineData("https://soundcloud.com/artist/track", true)]
    [InlineData("", false)]
    [InlineData("   ", false)]
    [InlineData("not-a-url", false)]
    [InlineData("ftp://invalid.com", false)]
    public void CanHandle_ValidatesUrlsCorrectly(string url, bool expected)
    {
        var service = new YtDlpDownloadService();
        var result = service.CanHandle(url);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void DownloadRequest_DefaultsAndProperties_AreCorrect()
    {
        var request = new DownloadRequest(
            Url: "https://www.youtube.com/watch?v=123",
            OutputPath: "C:\\Downloads"
        );

        Assert.Equal("https://www.youtube.com/watch?v=123", request.Url);
        Assert.Equal("C:\\Downloads", request.OutputPath);
        Assert.Equal(DownloadMediaType.Audio, request.MediaType);
        Assert.Equal("mp3", request.Format);
        Assert.Null(request.FormatId);

        var videoRequest = new DownloadRequest(
            Url: "https://www.youtube.com/watch?v=123",
            OutputPath: "C:\\Downloads",
            MediaType: DownloadMediaType.Video,
            Format: "mp4",
            FormatId: "18"
        );

        Assert.Equal(DownloadMediaType.Video, videoRequest.MediaType);
        Assert.Equal("mp4", videoRequest.Format);
        Assert.Equal("18", videoRequest.FormatId);
        Assert.False(videoRequest.VideoFormatHasAudio);
    }

    [Fact]
    public void DependencyInjection_Resolves_IDownloadProvider()
    {
        var services = new ServiceCollection();
        services.AddDownloadServices();
        var provider = services.BuildServiceProvider();

        var downloadProvider = provider.GetService<IDownloadProvider>();
        Assert.NotNull(downloadProvider);
        Assert.IsType<YtDlpDownloadService>(downloadProvider);
    }

    [Fact]
    public void ResolveToolPath_FindsYtDlpTool()
    {
        var path = YtDlpDownloadService.ResolveToolPath("yt-dlp", "yt-dlp.exe");
        Assert.True(System.IO.File.Exists(path), $"yt-dlp.exe should be located at: {path}");
    }

    [Fact]
    public void ParseFormats_HandlesQuotedNumbersAndEmptyStrings()
    {
        const string json = """
            {
              "formats": [
                {
                  "format_id": "251",
                  "ext": "webm",
                  "vcodec": "none",
                  "acodec": "opus",
                  "abr": "160.0",
                  "filesize": "",
                  "filesize_approx": "123456",
                  "tbr": "160.0",
                  "fps": "",
                  "width": null,
                  "height": "720"
                },
                {
                  "format_id": "18",
                  "ext": "mp4",
                  "vcodec": "avc1",
                  "acodec": "mp4a",
                  "height": "720",
                  "tbr": "1280",
                  "fps": "30",
                  "width": "1280"
                                },
                                {
                                    "format_id": "248",
                                    "ext": "webm",
                                    "vcodec": "vp9",
                                    "acodec": "none",
                                    "height": "1080",
                                    "tbr": "2400",
                                    "fps": "30",
                                    "width": "1920"
                }
              ]
            }
            """;

        var method = typeof(MediaMetadataService).GetMethod("ParseFormats", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(method);

        var result = method.Invoke(null, new object[] { json }) as List<MediaFormatOption>;
        Assert.NotNull(result);
        Assert.Contains(result, x => x.MediaType == DownloadMediaType.Audio && x.FormatId == "251");
        Assert.Contains(result, x => x.MediaType == DownloadMediaType.Video && x.FormatId == "18" && x.HasAudio);
        Assert.Contains(result, x => x.MediaType == DownloadMediaType.Video && x.FormatId == "248" && !x.HasAudio);
    }

    [Fact]
    public void YtDlpVideoSelector_UsesHeightTolerance()
    {
        var selector = YtDlpDownloadService.BuildVideoFormatSelector();

        Assert.Contains("height<=1080", selector, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("bestvideo", selector, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("bestaudio", selector, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void YtDlpSelectedVideoSelector_AddsAudioOnlyWhenVideoFormatNeedsIt()
    {
        Assert.Equal("248+bestaudio/248", YtDlpDownloadService.BuildVideoFormatSelector("248"));
        Assert.Equal("18", YtDlpDownloadService.BuildVideoFormatSelector("18", formatHasAudio: true));
    }

    [Fact]
    public void YtDlpProgressState_DetectsDownloadAndFfmpegPhases()
    {
        Assert.Equal("Descargando...", YtDlpDownloadService.GetProgressState("[download] 45.1% of 10MiB at 2MiB/s"));
        Assert.Equal("Procesando con FFmpeg...", YtDlpDownloadService.GetProgressState("[ffmpeg] Merging formats"));
        Assert.Equal("Procesando con FFmpeg...", YtDlpDownloadService.GetProgressState("[Merger] Merging formats"));
    }

    [Fact]
    public void YtDlpOutputPath_UsesMergedFileInsteadOfLastIntermediate()
    {
        var method = typeof(YtDlpDownloadService).GetMethod(
            "TryParseOutputPath",
            BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(method);

        object?[] args =
        [
            "[Merger] Merging formats into \"C:\\Downloads\\media ｜ adoración.mp4\"",
            null
        ];

        var success = method.Invoke(null, args);

        Assert.Equal(true, success);
        Assert.Equal("C:\\Downloads\\media ｜ adoración.mp4", args[1]);
    }

    [Fact]
    public void YtDlpProgressPercentage_UsesInvariantCulture()
    {
        var previousCulture = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("es-ES");

            Assert.True(YtDlpDownloadService.TryParseProgressPercentage("30.0", out var percentage));
            Assert.Equal(30.0, percentage);
            Assert.True(YtDlpDownloadService.TryParseProgressPercentage("100.0", out percentage));
            Assert.Equal(100.0, percentage);
        }
        finally
        {
            CultureInfo.CurrentCulture = previousCulture;
        }
    }

    [Fact]
    public void ClearDownloadUrlCommand_ClearsLinkAndPreviousFormatState()
    {
        var viewModel = new MainViewModel();
        var selectedFormat = new MediaFormatOption
        {
            FormatId = "248",
            Extension = "WEBM",
            Codec = "VP9",
            QualityLabel = "1080p",
            MediaType = DownloadMediaType.Video
        };

        viewModel.DownloadUrl = "https://www.youtube.com/watch?v=example";
        viewModel.VideoOptions.Add(selectedFormat);
        viewModel.SelectedFormatOption = selectedFormat;
        viewModel.IsFormatModalOpen = true;
        viewModel.IsLoadingFormats = true;

        viewModel.ClearDownloadUrlCommand.Execute(null);

        Assert.Empty(viewModel.DownloadUrl);
        Assert.Null(viewModel.SelectedFormatOption);
        Assert.Empty(viewModel.VideoOptions);
        Assert.False(viewModel.HasFormats);
        Assert.False(viewModel.IsFormatModalOpen);
        Assert.False(viewModel.IsLoadingFormats);
    }

    [Fact]
    public void DownloadProgress_TelemetryFields_ArePreserved()
    {
        var progress = new DownloadProgress(
            Percentage: 45.5,
            StatusMessage: "Descargando video...",
            Speed: "3.2MiB/s",
            Eta: "00:15",
            Title: "Cancion Increible",
            TotalSize: "12.4MiB"
        );

        Assert.Equal(45.5, progress.Percentage);
        Assert.Equal("Descargando video...", progress.StatusMessage);
        Assert.Equal("3.2MiB/s", progress.Speed);
        Assert.Equal("00:15", progress.Eta);
        Assert.Equal("Cancion Increible", progress.Title);
        Assert.Equal("12.4MiB", progress.TotalSize);
    }
}
