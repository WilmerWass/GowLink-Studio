using System;
using System.Threading;
using System.Threading.Tasks;
using WASSLink.Abstractions;

namespace WASSLink.Download;

public enum DownloadJobState
{
    Pending,
    Downloading,
    Paused,
    Retrying,
    Completed,
    Skipped,
    Failed,
    Cancelled
}

public sealed class DownloadJob
{
    private readonly CancellationTokenSource _cancellation = new();
    private readonly object _pauseLock = new();
    private TaskCompletionSource _resumeSignal = CompletedSignal();
    private readonly TaskCompletionSource<DownloadResult> _completion =
        new(TaskCreationOptions.RunContinuationsAsynchronously);

    public DownloadJob(IDownloadProvider provider, DownloadRequest request)
    {
        Provider = provider ?? throw new ArgumentNullException(nameof(provider));
        Request = request ?? throw new ArgumentNullException(nameof(request));
    }

    public Guid Id { get; } = Guid.NewGuid();
    public IDownloadProvider Provider { get; }
    public DownloadRequest Request { get; }
    public DownloadJobState State { get; private set; } = DownloadJobState.Pending;
    public int Attempt { get; private set; }
    public DownloadProgress? LastProgress { get; private set; }
    public Task<DownloadResult> Completion => _completion.Task;
    internal CancellationToken CancellationToken => _cancellation.Token;
    public event EventHandler? Changed;

    public void Cancel()
    {
        if (_completion.Task.IsCompleted)
            return;

        try
        {
            _cancellation.Cancel();
        }
        catch (ObjectDisposedException)
        {
        }
    }

    public void SetPaused(bool paused)
    {
        lock (_pauseLock)
        {
            if (paused && State == DownloadJobState.Downloading)
            {
                _resumeSignal = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
                State = DownloadJobState.Paused;
            }
            else if (!paused && State == DownloadJobState.Paused)
            {
                _resumeSignal.TrySetResult();
                State = DownloadJobState.Downloading;
            }
        }

        Changed?.Invoke(this, EventArgs.Empty);
    }

    internal async Task WaitIfPausedAsync(CancellationToken cancellationToken)
    {
        Task waitTask;
        lock (_pauseLock)
            waitTask = _resumeSignal.Task;

        await waitTask.WaitAsync(cancellationToken);
    }

    internal void SetState(DownloadJobState state)
    {
        State = state;
        Changed?.Invoke(this, EventArgs.Empty);
    }

    internal void SetAttempt(int attempt)
    {
        Attempt = attempt;
        Changed?.Invoke(this, EventArgs.Empty);
    }

    internal void SetProgress(DownloadProgress progress)
    {
        LastProgress = progress;
        Changed?.Invoke(this, EventArgs.Empty);
    }

    internal void Complete(DownloadResult result) => _completion.TrySetResult(result);

    internal void DisposeCancellation() => _cancellation.Dispose();

    private static TaskCompletionSource CompletedSignal()
    {
        var signal = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        signal.SetResult();
        return signal;
    }
}
