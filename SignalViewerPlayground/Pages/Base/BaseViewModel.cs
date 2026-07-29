using System.Windows;
using System.ComponentModel;

using CommunityToolkit.Mvvm.ComponentModel;


namespace SignalViewerPlayground.Pages;

public abstract partial class BaseViewModel : ObservableObject, IViewModelLifecycle
{
    public BaseViewModel() { }

    [ObservableProperty] bool _isInitialized;
    [ObservableProperty] bool _isBusy;

    protected virtual void OnInitialized(EventArgs args) { }
    protected virtual void OnLoaded(RoutedEventArgs args) { }
    protected virtual void OnContentRendered(EventArgs args) { }
    protected virtual void OnActivated(EventArgs args) { }
    protected virtual void OnDeactivated(EventArgs args) { }
    protected virtual void OnStateChanged(EventArgs args) { }
    protected virtual void OnLocationChanged(EventArgs args) { }
    protected virtual void OnSizeChanged(SizeChangedEventArgs args) { }
    protected virtual void OnClosing(CancelEventArgs args) { }
    protected virtual void OnClosed(EventArgs args) { }
    protected virtual void OnUnloaded(RoutedEventArgs args) { }

    void IViewModelLifecycle.Initialized(EventArgs args) => OnInitialized(args);
    void IViewModelLifecycle.Loaded(RoutedEventArgs args) => OnLoaded(args);
    void IViewModelLifecycle.ContentRendered(EventArgs args) => OnContentRendered(args);
    void IViewModelLifecycle.Activated(EventArgs args) => OnActivated(args);
    void IViewModelLifecycle.Deactivated(EventArgs args) => OnDeactivated(args);
    void IViewModelLifecycle.StateChanged(EventArgs args) => OnStateChanged(args);
    void IViewModelLifecycle.LocationChanged(EventArgs args) => OnLocationChanged(args);
    void IViewModelLifecycle.SizeChanged(SizeChangedEventArgs args) => OnSizeChanged(args);
    void IViewModelLifecycle.Closing(CancelEventArgs args) => OnClosing(args);
    void IViewModelLifecycle.Closed(EventArgs args) => OnClosed(args);
    void IViewModelLifecycle.Unloaded(RoutedEventArgs args) => OnUnloaded(args);
}
