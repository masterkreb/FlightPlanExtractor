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
            var matchingCrew = crewBriefings.FirstOrDefault(crew =>
                crew.FlightNumber == ofp.FlightNumber
                && crew.AtcCallSign == ofp.AtcCallSign);

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
            var matchingOfp = operationalFlightPlans.FirstOrDefault(ofp =>
                ofp.FlightNumber == crew.FlightNumber
                && ofp.AtcCallSign == crew.AtcCallSign);

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
}