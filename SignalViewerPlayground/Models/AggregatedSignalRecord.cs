using CommunityToolkit.Mvvm.ComponentModel;

using SignalViewerPlayground.Protocol;

namespace SignalViewerPlayground.Models;

/// <summary>
/// One aggregated table row: a run of consecutive signals that share the same
/// frequency/bandwidth range. Timestamp/Frequency/Bandwidth/SNR are fixed from the
/// first signal in the run; only Count grows as further matching signals arrive.
/// </summary>
public partial class AggregatedSignalRecord : ObservableObject
{
    public DateTimeOffset Timestamp { get; }
    public ulong FrequencyHz { get; }
    public uint BandwidthHz { get; }
    public double FrequencyMHz { get; }
    public double BandwidthKHz { get; }
    public double SnrDb { get; }

    [ObservableProperty]
    private int _count;

    public AggregatedSignalRecord(FoundSignalPayload firstSignal)
    {
        Timestamp = DateTimeOffset.FromUnixTimeMilliseconds((long)firstSignal.TimestampRaw);
        FrequencyHz = firstSignal.FrequencyHz;
        BandwidthHz = firstSignal.BandwidthHz;
        FrequencyMHz = firstSignal.FrequencyHz / 1_000_000.0; // convert Hz -> MHz // (10^6)
        BandwidthKHz = firstSignal.BandwidthHz / 1_000.0; // convert Hz -> KHz // (10^3)
        SnrDb = firstSignal.SnrDb;
        Count = 1;
    }

    /// <summary>
    /// Whether the given signal's frequency falls into the specified range
    /// [FrequencyHz - BandwidthHz/2, FrequencyHz + BandwidthHz/2).
    /// </summary>
    public bool Matches(FoundSignalPayload signal)
    {
        double range = BandwidthHz / 2.0;
        double lower = FrequencyHz - range;
        double upper = FrequencyHz + range;

        return signal.FrequencyHz >= lower && signal.FrequencyHz < upper;
    }
}
