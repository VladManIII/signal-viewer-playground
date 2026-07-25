using SignalViewerPlayground.Protocol;
using SignalViewerPlayground.Services;

namespace SignalViewerPlayground.Tests;

/// <summary>
/// Test double for <see cref="ISignalClient"/>: lets a test raise
/// <see cref="SignalReceived"/> directly, and configure whether
/// <see cref="ConnectAndStreamAsync"/> fails or just hangs (like a real,
/// still-open connection would) until cancelled.
/// </summary>
public sealed class FakeSignalClient : ISignalClient
{
    public event Action<FoundSignalPayload>? SignalReceived;

    private readonly TaskCompletionSource _pendingConnection = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private Exception? _connectException;

    public void SetConnectException(Exception exception) => _connectException = exception;

    public void RaiseSignalReceived(FoundSignalPayload payload) => SignalReceived?.Invoke(payload);

    public Task ConnectAndStreamAsync(string host, int port, CancellationToken cancellationToken)
    {
        if (_connectException is not null)
            throw _connectException;

        cancellationToken.Register(() => _pendingConnection.TrySetCanceled(cancellationToken));
        return _pendingConnection.Task;
    }
}
