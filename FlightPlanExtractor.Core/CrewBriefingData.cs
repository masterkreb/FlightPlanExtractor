namespace FlightPlanExtractor.Core;

// Stores extracted fields from a Crew Briefing page.
public sealed record CrewBriefingData(
    int PageNumber,
    DateOnly? Date,
    string? FlightNumber,
    string? AtcCallSign,
    int? BusinessPassengers,
    int? EconomyPassengers,
    int? DryOperatingWeight,
    decimal? DryOperatingIndex,
    IReadOnlyList<CrewMember> CrewMembers);
