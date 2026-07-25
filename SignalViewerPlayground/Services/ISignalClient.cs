using SignalViewerPlayground.Protocol;

namespace SignalViewerPlayground.Services;

/// <summary>
/// Abstraction over a source of decoded signals, so ViewModels can be tested
/// with a fake stream instead of a live TCP connection.
/// </summary>
public interface ISignalClient
{
    event Action<FoundSignalPayload>? SignalReceived;

    Task ConnectAndStreamAsync(string host, int port, CancellationToken cancellationToken);
}
