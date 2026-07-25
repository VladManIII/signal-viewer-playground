using SignalViewerPlayground.Models;
using SignalViewerPlayground.Protocol;
using SignalViewerPlayground.Services;

namespace SignalViewerPlayground.Tests;

[TestFixture]
public class RecordFrequencyFilterTests
{
    private static AggregatedSignalRecord Record(double frequencyMHz) =>
        new(new FoundSignalPayload(TimestampRaw: 0, FrequencyHz: (ulong)(frequencyMHz * 1_000_000), BandwidthHz: 20_000, SnrDb: 15.0));

    [Test]
    public void Matches_AllPreset_AlwaysReturnsTrue()
    {
        var record = Record(500);

        Assert.That(RecordFrequencyFilter.Matches(record, FrequencyBandPreset.All, null, null), Is.True);
    }

    [TestCase(5.5, true)]
    [TestCase(6.999, true)]
    [TestCase(7.0, false)] // upper bound is exclusive
    [TestCase(4.0, false)] // below lower bound
    public void Matches_NamedPreset_UsesHalfOpenRange(double frequencyMHz, bool expected)
    {
        var preset = FrequencyBandPreset.Presets.Single(p => p.Name == "5.5 - 7.0 MHz");
        var record = Record(frequencyMHz);

        Assert.That(RecordFrequencyFilter.Matches(record, preset, null, null), Is.EqualTo(expected));
    }

    [Test]
    public void Matches_CustomRange_BothBoundsInclusive()
    {
        Assert.That(RecordFrequencyFilter.Matches(Record(10), FrequencyBandPreset.Custom, 10, 20), Is.True);
        Assert.That(RecordFrequencyFilter.Matches(Record(20), FrequencyBandPreset.Custom, 10, 20), Is.True);
        Assert.That(RecordFrequencyFilter.Matches(Record(9.99), FrequencyBandPreset.Custom, 10, 20), Is.False);
        Assert.That(RecordFrequencyFilter.Matches(Record(20.01), FrequencyBandPreset.Custom, 10, 20), Is.False);
    }

    [Test]
    public void Matches_CustomRange_OpenEndedWhenBoundMissing()
    {
        Assert.That(RecordFrequencyFilter.Matches(Record(1_000), FrequencyBandPreset.Custom, 10, null), Is.True);
        Assert.That(RecordFrequencyFilter.Matches(Record(1), FrequencyBandPreset.Custom, null, 10), Is.True);
        Assert.That(RecordFrequencyFilter.Matches(Record(5), FrequencyBandPreset.Custom, null, null), Is.True);
    }
}
