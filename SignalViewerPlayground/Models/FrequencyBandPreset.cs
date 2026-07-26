using CommunityToolkit.Mvvm.ComponentModel;

using WPFLocalizeExtension.Engine;
using WPFLocalizeExtension.Extensions;

namespace SignalViewerPlayground.Models;

public enum FrequencyBandKind
{
    All,
    Preset,
    Custom,
}

/// <summary>
/// A selectable frequency filter option for the signal table.
/// </summary>
public sealed class FrequencyBandPreset : ObservableObject
{
    public string RawName { get; }

    public FrequencyBandKind Kind { get; }

    public double MinMHz { get; }

    public double MaxMHz { get; }

    public FrequencyBandPreset(string rawName, FrequencyBandKind kind, double minMHz = 0, double maxMHz = 0)
    {
        RawName = rawName;
        Kind = kind;
        MinMHz = minMHz;
        MaxMHz = maxMHz;
    }

    public string Name => Kind switch
    {
        FrequencyBandKind.All => LocExtension.GetLocalizedValue<string>("SignalViewerPlayground:Resources.Languages.Strings:BandPresetAll") ??
                                 RawName,
        FrequencyBandKind.Custom => LocExtension.GetLocalizedValue<string>("SignalViewerPlayground:Resources.Languages.Strings:BandPresetCustom") ??
                                 RawName,
        _ => RawName,
    };

    public static readonly FrequencyBandPreset All = new("All", FrequencyBandKind.All);
    public static readonly FrequencyBandPreset Custom = new("Custom range...", FrequencyBandKind.Custom);

    static FrequencyBandPreset()
    {
        LocalizeDictionary.Instance.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName != nameof(LocalizeDictionary.Culture)) return;

            All.OnPropertyChanged(nameof(Name));
            Custom.OnPropertyChanged(nameof(Name));
        };
    }

    public static readonly IReadOnlyList<FrequencyBandPreset> Presets = new[]
    {
        All,
        new FrequencyBandPreset("0 - 1.8 MHz", FrequencyBandKind.Preset, 0, 1.8),
        new FrequencyBandPreset("1.8 - 2.8 MHz", FrequencyBandKind.Preset, 1.8, 2.8),
        new FrequencyBandPreset("2.8 - 4.0 MHz", FrequencyBandKind.Preset, 2.8, 4.0),
        new FrequencyBandPreset("4.0 - 5.5 MHz", FrequencyBandKind.Preset, 4.0, 5.5),
        new FrequencyBandPreset("5.5 - 7.0 MHz", FrequencyBandKind.Preset, 5.5, 7.0),
        new FrequencyBandPreset("7 - 10 MHz", FrequencyBandKind.Preset, 7, 10),
        new FrequencyBandPreset("10 - 14 MHz", FrequencyBandKind.Preset, 10, 14),
        new FrequencyBandPreset("14 - 20 MHz", FrequencyBandKind.Preset, 14, 20),
        new FrequencyBandPreset("20 - 28 MHz", FrequencyBandKind.Preset, 20, 28),
        new FrequencyBandPreset("28 - 35 MHz", FrequencyBandKind.Preset, 28, 35),
        Custom,
    };
}
