using SignalViewerPlayground.Protocol;
using SignalViewerPlayground.Services;

namespace SignalViewerPlayground.Tests;

[TestFixture]
public class SignalAggregatorServiceTests
{
    private static FoundSignalPayload Signal(ulong frequencyHz, uint bandwidthHz = 20_000, ulong timestampRaw = 0, double snrDb = 15.0) =>
        new(timestampRaw, frequencyHz, bandwidthHz, snrDb);

    [Test]
    public void AddSignal_FirstSignal_CreatesOneRecordWithCountOne()
    {
        var aggregator = new SignalAggregatorService();
        var signal = Signal(100_000_000);

        aggregator.AddSignal(signal);

        Assert.That(aggregator.Records, Has.Count.EqualTo(1));
        var record = aggregator.Records[0];
        Assert.That(record.Count, Is.EqualTo(1));
        Assert.That(record.FrequencyHz, Is.EqualTo(signal.FrequencyHz));
        Assert.That(record.BandwidthHz, Is.EqualTo(signal.BandwidthHz));
        Assert.That(record.SnrDb, Is.EqualTo(signal.SnrDb));
    }

    [Test]
    public void AddSignal_SignalWithinBand_IncrementsCountAndKeepsFirstSignalValues()
    {
        var aggregator = new SignalAggregatorService();
        var first = Signal(frequencyHz: 100_000_000, bandwidthHz: 20_000, snrDb: 15.0);
        var second = Signal(frequencyHz: 100_005_000, bandwidthHz: 5_000, snrDb: 99.0); // within [90M+10k boundaries), different bw/snr

        aggregator.AddSignal(first);
        aggregator.AddSignal(second);

        Assert.That(aggregator.Records, Has.Count.EqualTo(1));
        var record = aggregator.Records[0];
        Assert.That(record.Count, Is.EqualTo(2));
        Assert.That(record.FrequencyHz, Is.EqualTo(first.FrequencyHz));
        Assert.That(record.BandwidthHz, Is.EqualTo(first.BandwidthHz));
        Assert.That(record.SnrDb, Is.EqualTo(first.SnrDb));
    }

    [Test]
    public void AddSignal_FrequencyAtLowerBound_IsIncludedInBand()
    {
        var aggregator = new SignalAggregatorService();
        var first = Signal(frequencyHz: 100_000_000, bandwidthHz: 20_000); // band: [99_990_000, 100_010_000)

        aggregator.AddSignal(first);
        aggregator.AddSignal(Signal(frequencyHz: 99_990_000));

        Assert.That(aggregator.Records, Has.Count.EqualTo(1));
        Assert.That(aggregator.Records[0].Count, Is.EqualTo(2));
    }

    [Test]
    public void AddSignal_FrequencyAtUpperBound_IsExcludedFromBand()
    {
        var aggregator = new SignalAggregatorService();
        var first = Signal(frequencyHz: 100_000_000, bandwidthHz: 20_000); // band: [99_990_000, 100_010_000)

        aggregator.AddSignal(first);
        aggregator.AddSignal(Signal(frequencyHz: 100_010_000));

        Assert.That(aggregator.Records, Has.Count.EqualTo(2));
        Assert.That(aggregator.Records[0].Count, Is.EqualTo(1));
        Assert.That(aggregator.Records[1].Count, Is.EqualTo(1));
    }

    [Test]
    public void AddSignal_SignalOutsideBand_ClosesCurrentRecordAndStartsNewOne()
    {
        var aggregator = new SignalAggregatorService();

        aggregator.AddSignal(Signal(frequencyHz: 100_000_000, bandwidthHz: 20_000));
        aggregator.AddSignal(Signal(frequencyHz: 200_000_000, bandwidthHz: 20_000));

        Assert.That(aggregator.Records, Has.Count.EqualTo(2));
        Assert.That(aggregator.Records[0].Count, Is.EqualTo(1));
        Assert.That(aggregator.Records[0].FrequencyHz, Is.EqualTo(100_000_000));
        Assert.That(aggregator.Records[1].Count, Is.EqualTo(1));
        Assert.That(aggregator.Records[1].FrequencyHz, Is.EqualTo(200_000_000));
    }

    [Test]
    public void AddSignal_MixedSequence_ProducesExpectedRecordsAndCounts()
    {
        var aggregator = new SignalAggregatorService();

        // Run of 3 signals on band A (100MHz, 20kHz bandwidth)
        aggregator.AddSignal(Signal(frequencyHz: 100_000_000, bandwidthHz: 20_000));
        aggregator.AddSignal(Signal(frequencyHz: 100_002_000, bandwidthHz: 20_000));
        aggregator.AddSignal(Signal(frequencyHz: 99_995_000, bandwidthHz: 20_000));

        // Switch to band B (500MHz, 50kHz bandwidth), run of 2
        aggregator.AddSignal(Signal(frequencyHz: 500_000_000, bandwidthHz: 50_000));
        aggregator.AddSignal(Signal(frequencyHz: 500_010_000, bandwidthHz: 50_000));

        // Back to a band overlapping band A's original frequency - still a new record (sequential rule, not lookup)
        aggregator.AddSignal(Signal(frequencyHz: 100_000_000, bandwidthHz: 20_000));

        Assert.That(aggregator.Records, Has.Count.EqualTo(3));
        Assert.That(aggregator.Records[0].Count, Is.EqualTo(3));
        Assert.That(aggregator.Records[1].Count, Is.EqualTo(2));
        Assert.That(aggregator.Records[2].Count, Is.EqualTo(1));
    }
}
