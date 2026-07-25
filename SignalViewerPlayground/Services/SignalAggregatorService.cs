using System.Collections.ObjectModel;

using SignalViewerPlayground.Models;
using SignalViewerPlayground.Protocol;

namespace SignalViewerPlayground.Services;

/// <summary>
/// Aggregates a stream of decoded signals into records keyed by frequency/bandwidth.
/// Tracks a single current record: a matching signal increments its Count, and a
/// non-matching signal closes the record and starts a new one.
/// </summary>
public sealed class SignalAggregatorService
{
    public ObservableCollection<AggregatedSignalRecord> Records { get; } = new();

    private AggregatedSignalRecord? _current;

    public void AddSignal(FoundSignalPayload signal)
    {
        if (_current is not null && _current.Matches(signal))
        {
            _current.Count++;
            return;
        }

        _current = new AggregatedSignalRecord(signal);
        Records.Add(_current);
    }
}
