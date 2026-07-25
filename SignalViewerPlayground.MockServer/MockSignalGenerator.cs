using SignalViewerPlayground.Protocol;

namespace SignalViewerPlayground.MockServer;

/// <summary>
/// Generates "found signal" payloads clustered into a small set of frequency/bandwidth
/// bands, with occasional outliers. Modeled as runs: several consecutive signals stay
/// on the same band (so a client's aggregation-by-record logic actually accumulates a
/// count), before switching to another band (reused or brand new) at the run boundary.
/// </summary>
public sealed class MockSignalGenerator(
    int? seed = null,
    int maxActiveBands = 6,
    double outlierProbability = 0.2,
    int minRunLength = 3,
    int maxRunLength = 12,
    double jitterFraction = 0.25)
{
    private sealed record Band(ulong CenterFrequencyHz, uint BandwidthHz);

    private readonly Random _random = seed.HasValue ? new Random(seed.Value) : new Random();
    private readonly List<Band> _activeBands = new();
    private Band? _currentBand;
    private int _remainingInRun;

    public FoundSignalPayload NextSignal()
    {
        if (_currentBand is null || _remainingInRun <= 0)
        {
            SelectNextBand();
            _remainingInRun = _random.Next(minRunLength, maxRunLength + 1);
        }
        _remainingInRun--;

        return new FoundSignalPayload(
            TimestampRaw: (ulong)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            FrequencyHz: JitterFrequency(_currentBand!),
            BandwidthHz: _currentBand!.BandwidthHz,
            SnrDb: 5 + _random.NextDouble() * 25); // ~5-30 dB
    }

    private void SelectNextBand()
    {
        bool spawnNew = _activeBands.Count == 0 || _random.NextDouble() < outlierProbability;
        var band = spawnNew ? CreateRandomBand() : _activeBands[_random.Next(_activeBands.Count)];

        if (spawnNew)
        {
            if (_activeBands.Count >= maxActiveBands)
                _activeBands[_random.Next(_activeBands.Count)] = band; // random existing frequency band
            else
                _activeBands.Add(band);
        }

        _currentBand = band;
    }

    private ulong JitterFrequency(Band band)
    {
        double halfBandwidth = band.BandwidthHz / 2.0;
        double jitter = (_random.NextDouble() * 2 - 1) * halfBandwidth * jitterFraction;
        return (ulong)Math.Max(0, band.CenterFrequencyHz + jitter);
    }

    private Band CreateRandomBand() => new(
        CenterFrequencyHz: (ulong)_random.NextInt64(0, 150_000_000), // 0 .. 150 MHz
        BandwidthHz: (uint)_random.Next(5_000, 200_000));             // ~5 kHz .. 200 kHz
}
