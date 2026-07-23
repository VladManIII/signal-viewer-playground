using System.Windows;
using System.ComponentModel;

namespace SignalViewerPlayground.Pages
{
    public interface IViewModelLifecycle
    {
        void Initialized(EventArgs args);
        void Loaded(RoutedEventArgs args);
        void ContentRendered(EventArgs args);
        void Activated(EventArgs args);
        void Deactivated(EventArgs args);
        void StateChanged(EventArgs args);
        void LocationChanged(EventArgs args);
        void SizeChanged(SizeChangedEventArgs args);
        void Closing(CancelEventArgs args);
        void Closed(EventArgs args);
        void Unloaded(RoutedEventArgs args);
    }
}
