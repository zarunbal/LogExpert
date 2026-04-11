namespace LogExpert.Core.Classes.DateTimeParser;

//TODO: This should be moved into LogExpert.UI and changed to internal
// Ensures we have constant width (number of characters) date formats
public static class DateFormatPartAdjuster
{
    private static readonly IDictionary<string, string> _dateTimePartReplacements = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["y"] = "yyy",
        ["yyy"] = "yyyy",
        ["m"] = "mm",
        ["d"] = "dd",
        ["h"] = "hh",
        ["s"] = "ss"
    };

    public static string AdjustDateTimeFormatPart (string part)
    {
        ArgumentNullException.ThrowIfNull(part, nameof(part));

        return !_dateTimePartReplacements.TryGetValue(part, out var adjustedPart)
            ? part
            : char.IsUpper(part[0]) ? adjustedPart.ToUpperInvariant() : adjustedPart.ToLowerInvariant();
    }
}