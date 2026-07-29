using System.Windows;
using System.Windows.Data;
using System.Globalization;
using System.ComponentModel;
using System.Collections.ObjectModel;

using CommunityToolkit.Mvvm.ComponentModel;

using WPFLocalizeExtension.Engine;
using WPFLocalizeExtension.Extensions;

using SignalViewerPlayground.Pages;
using SignalViewerPlayground.Models;
using SignalViewerPlayground.Protocol;
using SignalViewerPlayground.Services;

namespace SignalViewerPlayground;

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

    private enum StatusKind
    {
        Started,
        Connecting,
        Connected,
        Error,
    }

    [ObservableProperty] string? _status;

    private StatusKind _statusKind = StatusKind.Started;
    private string? _statusErrorMessage;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsCustomRangeSelected))]
    FrequencyBandPreset _selectedBandPreset = FrequencyBandPreset.All;

    [ObservableProperty] double? _customMinMHz;
    [ObservableProperty] double? _customMaxMHz;

    [ObservableProperty] LanguageOption _selectedLanguage = LanguageOption.English;

    public IReadOnlyList<FrequencyBandPreset> BandPresets => FrequencyBandPreset.Presets;

    public IReadOnlyList<LanguageOption> Languages => LanguageOption.All;

    public bool IsCustomRangeSelected => SelectedBandPreset.Kind == FrequencyBandKind.Custom;

    public ObservableCollection<AggregatedSignalRecord> Records => _aggregator.Records;

    private readonly SignalAggregatorService _aggregator;
    private readonly CancellationTokenSource _tcpClientCts = new();

    private readonly ISignalClient _tcpSignalClient;
    private readonly IUiDispatcher _dispatcher;
    private readonly ICollectionView _recordsView;

    public MainWindowViewModel(
        SignalAggregatorService? aggregator = null,
        ISignalClient? tcpSignalClient = null,
        IUiDispatcher? dispatcher = null)
    {
        _aggregator = aggregator ?? new SignalAggregatorService();
        _tcpSignalClient = tcpSignalClient ?? new TcpSignalClient();
        _dispatcher = dispatcher ?? new WpfUiDispatcher();

        SetStatus(StatusKind.Started);

        LocalizeDictionary.Instance.PropertyChanged += OnCultureChanged;
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

    partial void OnSelectedLanguageChanged(LanguageOption value) =>
        LocalizeDictionary.Instance.Culture = CultureInfo.GetCultureInfo(value.CultureCode);

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

        LocalizeDictionary.Instance.PropertyChanged -= OnCultureChanged;
        _tcpSignalClient.SignalReceived -= OnSignalReceived;

        _tcpClientCts.Cancel();
        _aggregator.CloseCurrent();
    }

    private void OnSignalReceived(FoundSignalPayload signal)
    {
        _dispatcher.Invoke(() =>
        {
            SetStatus(StatusKind.Connected);
            _aggregator.AddSignal(signal);
        });
    }

    private async Task StartTcpStreamingAsync()
    {
        IsBusy = true;
        SetStatus(StatusKind.Connecting);

        try
        {
            await _tcpSignalClient.ConnectAndStreamAsync(TcpHost, TcpPort, _tcpClientCts.Token);
        }
        catch (OperationCanceledException) { /* expected on shutdown */ }
        catch (Exception ex) { SetStatus(StatusKind.Error, ex.Message); }
        finally { IsBusy = false; }
    }

    private void OnCultureChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(LocalizeDictionary.Culture)) return;

        RefreshStatusText();
    }

    private void SetStatus(StatusKind kind, string? errorMessage = null)
    {
        _statusKind = kind;
        _statusErrorMessage = errorMessage;

        RefreshStatusText();
    }

    private void RefreshStatusText()
    {
        Status = _statusKind switch
        {
            StatusKind.Started => Loc("StatusStarted"),
            StatusKind.Connecting => Loc("StatusConnecting"),
            StatusKind.Connected => Loc("StatusConnected"),
            StatusKind.Error => string.Format(Loc("StatusErrorFormat"), _statusErrorMessage),
            _ => string.Empty,
        };
    }

    private static string Loc(string key) =>
        LocExtension.GetLocalizedValue<string>($"SignalViewerPlayground:Resources.Languages.Strings:{key}") ?? key;
}