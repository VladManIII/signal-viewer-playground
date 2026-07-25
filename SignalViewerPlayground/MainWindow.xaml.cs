using System.Windows;
using System.Windows.Data;
using System.ComponentModel;
using System.Collections.ObjectModel;

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

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsCustomRangeSelected))]
        FrequencyBandPreset _selectedBandPreset = FrequencyBandPreset.All;

        [ObservableProperty] double? _customMinMHz;
        [ObservableProperty] double? _customMaxMHz;

        public IReadOnlyList<FrequencyBandPreset> BandPresets => FrequencyBandPreset.Presets;

        public bool IsCustomRangeSelected => SelectedBandPreset.Kind == FrequencyBandKind.Custom;

        public ObservableCollection<AggregatedSignalRecord> Records => _aggregator.Records;

        private readonly SignalAggregatorService _aggregator;
        private readonly TcpSignalClient _tcpSignalClient;
        private readonly CancellationTokenSource _tcpClientCts = new();
        private readonly IUiDispatcher _dispatcher;
        private readonly ICollectionView _recordsView;

        public MainWindowViewModel(
            SignalAggregatorService? aggregator = null,
            TcpSignalClient? tcpSignalClient = null,
            IUiDispatcher? dispatcher = null)
        {
            _aggregator = aggregator ?? new SignalAggregatorService();
            _tcpSignalClient = tcpSignalClient ?? new TcpSignalClient();
            _dispatcher = dispatcher ?? new WpfUiDispatcher();

            Status = "Application started.";
            _tcpSignalClient.SignalReceived += OnSignalReceived;

            _recordsView = CollectionViewSource.GetDefaultView(_aggregator.Records);
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

        partial void OnSelectedBandPresetChanged(FrequencyBandPreset value) => _recordsView.Refresh();

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
            _aggregator.CloseCurrent();
        }

        private void OnSignalReceived(FoundSignalPayload signal)
        {
            _dispatcher.Invoke(() =>
            {
                Status = "Connected, receiving signals...";
                _aggregator.AddSignal(signal);
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