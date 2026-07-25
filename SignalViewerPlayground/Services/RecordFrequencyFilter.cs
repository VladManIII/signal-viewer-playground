using SignalViewerPlayground.Models;

namespace SignalViewerPlayground.Services;

/// <summary>
/// Decides whether an aggregated record's frequency falls within a selected
/// <see cref="FrequencyBandPreset"/>, including the open-ended "Custom range" case.
/// </summary>
public static class RecordFrequencyFilter
{
    public static bool Matches(AggregatedSignalRecord record, FrequencyBandPreset preset, double? customMinMHz, double? customMaxMHz)
    {
         return preset.Kind switch
        {
            FrequencyBandKind.All => true,
            FrequencyBandKind.Custom =>
                (customMinMHz is not double min || record.FrequencyMHz >= min) &&
                (customMaxMHz is not double max || record.FrequencyMHz <= max),
            FrequencyBandKind.Preset => record.FrequencyMHz >= preset.MinMHz && record.FrequencyMHz < preset.MaxMHz,
            _ => true,
        };
    }
}
