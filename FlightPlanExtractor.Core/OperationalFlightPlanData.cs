namespace FlightPlanExtractor.Core;

// Stores the first extracted fields from an Operational Flight Plan page.
public sealed record OperationalFlightPlanData(
    int PageNumber,
    string? Date,
    string? AircraftRegistration,
    string? RouteFrom,
    string? RouteTo,
    string? FlightNumber,
    string? AtcCallSign);