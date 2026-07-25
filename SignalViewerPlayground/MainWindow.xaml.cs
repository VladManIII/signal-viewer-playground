using System.Windows;

using CommunityToolkit.Mvvm.ComponentModel;

using SignalViewerPlayground.Pages;
using SignalViewerPlayground.Protocol;
using SignalViewerPlayground.Services;

namespace SignalViewerPlayground
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : BaseWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel();
        }
    }

    public partial class MainWindowViewModel : BaseViewModel
    {
        private const string TcpHost = "127.0.0.1";
        private const int TcpPort = 1488;

        [ObservableProperty] string _status;

        public SignalAggregatorService Aggregator { get; } = new();

        private readonly TcpSignalClient _tcpSignalClient = new();
        private readonly CancellationTokenSource _tcpClientCts = new();

        public MainWindowViewModel()
        {
            Status = "Application started.";
            _tcpSignalClient.SignalReceived += OnSignalReceived;
        }

        protected override void OnLoaded(RoutedEventArgs args)
        {
            base.OnLoaded(args);

            _ = StartTcpStreamingAsync();
        }

        protected override void OnClosed(EventArgs args)
        {
            base.OnClosed(args);

            _tcpSignalClient.SignalReceived -= OnSignalReceived;
            _tcpClientCts.Cancel();
        }

        private void OnSignalReceived(FoundSignalPayload signal)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Status = "Connected, receiving signals...";
                Aggregator.AddSignal(signal);
            });
        }

        private async Task StartTcpStreamingAsync()
        {
            IsBusy = true;
            Status = "Connecting...";

            try
            {
                await _tcpSignalClient.ConnectAndStreamAsync(TcpHost, TcpPort, _tcpClientCts.Token);
            }
            catch (OperationCanceledException)
            {
                // expected on shutdown
            }
            catch (Exception ex)
            {
                Status = $"Error: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}