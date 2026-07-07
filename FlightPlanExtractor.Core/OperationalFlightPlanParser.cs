using System.Text.RegularExpressions;

namespace FlightPlanExtractor.Core;

public sealed class OperationalFlightPlanParser
{
    public OperationalFlightPlanData Parse(PdfPageText page)
    {
        var text = page.Text;

        return new OperationalFlightPlanData(
            PageNumber: page.PageNumber,
            Date: FindValueAfterLabel(text, "Date:"),
            AircraftRegistration: FindValueAfterLabel(text, "Reg.:"),
            RouteFrom: FindAirportAfterLabel(text, "From:"),
            RouteTo: FindAirportAfterLabel(text, "To:"),
            FlightNumber: FindValueAfterLabel(text, "FltNr:"),
            AtcCallSign: FindValueAfterLabel(text, "ATC:"));
    }

    private static string? FindValueAfterLabel(string text, string label)
    {
        var pattern = Regex.Escape(label) + @"\s*([A-Z0-9]+)";
        var match = Regex.Match(text, pattern, RegexOptions.IgnoreCase);

        return match.Success ? match.Groups[1].Value : null;
    }

    private static string? FindAirportAfterLabel(string text, string label)
    {
        var pattern = Regex.Escape(label) + @"\s*([A-Z]{4})";
        var match = Regex.Match(text, pattern, RegexOptions.IgnoreCase);

        return match.Success ? match.Groups[1].Value : null;
    }
}