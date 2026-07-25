using System.Windows;

using SignalViewerPlayground.Pages;
using SignalViewerPlayground.Protocol;

namespace SignalViewerPlayground.Tests;

[TestFixture]
public class MainWindowViewModelTests
{
    private static FoundSignalPayload Signal(ulong frequencyHz, uint bandwidthHz = 20_000, double snrDb = 15.0) =>
        new(TimestampRaw: 0, FrequencyHz: frequencyHz, BandwidthHz: bandwidthHz, SnrDb: snrDb);

    [Test]
    public void OnSignalReceived_UpdatesStatusAndAggregatesRecord()
    {
        var signalClient = new FakeSignalClient();
        var viewModel = new MainWindowViewModel(tcpSignalClient: signalClient, dispatcher: new FakeUiDispatcher());

        signalClient.RaiseSignalReceived(Signal(100_000_000));

        Assert.That(viewModel.Status, Is.EqualTo("Connected, receiving signals..."));
        Assert.That(viewModel.Records, Has.Count.EqualTo(1));
        Assert.That(viewModel.Records[0].FrequencyHz, Is.EqualTo(100_000_000));
    }

    [Test]
    public void Loaded_ConnectFails_SetsErrorStatusAndClearsIsBusy()
    {
        var signalClient = new FakeSignalClient();
        signalClient.SetConnectException(new InvalidOperationException("boom"));
        var viewModel = new MainWindowViewModel(tcpSignalClient: signalClient, dispatcher: new FakeUiDispatcher());

        ((IViewModelLifecycle)viewModel).Loaded(new RoutedEventArgs());

        Assert.That(viewModel.Status, Is.EqualTo("Error: boom"));
        Assert.That(viewModel.IsBusy, Is.False);
    }

    [Test]
    public void Closed_FinalizesStillOpenRecordToMedian()
    {
        var signalClient = new FakeSignalClient();
        var viewModel = new MainWindowViewModel(tcpSignalClient: signalClient, dispatcher: new FakeUiDispatcher());

        // Band defined by the first signal: 100_000_000 Hz, 20_000 Hz bandwidth.
        signalClient.RaiseSignalReceived(Signal(frequencyHz: 100_000_000, bandwidthHz: 20_000));
        signalClient.RaiseSignalReceived(Signal(frequencyHz: 100_008_000, bandwidthHz: 20_000));
        signalClient.RaiseSignalReceived(Signal(frequencyHz: 99_992_000, bandwidthHz: 20_000));

        ((IViewModelLifecycle)viewModel).Closed(EventArgs.Empty);

        // Sorted matched frequencies: 99_992_000, 100_000_000, 100_008_000 -> median is the middle value.
        Assert.That(viewModel.Records, Has.Count.EqualTo(1));
        Assert.That(viewModel.Records[0].FrequencyMHz, Is.EqualTo(100.0).Within(1e-9));
    }
}
