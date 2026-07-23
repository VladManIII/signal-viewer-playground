using System.Windows;

namespace SignalViewerPlayground.Pages
{
    public abstract class BaseWindow : Window
    {
        private IViewModelLifecycle? Model => DataContext as IViewModelLifecycle;

        public BaseWindow()
        {
            Initialized += (s, e) => Model?.Initialized(e);
            Loaded += (s, e) => Model?.Loaded(e);
            ContentRendered += (s, e) => Model?.ContentRendered(e);
            Activated += (s, e) => Model?.Activated(e);
            Deactivated += (s, e) => Model?.Deactivated(e);
            StateChanged += (s, e) => Model?.StateChanged(e);
            LocationChanged += (s, e) => Model?.LocationChanged(e);
            SizeChanged += (s, e) => Model?.SizeChanged(e);
            Closing += (s, e) => Model?.Closing(e);
            Closed += (s, e) => Model?.Closed(e);
            Unloaded += (s, e) => Model?.Unloaded(e);
        }
    }
}
