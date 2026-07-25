using System.Windows;

namespace SignalViewerPlayground.Services;

/// <summary>
/// Default <see cref="IUiDispatcher"/> backed by the running WPF application's dispatcher.
/// </summary>
public sealed class WpfUiDispatcher : IUiDispatcher
{
    public void Invoke(Action action) => Application.Current.Dispatcher.Invoke(action);
}
