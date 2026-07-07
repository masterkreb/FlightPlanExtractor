namespace FlightPlanExtractor.Core;

public sealed class FlightDataMerger
{
    public IReadOnlyList<FlightData> Merge(
        IReadOnlyList<OperationalFlightPlanData> operationalFlightPlans,
        IReadOnlyList<CrewBriefingData> crewBriefings)
    {
        var flights = new List<FlightData>();

        foreach (var ofp in operationalFlightPlans)
        {
            var matchingCrew = crewBriefings.FirstOrDefault(crew =>
                crew.FlightNumber == ofp.FlightNumber
                && crew.AtcCallSign == ofp.AtcCallSign);

            flights.Add(new FlightData(
                FlightNumber: ofp.FlightNumber,
                AtcCallSign: ofp.AtcCallSign,
                OperationalFlightPlan: ofp,
                CrewBriefing: matchingCrew));
        }

        return flights;
    }
}