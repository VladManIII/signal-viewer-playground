namespace SignalViewerPlayground.Models;

/// <summary>
/// An entry in the UI language switcher. DisplayName is intentionally a fixed abbreviation
/// (not a localized resource) since it should stay recognizable regardless of the currently
/// selected language.
/// </summary>
public sealed record LanguageOption(string DisplayName, string CultureCode)
{
    public static readonly LanguageOption English = new("EN", "en");
    public static readonly LanguageOption Ukrainian = new("UA", "uk");

    public static readonly IReadOnlyList<LanguageOption> All = new[] { English, Ukrainian };
}
