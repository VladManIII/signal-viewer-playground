using CommunityToolkit.Mvvm.ComponentModel;

using SignalViewerPlayground.Protocol;

namespace SignalViewerPlayground.Models;

/// <summary>
/// One aggregated table row: a run of consecutive signals that share the same
/// frequency/bandwidth range. Timestamp/Bandwidth/SNR are fixed from the first
/// signal in the run; Count grows as further matching signals arrive, and
/// FrequencyMHz is re-based to the median of all matched signals once the
/// record is closed (superseded by a new record, or the stream ends).
/// </summary>
public partial class AggregatedSignalRecord : ObservableObject
{
    public DateTimeOffset Timestamp { get; }
    public ulong FrequencyHz { get; }
    public uint BandwidthHz { get; }
    public double BandwidthKHz { get; }
    public double SnrDb { get; }

    [ObservableProperty]
    private double _frequencyMHz;

    [ObservableProperty]
    private int _count;

    private readonly double loverRange;
    private readonly double upperRange;

    private readonly List<ulong> _matchedFrequenciesHz = new();

    public AggregatedSignalRecord(FoundSignalPayload firstSignal)
    {
        Timestamp = DateTimeOffset.FromUnixTimeMilliseconds((long)firstSignal.TimestampRaw);
        FrequencyHz = firstSignal.FrequencyHz;
        BandwidthHz = firstSignal.BandwidthHz;
        FrequencyMHz = firstSignal.FrequencyHz / 1_000_000.0; // convert Hz -> MHz // (10^6)
        BandwidthKHz = firstSignal.BandwidthHz / 1_000.0; // convert Hz -> KHz // (10^3)
        SnrDb = firstSignal.SnrDb;
        Count = 1;

        double range = BandwidthHz / 2.0;
        loverRange = FrequencyHz - range;
        upperRange = FrequencyHz + range;

        _matchedFrequenciesHz.Add(firstSignal.FrequencyHz);
    }

    /// <summary>
    /// Whether the given signal's frequency falls into the specified range
    /// [FrequencyHz - BandwidthHz/2, FrequencyHz + BandwidthHz/2).
    /// </summary>
    public bool Matches(ulong frequencyHz) => frequencyHz >= loverRange && frequencyHz < upperRange;

    /// <summary>
    /// Records a further signal that matched this record's band.
    /// </summary>
    public void AddMatchingSignal(FoundSignalPayload signal)
    {
        Count++;
        _matchedFrequenciesHz.Add(signal.FrequencyHz);
        FrequencyMHz = MedianHz(_matchedFrequenciesHz) / 1_000_000.0;
    }

    private static double MedianHz(List<ulong> frequenciesHz)
    {
        var sorted = frequenciesHz.OrderBy(f => f).ToList();
        int mid = sorted.Count / 2;

        return sorted.Count % 2 == 1
            ? sorted[mid]
            : (sorted[mid - 1] + sorted[mid]) / 2.0;
    }
}
