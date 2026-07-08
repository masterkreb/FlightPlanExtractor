using System.Globalization;

namespace FlightPlanExtractor.Core;

internal static class DateParser
{
    private static readonly string[] SupportedFormats =
    [
        "ddMMMyy",
        "dMMMyy",
        "dd.MMM.yyyy",
        "d.MMM.yyyy"
    ];

    public static DateOnly? Parse(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return DateOnly.TryParseExact(
            value.Trim(),
            SupportedFormats,
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out var date)
            ? date
            : null;
    }
}
