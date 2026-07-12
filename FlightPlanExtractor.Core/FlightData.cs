namespace FlightPlanExtractor.Core;

// Combines extracted data from the OFP and the matching Crew Briefing.
public sealed record FlightData(
    OperationalFlightPlanData? OperationalFlightPlan,
    CrewBriefingData? CrewBriefing);
