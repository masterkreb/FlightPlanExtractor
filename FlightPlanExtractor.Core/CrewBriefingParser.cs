using System.Globalization;
using System.Text.RegularExpressions;

namespace FlightPlanExtractor.Core;

public sealed class CrewBriefingParser
{
    public CrewBriefingData Parse(PdfPageText page)
    {
        var text = page.Text;

        return new CrewBriefingData(
            PageNumber: page.PageNumber,
            Date: FindDate(text),
            FlightNumber: FindFlightNumber(text),
            AtcCallSign: FindAtcCallSign(text),
            BusinessPassengers: FindPassengerCount(text, 1),
            EconomyPassengers: FindPassengerCount(text, 2),
            DryOperatingWeight: FindIntegerAfterLabel(text, "DOW:"),
            DryOperatingIndex: FindDecimalAfterLabel(text, "DOI:"),
            CrewMembers: FindCrewMembers(text));
    }

    private static string? FindDate(string text)
    {
        var match = Regex.Match(text, @"\d{1,2}\.Mar\.20\d{2}", RegexOptions.IgnoreCase);
        return match.Success ? match.Value : null;
    }

    private static string? FindFlightNumber(string text)
    {
        var match = Regex.Match(text, @"\bLX\d{3,4}\b", RegexOptions.IgnoreCase);
        return match.Success ? match.Value : null;
    }

    private static string? FindAtcCallSign(string text)
    {
        var match = Regex.Match(text, @"\bSWR[A-Z0-9]+\b", RegexOptions.IgnoreCase);
        return match.Success ? match.Value : null;
    }

    private static int? FindPassengerCount(string text, int groupNumber)
    {
        // In PdfPig text the passenger count is attached directly after the flight time,
        // for example: 00:559/42Scheduled.
        var match = Regex.Match(text, @"\d{2}:\d{2}(\d+)\/(\d+)Scheduled", RegexOptions.IgnoreCase);

        if (!match.Success)
        {
            return null;
        }

        return int.Parse(match.Groups[groupNumber].Value, CultureInfo.InvariantCulture);
    }

    private static int? FindIntegerAfterLabel(string text, string label)
    {
        if (label.Equals("DOW:", StringComparison.OrdinalIgnoreCase))
        {
            return FindDow(text);
        }

        var pattern = Regex.Escape(label) + @"\s*(\d+)";
        var match = Regex.Match(text, pattern, RegexOptions.IgnoreCase);

        return match.Success
            ? int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture)
            : null;
    }

    private static decimal? FindDecimalAfterLabel(string text, string label)
    {
        if (label.Equals("DOI:", StringComparison.OrdinalIgnoreCase))
        {
            return FindDoi(text);
        }

        var pattern = Regex.Escape(label) + @"\s*(\d+(?:\.\d+)?)";
        var match = Regex.Match(text, pattern, RegexOptions.IgnoreCase);

        return match.Success
            ? decimal.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture)
            : null;
    }

    private static int? FindDow(string text)
    {
        var match = Regex.Match(text, @"DOW:DOI:EZFW:(\d+)kg", RegexOptions.IgnoreCase);

        return match.Success
            ? int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture)
            : null;
    }

    private static decimal? FindDoi(string text)
    {
        var match = Regex.Match(text, @"DOW:DOI:EZFW:\d+kg([0-9.]+?)(\d{5})kg", RegexOptions.IgnoreCase);

        if (!match.Success)
        {
            return null;
        }

        var valueBlock = match.Groups[1].Value;

        if (valueBlock.Contains('.'))
        {
            return decimal.Parse(valueBlock, CultureInfo.InvariantCulture);
        }

        // In the sample file a DOI of 0 is joined with the next value, for example 28822kg034065kg.
        return decimal.Parse(valueBlock[..1], CultureInfo.InvariantCulture);
    }

    private static IReadOnlyList<CrewMember> FindCrewMembers(string text)
    {
        var crewMembers = new List<CrewMember>();
        var pattern = @"(CMD|COP|CAB|SEN)[A-Z]{3}(.+?)(Commander|Copilot|Cabin Attendant|Senior Cabin Attendant)";
        var matches = Regex.Matches(text, pattern, RegexOptions.IgnoreCase);

        foreach (Match match in matches)
        {
            crewMembers.Add(new CrewMember(
                Name: match.Groups[2].Value.Trim(),
                Function: match.Groups[1].Value.ToUpperInvariant()));
        }

        return crewMembers;
    }
}
