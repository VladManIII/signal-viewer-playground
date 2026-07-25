using SignalViewerPlayground.Services;

namespace SignalViewerPlayground.Tests;

/// <summary>
/// Runs dispatched actions synchronously and inline, so ViewModel tests
/// don't need a live WPF Application/Dispatcher.
/// </summary>
public sealed class FakeUiDispatcher : IUiDispatcher
{
    public void Invoke(Action action) => action();
}
