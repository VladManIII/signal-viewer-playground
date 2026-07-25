namespace SignalViewerPlayground.Services;

/// <summary>
/// Abstraction over marshaling a callback onto the UI thread, so ViewModels
/// don't need a live WPF Application instance to be tested.
/// </summary>
public interface IUiDispatcher
{
    void Invoke(Action action);
}
