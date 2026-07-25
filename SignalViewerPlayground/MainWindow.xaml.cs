using System.Windows;
using System.Windows.Data;
using System.ComponentModel;

using CommunityToolkit.Mvvm.ComponentModel;

using SignalViewerPlayground.Pages;
using SignalViewerPlayground.Models;
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
        [ObservableProperty] FrequencyBandPreset _selectedBandPreset = FrequencyBandPreset.All;
        [ObservableProperty] double? _customMinMHz;
        [ObservableProperty] double? _customMaxMHz;

        public SignalAggregatorService Aggregator { get; } = new();

        public IReadOnlyList<FrequencyBandPreset> BandPresets => FrequencyBandPreset.Presets;

        public bool IsCustomRangeSelected => SelectedBandPreset.Kind == FrequencyBandKind.Custom;

        private readonly TcpSignalClient _tcpSignalClient = new();
        private readonly CancellationTokenSource _tcpClientCts = new();
        private readonly ICollectionView _recordsView;

        public MainWindowViewModel()
        {
            Status = "Application started.";
            _tcpSignalClient.SignalReceived += OnSignalReceived;

            _recordsView = CollectionViewSource.GetDefaultView(Aggregator.Records);
            _recordsView.Filter = FilterRecord;

            // FrequencyMHz can change after a record is already displayed (re-based to
            // the median once the record closes), so the filter needs to react to that.
            if (_recordsView is ICollectionViewLiveShaping liveShaping &&
                liveShaping.CanChangeLiveFiltering)
            {
                liveShaping.LiveFilteringProperties.Add(nameof(AggregatedSignalRecord.FrequencyMHz));
                liveShaping.IsLiveFiltering = true;
            }
        }

        partial void OnSelectedBandPresetChanged(FrequencyBandPreset value)
        {
            OnPropertyChanged(nameof(IsCustomRangeSelected));
            _recordsView.Refresh();
        }

        partial void OnCustomMinMHzChanged(double? value) => _recordsView.Refresh();

        partial void OnCustomMaxMHzChanged(double? value) => _recordsView.Refresh();

        private bool FilterRecord(object obj)
        {
            return obj is AggregatedSignalRecord record &&
                   RecordFrequencyFilter.Matches(record, SelectedBandPreset, CustomMinMHz, CustomMaxMHz);
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
            Aggregator.CloseCurrent();
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
            catch (OperationCanceledException) { /* expected on shutdown */ }
            catch (Exception ex) { Status = $"Error: {ex.Message}"; }
            finally { IsBusy = false; }
        }
    }
}