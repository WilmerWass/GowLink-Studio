using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using WASSLink.Abstractions;
using WASSLink.Configuration;
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
        Assert.NotNull(provider.GetService<DownloadQueueService>());
    }

    [Theory]
    [InlineData("https://example.com/archive.zip", "HTTP Directo (Streaming)")]
    [InlineData("https://example.com/disk.iso?download=1", "HTTP Directo (Streaming)")]
    [InlineData("https://www.mediafire.com/file/key/archive.rar/file", "HTTP Directo (Streaming)")]
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
            viewModel.SelectSectionCommand.Execute("Historial");
            Assert.True(viewModel.IsDownloadsSelected);
            viewModel.SelectSectionCommand.Execute("Descargar");
            Assert.True(viewModel.IsDownloaderSelected);
            viewModel.SelectSectionCommand.Execute("Configuración");
            Assert.True(viewModel.IsSettingsSelected);
        }
        finally
        {
            Directory.Delete(libraryPath, recursive: true);
        }
    }

    [Fact]
    public void MainViewModel_AccentColorOptionsUseNamesAndUpdateAccent()
    {
        var viewModel = new MainViewModel();

        Assert.Equal(new[] { "Violeta", "Cian", "Esmeralda", "Ámbar" }, viewModel.AccentColorOptions);
        viewModel.AccentColorName = "Cian";
        Assert.Equal("#0891B2", viewModel.AccentColor);
        viewModel.AccentColorName = "Esmeralda";
        Assert.Equal("#059669", viewModel.AccentColor);
        viewModel.AccentColorName = "Ámbar";
        Assert.Equal("#D97706", viewModel.AccentColor);
    }

    [Fact]
    public void MainViewModel_OffersBingAfterLocalMissAndAppliesImageFilter()
    {
        var viewModel = new MainViewModel();
        var searchPath = Path.Combine(Path.GetTempPath(), $"GowLink.Bing.{Guid.NewGuid():N}");
        Directory.CreateDirectory(searchPath);
        viewModel.OutputPath = searchPath;
        viewModel.SearchQuery = "aurora lights";

        try
        {
            viewModel.SearchLocalFilesCommand.Execute(null);

            Assert.True(viewModel.CanSearchOnline);
            Assert.Contains("carpeta local", viewModel.SearchResultMessage, StringComparison.OrdinalIgnoreCase);
            var generalSearch = new Uri(viewModel.BingSearchUrl);
            Assert.Equal("/search", generalSearch.AbsolutePath);
            Assert.Contains("aurora lights", Uri.UnescapeDataString(generalSearch.Query));

            viewModel.SearchImages = true;
            var imageSearch = new Uri(viewModel.BingSearchUrl);
            Assert.Equal("/images/search", imageSearch.AbsolutePath);
            Assert.Contains("images", Uri.UnescapeDataString(imageSearch.Query));
        }
        finally
        {
            Directory.Delete(searchPath, recursive: true);
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
    public async Task DirectHttpProvider_AnalyzesExtensionlessImageLink()
    {
        using var httpClient = new HttpClient(new StubHttpMessageHandler((request, _) =>
        {
            Assert.Equal(HttpMethod.Head, request.Method);
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent([1, 2, 3])
            };
            response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
            return Task.FromResult(response);
        }));
        var service = new DirectHttpDownloadService(httpClient);

        var result = await service.AnalyzeAsync("https://images.example.com/images?id=123");

        Assert.Equal("images.jpg", result.FileName);
        Assert.Equal("Imagen JPEG", result.FileType);
        Assert.Equal("image/jpeg", result.ContentType);
        Assert.Equal(3L, result.ContentLength);
        Assert.True(result.IsDownloadable);
    }

    [Fact]
    public async Task DirectHttpProvider_AnalyzesAndDownloadsMediaFirePage()
    {
        const string pageUrl = "https://www.mediafire.com/file_premium/key/archive.rar/file";
        const string fileUrl = "https://download.example.mediafire.com/signed/archive.rar";
        const string page = """
            <html><body><a class="input" id="downloadButton" href="https://download.example.mediafire.com/signed/archive.rar">Download</a></body></html>
            """;
        var payload = new byte[] { 0x52, 0x61, 0x72, 0x21, 1, 2 };
        using var httpClient = new HttpClient(new StubHttpMessageHandler((request, _) =>
        {
            if (request.RequestUri?.AbsoluteUri == pageUrl)
            {
                Assert.Equal(HttpMethod.Get, request.Method);
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(page, System.Text.Encoding.UTF8, "text/html")
                });
            }

            Assert.Equal(fileUrl, request.RequestUri?.AbsoluteUri);
            Assert.Contains(request.Method, new[] { HttpMethod.Head, HttpMethod.Get });
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(payload)
            };
            response.Content.Headers.ContentType =
                new System.Net.Http.Headers.MediaTypeHeaderValue("application/x-rar-compressed");
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = "Asere.Installer - dprojects.rar"
            };
            return Task.FromResult(response);
        }));
        var service = new DirectHttpDownloadService(httpClient);
        var outputPath = Path.Combine(Path.GetTempPath(), $"GowLink.MediaFire.{Guid.NewGuid():N}");

        try
        {
            Assert.True(service.CanHandle(pageUrl));
            var info = await service.AnalyzeAsync(pageUrl);

            Assert.Equal("Asere.Installer - dprojects.rar", info.FileName);
            Assert.Equal("Archivo RAR", info.FileType);
            Assert.True(info.IsDownloadable);

            var result = await service.DownloadAsync(new DownloadRequest(pageUrl, outputPath));

            Assert.True(result.Success, result.ErrorMessage);
            Assert.Equal(payload, await File.ReadAllBytesAsync(result.OutputPath!));
        }
        finally
        {
            if (Directory.Exists(outputPath))
                Directory.Delete(outputPath, recursive: true);
        }
    }

    [Fact]
    public async Task DirectHttpProvider_RejectsMediaFireDownloadOutsideMediaFire()
    {
        const string pageUrl = "https://www.mediafire.com/file/key/archive.rar/file";
        using var httpClient = new HttpClient(new StubHttpMessageHandler((_, _) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    """<a id="downloadButton" href="https://attacker.example/archive.rar">Download</a>""",
                    System.Text.Encoding.UTF8,
                    "text/html")
            })));
        var service = new DirectHttpDownloadService(httpClient);

        await Assert.ThrowsAsync<InvalidDataException>(() => service.AnalyzeAsync(pageUrl));
    }

    [Fact]
    public async Task MainViewModel_AnalyzesDirectFileBeforeStartingDownload()
    {
        var outputPath = Path.Combine(Path.GetTempPath(), $"GowLink.Preview.{Guid.NewGuid():N}");
        var payload = new byte[] { 1, 2, 3, 4 };
        using var httpClient = new HttpClient(new StubHttpMessageHandler((request, _) =>
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(payload)
            };
            response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
            if (request.Method == HttpMethod.Get)
            {
                response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                {
                    FileName = "photo.jpg"
                };
            }
            return Task.FromResult(response);
        }));
        var provider = new CompositeDownloadProvider(
            new DirectHttpDownloadService(httpClient),
            new YtDlpDownloadService());
        var viewModel = new MainViewModel(provider, new StubClipboardService())
        {
            OutputPath = outputPath,
            DownloadUrl = "https://images.example.com/photo"
        };

        try
        {
            await viewModel.AnalyzeCommand.ExecuteAsync(null);

            Assert.True(viewModel.IsDirectFilePreview);
            Assert.Equal("photo.jpg", viewModel.DirectFileName);
            Assert.Equal("Imagen JPEG", viewModel.DirectFileType);
            Assert.False(Directory.Exists(outputPath));

            await viewModel.ConfirmDirectFileDownloadCommand.ExecuteAsync(null);

            Assert.False(viewModel.IsDirectFilePreview);
            Assert.True(viewModel.IsDownloadConfirmation);
            Assert.Equal(payload, await File.ReadAllBytesAsync(Path.Combine(outputPath, "photo.jpg")));
        }
        finally
        {
            if (Directory.Exists(outputPath))
                Directory.Delete(outputPath, recursive: true);
        }
    }

    [Fact]
    public async Task DirectHttpProvider_SavesDuplicateAsNumberedCopy()
    {
        var outputPath = Path.Combine(Path.GetTempPath(), $"GowLink.Duplicate.{Guid.NewGuid():N}");
        Directory.CreateDirectory(outputPath);
        var originalPath = Path.Combine(outputPath, "archive.zip");
        await File.WriteAllBytesAsync(originalPath, [9, 9, 9]);
        var payload = new byte[] { 1, 2, 3, 4 };
        using var httpClient = new HttpClient(new StubHttpMessageHandler((_, _) =>
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(payload)
            };
            response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/zip");
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
                "https://example.com/archive.zip",
                outputPath));

            Assert.True(result.Success, result.ErrorMessage);
            Assert.Equal(Path.Combine(outputPath, "archive (1).zip"), result.OutputPath);
            Assert.Equal(new byte[] { 9, 9, 9 }, await File.ReadAllBytesAsync(originalPath));
            Assert.Equal(payload, await File.ReadAllBytesAsync(result.OutputPath!));
        }
        finally
        {
            Directory.Delete(outputPath, recursive: true);
        }
    }

    [Theory]
    [InlineData(HttpStatusCode.NotFound, false)]
    [InlineData(HttpStatusCode.ServiceUnavailable, true)]
    public async Task DirectHttpProvider_ClassifiesHttpFailuresForRetry(
        HttpStatusCode statusCode,
        bool expectedTransient)
    {
        var outputPath = Path.Combine(Path.GetTempPath(), $"GowLink.HttpFailure.{Guid.NewGuid():N}");
        using var httpClient = new HttpClient(new StubHttpMessageHandler((_, _) =>
            Task.FromResult(new HttpResponseMessage(statusCode))));
        var service = new DirectHttpDownloadService(httpClient);

        try
        {
            var result = await service.DownloadAsync(new DownloadRequest(
                "https://example.com/archive.zip",
                outputPath));

            Assert.False(result.Success);
            Assert.Equal(expectedTransient, result.IsTransientFailure);
            Assert.Empty(Directory.GetFiles(outputPath));
        }
        finally
        {
            if (Directory.Exists(outputPath))
                Directory.Delete(outputPath, recursive: true);
        }
    }

    [Fact]
    public async Task DirectHttpProvider_SkipsOrOverwritesDuplicatesOnlyWhenRequested()
    {
        var outputPath = Path.Combine(Path.GetTempPath(), $"GowLink.Conflict.{Guid.NewGuid():N}");
        Directory.CreateDirectory(outputPath);
        var existingPath = Path.Combine(outputPath, "archive.zip");
        await File.WriteAllBytesAsync(existingPath, [9, 9, 9]);
        var payload = new byte[] { 1, 2, 3, 4 };
        using var httpClient = new HttpClient(new StubHttpMessageHandler((_, _) =>
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(payload)
            };
            response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/zip");
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = "archive.zip"
            };
            return Task.FromResult(response);
        }));
        var service = new DirectHttpDownloadService(httpClient);

        try
        {
            var skipped = await service.DownloadAsync(new DownloadRequest(
                "https://example.com/archive.zip",
                outputPath,
                ConflictAction: FileConflictAction.Skip));

            Assert.True(skipped.Success);
            Assert.True(skipped.WasAlreadyPresent);
            Assert.Equal(new byte[] { 9, 9, 9 }, await File.ReadAllBytesAsync(existingPath));

            var overwritten = await service.DownloadAsync(new DownloadRequest(
                "https://example.com/archive.zip",
                outputPath,
                ConflictAction: FileConflictAction.Overwrite));

            Assert.True(overwritten.Success, overwritten.ErrorMessage);
            Assert.Equal(existingPath, overwritten.OutputPath);
            Assert.Equal(payload, await File.ReadAllBytesAsync(existingPath));
        }
        finally
        {
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

    [Fact]
    public async Task DownloadQueue_LimitsConcurrencyAndStartsJobsInFifoOrder()
    {
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var twoStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var startOrder = new List<string>();
        var active = 0;
        var maximumActive = 0;
        var provider = new StubDownloadProvider(async (request, _, token) =>
        {
            var current = Interlocked.Increment(ref active);
            lock (startOrder)
            {
                startOrder.Add(request.Url);
                maximumActive = Math.Max(maximumActive, current);
            }
            if (current == 2)
                twoStarted.TrySetResult();

            try
            {
                await release.Task.WaitAsync(token);
                return new DownloadResult(true, request.Url);
            }
            finally
            {
                Interlocked.Decrement(ref active);
            }
        });
        var queue = new DownloadQueueService((_, token) => Task.Delay(Timeout.Infinite, token))
        {
            MaximumConcurrency = 2
        };
        var jobs = Enumerable.Range(1, 3)
            .Select(index => queue.Enqueue(new DownloadJob(
                provider,
                new DownloadRequest($"https://example.com/{index}", Path.GetTempPath()))))
            .ToArray();

        try
        {
            await twoStarted.Task.WaitAsync(TimeSpan.FromSeconds(5));
            Assert.Equal(2, queue.ActiveCount);
            Assert.Equal(2, maximumActive);
            release.TrySetResult();
            await Task.WhenAll(jobs.Select(job => job.Completion)).WaitAsync(TimeSpan.FromSeconds(5));

            Assert.Equal(2, maximumActive);
            Assert.Equal(
                ["https://example.com/1", "https://example.com/2", "https://example.com/3"],
                startOrder);
        }
        finally
        {
            release.TrySetResult();
        }
    }

    [Fact]
    public async Task DownloadQueue_RetriesTransientFailuresUpToThreeAttempts()
    {
        var attempts = 0;
        var delays = new List<double>();
        var provider = new StubDownloadProvider((request, _, _) =>
        {
            var currentAttempt = Interlocked.Increment(ref attempts);
            return Task.FromResult(currentAttempt < 3
                ? new DownloadResult(false, ErrorMessage: "Temporary failure", IsTransientFailure: true)
                : new DownloadResult(true, request.Url));
        });
        var queue = new DownloadQueueService((delay, _) =>
        {
            delays.Add(delay.TotalSeconds);
            return Task.CompletedTask;
        });
        var job = queue.Enqueue(new DownloadJob(
            provider,
            new DownloadRequest("https://example.com/retry", Path.GetTempPath())));

        var result = await job.Completion.WaitAsync(TimeSpan.FromSeconds(5));

        Assert.True(result.Success);
        Assert.Equal(3, attempts);
        Assert.Equal(3, job.Attempt);
        Assert.Equal([1d, 2d], delays);
    }

    [Fact]
    public async Task DownloadQueue_FailedJobDoesNotPreventFollowingJob()
    {
        var provider = new StubDownloadProvider((request, _, _) =>
            Task.FromResult(request.Url.EndsWith("/first", StringComparison.Ordinal)
                ? new DownloadResult(false, ErrorMessage: "Permanent failure")
                : new DownloadResult(true, request.Url)));
        var queue = new DownloadQueueService((_, _) => Task.CompletedTask)
        {
            MaximumConcurrency = 1
        };
        var first = queue.Enqueue(new DownloadJob(
            provider,
            new DownloadRequest("https://example.com/first", Path.GetTempPath())));
        var second = queue.Enqueue(new DownloadJob(
            provider,
            new DownloadRequest("https://example.com/second", Path.GetTempPath())));

        var results = await Task.WhenAll(first.Completion, second.Completion).WaitAsync(TimeSpan.FromSeconds(5));

        Assert.False(results[0].Success);
        Assert.True(results[1].Success);
    }

    [Fact]
    public async Task DownloadQueue_CancelsPendingJobWithoutStartingIt()
    {
        var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var provider = new StubDownloadProvider(async (request, _, token) =>
        {
            if (request.Url.EndsWith("/first", StringComparison.Ordinal))
            {
                started.TrySetResult();
                await release.Task.WaitAsync(token);
                return new DownloadResult(true, request.Url);
            }

            return new DownloadResult(true, request.Url);
        });
        var queue = new DownloadQueueService((_, token) => Task.Delay(Timeout.Infinite, token))
        {
            MaximumConcurrency = 1
        };
        var first = queue.Enqueue(new DownloadJob(
            provider,
            new DownloadRequest("https://example.com/first", Path.GetTempPath())));
        var pending = queue.Enqueue(new DownloadJob(
            provider,
            new DownloadRequest("https://example.com/pending", Path.GetTempPath())));

        try
        {
            await started.Task.WaitAsync(TimeSpan.FromSeconds(5));
            queue.Cancel(pending);
            var cancelledResult = await pending.Completion.WaitAsync(TimeSpan.FromSeconds(5));
            Assert.False(cancelledResult.Success);
            Assert.Equal(DownloadJobState.Cancelled, pending.State);

            release.TrySetResult();
            Assert.True((await first.Completion.WaitAsync(TimeSpan.FromSeconds(5))).Success);
        }
        finally
        {
            release.TrySetResult();
        }
    }

    [Fact]
    public async Task DownloadQueue_CancelsActiveJob()
    {
        var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var provider = new StubDownloadProvider(async (_, _, token) =>
        {
            started.TrySetResult();
            await Task.Delay(Timeout.Infinite, token);
            return new DownloadResult(true);
        });
        var queue = new DownloadQueueService();
        var job = queue.Enqueue(new DownloadJob(
            provider,
            new DownloadRequest("https://example.com/active", Path.GetTempPath())));

        await started.Task.WaitAsync(TimeSpan.FromSeconds(5));
        queue.Cancel(job);
        var result = await job.Completion.WaitAsync(TimeSpan.FromSeconds(5));

        Assert.False(result.Success);
        Assert.Equal(DownloadJobState.Cancelled, job.State);
    }

    [Fact]
    public void UserDataStore_RoundTripsSettingsAndHistory()
    {
        var directory = Path.Combine(Path.GetTempPath(), $"GowLink.UserData.{Guid.NewGuid():N}");
        var store = new UserDataStore(directory);
        var settings = new GowLinkSettings("C:\\Downloads\\GowLink", 128, "Cian", 3);
        var history = new[]
        {
            new DownloadHistoryRecord(
                "https://example.com/file.zip",
                "HTTP Directo (Streaming)",
                "Completada",
                100,
                "C:\\Downloads\\GowLink\\file.zip",
                DateTime.UtcNow)
        };

        try
        {
            store.SaveSettings(settings);
            store.SaveHistory(history);

            Assert.Equal(settings, store.LoadSettings());
            Assert.Equal(history, store.LoadHistory());
        }
        finally
        {
            if (Directory.Exists(directory))
                Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public void MainViewModel_ReportsCorruptPersistedHistoryWithoutDeletingIt()
    {
        var directory = Path.Combine(Path.GetTempPath(), $"GowLink.CorruptHistory.{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        var store = new UserDataStore(directory);
        var historyPath = Path.Combine(directory, "history.json");
        File.WriteAllText(historyPath, "{ invalid json");

        try
        {
            var viewModel = new MainViewModel(
                new CompositeDownloadProvider(new DirectHttpDownloadService(), new YtDlpDownloadService()),
                new StubClipboardService(),
                userDataStore: store);

            Assert.Contains("No se pudo leer el historial guardado", viewModel.SettingsMessage, StringComparison.Ordinal);
            Assert.True(File.Exists(historyPath));
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public void LoggerService_RemovesQueryAndFragmentFromUrls()
    {
        var sanitized = LoggerService.SanitizeUrls(
            "Failed URL=\"https://example.com/file.zip?token=private&download=1#fragment\"");

        Assert.Contains("https://example.com/file.zip", sanitized, StringComparison.Ordinal);
        Assert.DoesNotContain("private", sanitized, StringComparison.Ordinal);
        Assert.DoesNotContain("download=1", sanitized, StringComparison.Ordinal);
        Assert.DoesNotContain("fragment", sanitized, StringComparison.Ordinal);
    }

    private sealed class StubHttpMessageHandler(
        Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> sendAsync) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) => sendAsync(request,             cancellationToken);
    }

    private sealed class StubDownloadProvider(
        Func<DownloadRequest, IProgress<DownloadProgress>?, CancellationToken, Task<DownloadResult>> download) : IDownloadProvider
    {
        public string Name => "stub";

        public bool CanHandle(string url) => true;

        public Task<DownloadResult> DownloadAsync(
            DownloadRequest request,
            IProgress<DownloadProgress>? progress = null,
            CancellationToken cancellationToken = default) =>
            download(request, progress, cancellationToken);
    }

    private sealed class StubClipboardService : IClipboardService
    {
        public Task<string?> GetTextAsync() => Task.FromResult<string?>(null);

        public Task SetTextAsync(string text) => Task.CompletedTask;
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
        var audioFormat = Assert.Single(result, x => x.MediaType == DownloadMediaType.Audio && x.FormatId == "251");
        Assert.Contains("(estimado)", audioFormat.FormattedSize, StringComparison.Ordinal);
        Assert.Contains(result, x => x.MediaType == DownloadMediaType.Video && x.FormatId == "18" && x.HasAudio);
        var videoOnlyFormat = Assert.Single(
            result,
            x => x.MediaType == DownloadMediaType.Video && x.FormatId == "248" && !x.HasAudio);
        Assert.Equal("Tamaño desconocido", videoOnlyFormat.FormattedSize);
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
