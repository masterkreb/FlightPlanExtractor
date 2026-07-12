using System.Globalization;
using System.Text.RegularExpressions;

namespace FlightPlanExtractor.Core;

// Extracts required OFP fields from the text of one Operational Flight Plan page.
public sealed class OperationalFlightPlanParser
{
    public OperationalFlightPlanData Parse(PdfPageText page)
    {
        var text = page.Text;
        var destinationAirport = FindAirportAfterLabel(text, "To:");
        var alternateAirport1 = FindAirportAfterLabel(text, "ALTN1:");

        return new OperationalFlightPlanData(
            PageNumber: page.PageNumber,
            Date: FindDateAfterLabel(text, "Date:"),
            AircraftRegistration: FindValueAfterLabel(text, "Reg.:"),
            RouteFrom: FindAirportAfterLabel(text, "From:"),
            RouteTo: destinationAirport,
            AlternateAirdrome1: alternateAirport1,
            AlternateAirdrome2: FindAirportAfterLabel(text, "ALTN2:"),
            FlightNumber: FindValueAfterLabel(text, "FltNr:"),
            AtcCallSign: FindValueAfterLabel(text, "ATC:"),
            DepartureTime: FindTimeAfterLabel(text, "STD:"),
            ArrivalTime: FindTimeAfterLabel(text, "STA:"),
            ZeroFuelMass: FindIntegerAfterLabel(text, "ZFM:"),
            TimeToDestination: FindFuelTimeForAirport(text, destinationAirport),
            FuelToDestination: FindFuelAmountForAirport(text, destinationAirport),
            TimeToAlternate: FindFuelTimeForAirport(text, alternateAirport1),
            FuelToAlternate: FindFuelAmountForAirport(text, alternateAirport1),
            MinimumFuelRequired: FindMinimumFuelRequired(text),
            RouteFirstAndLastNavigationPoint: FindRouteFirstAndLastNavigationPoint(text),
            GainLoss: FindGainLoss(text));
    }

    private static string? FindValueAfterLabel(string text, string label)
    {
        var pattern = Regex.Escape(label) + @"\s*([A-Z0-9]+)";
        var match = Regex.Match(text, pattern, RegexOptions.IgnoreCase);

        return match.Success ? match.Groups[1].Value : null;
    }

    private static DateOnly? FindDateAfterLabel(string text, string label)
    {
        return DateParser.Parse(FindValueAfterLabel(text, label));
    }

    private static string? FindAirportAfterLabel(string text, string label)
    {
        // Airport values include the ICAO and IATA code, for example "LIML LIN".
        // This avoids reading "Dela" from an empty ALTN2 field followed by "Delay".
        var pattern = Regex.Escape(label) + @"\s*([A-Z]{4})\s+[A-Z]{3}\b";
        var match = Regex.Match(text, pattern, RegexOptions.IgnoreCase);

        return match.Success ? match.Groups[1].Value.ToUpperInvariant() : null;
    }

    private static string? FindTimeAfterLabel(string text, string label)
    {
        var pattern = Regex.Escape(label) + @"\s*(\d{1,2}:\d{2})";
        var match = Regex.Match(text, pattern, RegexOptions.IgnoreCase);

        return match.Success ? match.Groups[1].Value : null;
    }

    private static int? FindIntegerAfterLabel(string text, string label)
    {
        var pattern = Regex.Escape(label) + @"\s*(\d+)";
        var match = Regex.Match(text, pattern, RegexOptions.IgnoreCase);

        return match.Success ? int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture) : null;
    }

    private static Match? FindFuelLineForAirport(string text, string? airportCode)
    {
        if (string.IsNullOrWhiteSpace(airportCode))
        {
            return null;
        }

        // Fuel lines look like "LIMC: 0:48 1.7" in the extracted text.
        var pattern = Regex.Escape(airportCode) + @":\s*(\d{1,2}:\d{2})\s+(\d+(?:\.\d+)?)";
        var match = Regex.Match(text, pattern, RegexOptions.IgnoreCase);

        return match.Success ? match : null;
    }

    private static string? FindFuelTimeForAirport(string text, string? airportCode)
    {
        var match = FindFuelLineForAirport(text, airportCode);

        return match?.Groups[1].Value;
    }

    private static decimal? FindFuelAmountForAirport(string text, string? airportCode)
    {
        var match = FindFuelLineForAirport(text, airportCode);

        return match is null
            ? null
            : decimal.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture);
    }

    private static decimal? FindMinimumFuelRequired(string text)
    {
        var match = Regex.Match(text, @"MIN:\s*\d{1,2}:\d{2}\s+(\d+(?:\.\d+)?)", RegexOptions.IgnoreCase);

        return match.Success
            ? decimal.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture)
            : null;
    }

    private static string? FindRouteFirstAndLastNavigationPoint(string text)
    {
        var match = Regex.Match(text, @"To DEST:\s*(.+?)\s+To ALTN1:", RegexOptions.IgnoreCase);

        if (!match.Success)
        {
            return null;
        }

        var route = match.Groups[1].Value;
        var navigationPoints = Regex.Matches(route, @"\b[A-Z]{4,6}(?:/[A-Z0-9]+|\d[A-Z]?)?\b")
            .Select(match => Regex.Match(match.Value, @"^[A-Z]{4,6}").Value)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .ToList();

        if (navigationPoints.Count == 0)
        {
            return null;
        }

        return $"{navigationPoints.First()} - {navigationPoints.Last()}";
    }

    private static decimal? FindGainLoss(string text)
    {
        var match = Regex.Match(text, @"Gain\s*/\s*Loss:\s*(GAIN|LOSS)\s*(\d+(?:\.\d+)?)\$/TON", RegexOptions.IgnoreCase);

        if (!match.Success)
        {
            return null;
        }

        var value = decimal.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture);

        return match.Groups[1].Value.Equals("LOSS", StringComparison.OrdinalIgnoreCase)
            ? -value
            : value;
    }
}
