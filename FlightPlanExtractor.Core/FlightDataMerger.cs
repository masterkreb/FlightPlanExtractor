using System.Globalization;

namespace FlightPlanExtractor.Core;

// Matches OFP and Crew Briefing records by flight number, ATC call sign and date.
public sealed class FlightDataMerger
{
    public ExtractionResult Merge(
        IReadOnlyList<OperationalFlightPlanData> operationalFlightPlans,
        IReadOnlyList<CrewBriefingData> crewBriefings)
    {
        var flights = new List<FlightData>();
        var issues = new List<ExtractionIssue>();

        foreach (var ofp in operationalFlightPlans)
        {
            var matchingCrew = crewBriefings.FirstOrDefault(crew => IsMatch(ofp, crew));

            if (matchingCrew is null)
            {
                issues.Add(new ExtractionIssue(
                    PageNumber: ofp.PageNumber,
                    Severity: "Warning",
                    Message: $"No matching crew briefing found for {ofp.FlightNumber} / {ofp.AtcCallSign} on {FormatDate(ofp.Date)}."));
            }

            flights.Add(new FlightData(
                OperationalFlightPlan: ofp,
                CrewBriefing: matchingCrew));
        }

        foreach (var crew in crewBriefings)
        {
            var matchingOfp = operationalFlightPlans.FirstOrDefault(ofp => IsMatch(ofp, crew));

            if (matchingOfp is null)
            {
                issues.Add(new ExtractionIssue(
                    PageNumber: crew.PageNumber,
                    Severity: "Warning",
                    Message: $"Crew briefing found without matching OFP for {crew.FlightNumber} / {crew.AtcCallSign} on {FormatDate(crew.Date)}."));
            }
        }

        return new ExtractionResult(flights, issues);
    }

    private static bool IsMatch(OperationalFlightPlanData ofp, CrewBriefingData crew)
    {
        // The date avoids mixing flights with the same flight number on different days.
        return string.Equals(ofp.FlightNumber, crew.FlightNumber, StringComparison.OrdinalIgnoreCase)
            && string.Equals(ofp.AtcCallSign, crew.AtcCallSign, StringComparison.OrdinalIgnoreCase)
            && ofp.Date == crew.Date;
    }

    private static string FormatDate(DateOnly? date)
    {
        return date?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
            ?? "unknown date";
    }
}
