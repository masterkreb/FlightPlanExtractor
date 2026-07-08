namespace FlightPlanExtractor.Core;

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
                    Message: $"No matching crew briefing found for {ofp.FlightNumber} / {ofp.AtcCallSign}."));
            }

            flights.Add(new FlightData(
                FlightNumber: ofp.FlightNumber,
                AtcCallSign: ofp.AtcCallSign,
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
                    Message: $"Crew briefing found without matching OFP for {crew.FlightNumber} / {crew.AtcCallSign}."));
            }
        }

        return new ExtractionResult(flights, issues);
    }

    private static bool IsMatch(OperationalFlightPlanData ofp, CrewBriefingData crew)
    {
        return string.Equals(ofp.FlightNumber, crew.FlightNumber, StringComparison.OrdinalIgnoreCase)
            && string.Equals(ofp.AtcCallSign, crew.AtcCallSign, StringComparison.OrdinalIgnoreCase)
            && ofp.Date == crew.Date;
    }
}
