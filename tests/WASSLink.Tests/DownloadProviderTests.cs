using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
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
        Assert.IsType<CompositeDownloadProvider>(downloadProvider);
    }

    [Theory]
    [InlineData("https://example.com/archive.zip", "HTTP Directo (Streaming)")]
    [InlineData("https://example.com/disk.iso?download=1", "HTTP Directo (Streaming)")]
    [InlineData("https://www.youtube.com/watch?v=test", "yt-dlp")]
    [InlineData("https://soundcloud.com/artist/track", "yt-dlp")]
    public void CompositeProvider_SelectsEngineFromUrl(string url, string expectedProviderName)
    {
        var provider = new CompositeDownloadProvider(
            new DirectHttpDownloadService(),
            new YtDlpDownloadService());

        Assert.Equal(expectedProviderName, provider.GetProviderForUrl(url)?.Name);
    }

    [Fact]
    public void MainViewModel_ShowsSelectedEngineAndGowLinkDestination()
    {
        var viewModel = new MainViewModel();

        Assert.Equal(
            System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "Downloads",
                "GowLink"),
            viewModel.OutputPath);

        viewModel.DownloadUrl = "https://www.youtube.com/watch?v=test";
        Assert.Equal("Motor: yt-dlp & FFmpeg", viewModel.ActiveEngineLabel);

        viewModel.DownloadUrl = "https://example.com/archive.zip?download=1";
        Assert.Equal("Motor: HTTP Directo (Streaming)", viewModel.ActiveEngineLabel);
    }

    [Fact]
    public void MainViewModel_NavigatesBetweenRequestedModules()
    {
        var viewModel = new MainViewModel();
        var libraryPath = Path.Combine(Path.GetTempPath(), $"GowLink.Library.{Guid.NewGuid():N}");
        Directory.CreateDirectory(libraryPath);
        viewModel.OutputPath = libraryPath;

        try
        {
            Assert.True(viewModel.IsDownloaderSelected);
            viewModel.SelectSectionCommand.Execute("Buscador");
            Assert.True(viewModel.IsSearchSelected);
            viewModel.SearchQuery = "https://example.com/archive.zip";
            Assert.Equal("Motor: HTTP Directo (Streaming)", viewModel.ActiveEngineLabel);

            viewModel.SelectSectionCommand.Execute("Biblioteca");
            Assert.True(viewModel.IsLibrarySelected);
            viewModel.SelectSectionCommand.Execute("Descargas");
            Assert.True(viewModel.IsDownloadsSelected);
            viewModel.SelectSectionCommand.Execute("Configuración");
            Assert.True(viewModel.IsSettingsSelected);
        }
        finally
        {
            Directory.Delete(libraryPath, recursive: true);
        }
    }

    [Fact]
    public void MainViewModel_SearchFiltersLocalFilesByNameAndCategory()
    {
        var libraryPath = Path.Combine(Path.GetTempPath(), $"GowLink.Search.{Guid.NewGuid():N}");
        Directory.CreateDirectory(libraryPath);
        File.WriteAllText(Path.Combine(libraryPath, "logo.jpg"), "image");
        File.WriteAllText(Path.Combine(libraryPath, "logo.zip"), "archive");
        var viewModel = new MainViewModel { OutputPath = libraryPath, SearchQuery = "logo", SearchImages = true };

        try
        {
            viewModel.SearchLocalFilesCommand.Execute(null);

            Assert.Single(viewModel.LocalSearchResults);
            Assert.Equal("logo.jpg", viewModel.LocalSearchResults[0].Name);

            viewModel.SearchImages = false;
            viewModel.SearchCompressed = true;
            Assert.Single(viewModel.LocalSearchResults);
            Assert.Equal("logo.zip", viewModel.LocalSearchResults[0].Name);
        }
        finally
        {
            Directory.Delete(libraryPath, recursive: true);
        }
    }

    [Fact]
    public async Task DirectHttpProvider_StreamsFileToDestination()
    {
        var outputPath = Path.Combine(Path.GetTempPath(), $"GowLink.Tests.{Guid.NewGuid():N}");
        var payload = new byte[] { 1, 2, 3, 4 };
        using var httpClient = new HttpClient(new StubHttpMessageHandler((_, _) =>
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(payload)
            };
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = "archive.zip"
            };
            return Task.FromResult(response);
        }));
        var service = new DirectHttpDownloadService(httpClient);

        try
        {
            var result = await service.DownloadAsync(new DownloadRequest(
                "https://example.com/archive.zip?download=1",
                outputPath));

            Assert.True(result.Success, result.ErrorMessage);
            Assert.Equal(Path.Combine(outputPath, "archive.zip"), result.OutputPath);
            Assert.Equal(payload, await File.ReadAllBytesAsync(result.OutputPath!));
        }
        finally
        {
            if (Directory.Exists(outputPath))
                Directory.Delete(outputPath, recursive: true);
        }
    }

    [Fact]
    public async Task DirectHttpProvider_WaitsForResumeBeforeReadingChunks()
    {
        var outputPath = Path.Combine(Path.GetTempPath(), $"GowLink.Pause.{Guid.NewGuid():N}");
        var pauseEntered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var resume = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        using var httpClient = new HttpClient(new StubHttpMessageHandler((_, _) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent([1, 2, 3, 4])
            })));
        var service = new DirectHttpDownloadService(httpClient);
        var request = new DownloadRequest(
            "https://example.com/archive.zip",
            outputPath,
            WaitIfPausedAsync: async token =>
            {
                pauseEntered.TrySetResult();
                await resume.Task.WaitAsync(token);
            });
        var download = service.DownloadAsync(request);

        try
        {
            await pauseEntered.Task.WaitAsync(TimeSpan.FromSeconds(5));
            Assert.False(download.IsCompleted);

            resume.TrySetResult();
            var result = await download.WaitAsync(TimeSpan.FromSeconds(5));

            Assert.True(result.Success, result.ErrorMessage);
            Assert.Equal(new byte[] { 1, 2, 3, 4 }, await File.ReadAllBytesAsync(result.OutputPath!));
        }
        finally
        {
            resume.TrySetResult();
            try
            {
                await download;
            }
            finally
            {
                if (Directory.Exists(outputPath))
                    Directory.Delete(outputPath, recursive: true);
            }
        }
    }

    private sealed class StubHttpMessageHandler(
        Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> sendAsync) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) => sendAsync(request, cancellationToken);
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
