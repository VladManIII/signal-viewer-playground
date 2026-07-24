using System.Runtime.InteropServices;
using System.Windows;

using CommunityToolkit.Mvvm.ComponentModel;

using SignalViewerPlayground.Pages;
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

        private readonly TcpSignalClient _tcpSignalClient = new();
        private readonly CancellationTokenSource _tcpClientCts = new();

        //[DllImport("kernel32.dll")]
        //private static extern bool AllocConsole();

        public MainWindowViewModel()
        {
            Status = "Application started.";
        }

        protected override async void OnLoaded(RoutedEventArgs args)
        {
            base.OnLoaded(args);

            //AllocConsole();
            _ = StartTcpStreamingAsync();

            IsBusy = true;

            try
            {
                Status = "Loading data...";
                await LoadData();
                Status = "Data loaded successfully.";
            }
            catch (Exception ex)
            {
                Status = $"Error loading data: {ex.Message}";
            }
            finally { IsBusy = false; }
        }

        protected override void OnClosed(EventArgs args)
        {
            base.OnClosed(args);

            _tcpClientCts.Cancel();
        }

        private async Task StartTcpStreamingAsync()
        {
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
                Console.WriteLine($"[TcpSignalClient] Stopped: {ex.Message}");
            }
        }

        private Task LoadData()
        {
            return Task.Delay(5000);
        }
    }
}