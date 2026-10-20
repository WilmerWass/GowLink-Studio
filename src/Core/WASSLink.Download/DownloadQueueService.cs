using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WASSLink.Abstractions;

namespace WASSLink.Download;

public sealed class DownloadQueueService
{
    private static readonly TimeSpan[] RetryDelays =
    [
        TimeSpan.FromSeconds(1),
        TimeSpan.FromSeconds(2)
    ];

    private readonly object _sync = new();
    private readonly Queue<DownloadJob> _pending = new();
    private readonly Func<TimeSpan, CancellationToken, Task> _delay;
    private int _maximumConcurrency = 2;
    private int _activeCount;

    public DownloadQueueService()
        : this(Task.Delay)
    {
    }

    public DownloadQueueService(Func<TimeSpan, CancellationToken, Task> delay)
    {
        _delay = delay ?? throw new ArgumentNullException(nameof(delay));
    }

    public event EventHandler? QueueChanged;

    public int MaximumConcurrency
    {
        get
        {
            lock (_sync)
                return _maximumConcurrency;
        }
        set
        {
            lock (_sync)
                _maximumConcurrency = Math.Clamp(value, 1, 3);
            Pump();
            QueueChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public int ActiveCount
    {
        get
        {
            lock (_sync)
                return _activeCount;
        }
    }

    public DownloadJob Enqueue(DownloadJob job)
    {
        ArgumentNullException.ThrowIfNull(job);
        lock (_sync)
            _pending.Enqueue(job);

        Pump();
        QueueChanged?.Invoke(this, EventArgs.Empty);
        return job;
    }

    public void Cancel(DownloadJob job)
    {
        ArgumentNullException.ThrowIfNull(job);
        var wasPending = false;
        lock (_sync)
        {
            var remaining = _pending.Where(candidate => candidate != job).ToArray();
            wasPending = remaining.Length != _pending.Count;
            if (wasPending)
            {
                _pending.Clear();
                foreach (var candidate in remaining)
                    _pending.Enqueue(candidate);
            }
        }

        if (wasPending)
        {
            job.SetState(DownloadJobState.Cancelled);
            job.Complete(new DownloadResult(false, null, "Descarga cancelada por el usuario."));
            job.DisposeCancellation();
        }
        else
        {
            job.Cancel();
        }

        QueueChanged?.Invoke(this, EventArgs.Empty);
    }

    private void Pump()
    {
        List<DownloadJob> starting = [];
        lock (_sync)
        {
            while (_activeCount < _maximumConcurrency && _pending.Count > 0)
            {
                var job = _pending.Dequeue();
                if (job.CancellationToken.IsCancellationRequested)
                {
                    job.SetState(DownloadJobState.Cancelled);
                    job.Complete(new DownloadResult(false, null, "Descarga cancelada por el usuario."));
                    job.DisposeCancellation();
                    continue;
                }

                _activeCount++;
                starting.Add(job);
            }
        }

        foreach (var job in starting)
            _ = ExecuteJobAsync(job);
    }

    private async Task ExecuteJobAsync(DownloadJob job)
    {
        DownloadResult result = new(false, null, "No se pudo completar la descarga.");
        try
        {
            for (var attempt = 1; attempt <= 3; attempt++)
            {
                job.CancellationToken.ThrowIfCancellationRequested();
                if (attempt > 1)
                {
                    job.SetState(DownloadJobState.Retrying);
                    await _delay(RetryDelays[attempt - 2], job.CancellationToken);
                }

                job.SetAttempt(attempt);
                job.SetState(DownloadJobState.Downloading);
                var request = job.Request with
                {
                    WaitIfPausedAsync = job.WaitIfPausedAsync
                };
                var progress = new Progress<DownloadProgress>(job.SetProgress);
                result = await job.Provider.DownloadAsync(request, progress, job.CancellationToken);

                if (result.Success || !result.IsTransientFailure || attempt == 3)
                    break;
            }
        }
        catch (OperationCanceledException)
        {
            result = new DownloadResult(false, null, "Descarga cancelada por el usuario.");
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidOperationException)
        {
            LoggerService.WriteException(ex, $"DOWNLOAD QUEUE FAILURE JOB=\"{job.Id}\"");
            result = new DownloadResult(false, null, ex.Message);
        }
        catch (Exception ex)
        {
            LoggerService.WriteException(ex, $"DOWNLOAD QUEUE UNEXPECTED FAILURE JOB=\"{job.Id}\"");
            result = new DownloadResult(false, null, $"Error inesperado: {ex.Message}");
        }
        finally
        {
            if (job.CancellationToken.IsCancellationRequested)
                job.SetState(DownloadJobState.Cancelled);
            else if (result.Success)
                job.SetState(result.WasAlreadyPresent ? DownloadJobState.Skipped : DownloadJobState.Completed);
            else
                job.SetState(DownloadJobState.Failed);

            job.Complete(result);
            job.DisposeCancellation();
            lock (_sync)
                _activeCount--;

            Pump();
            QueueChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
