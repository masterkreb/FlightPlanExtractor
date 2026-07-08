namespace FlightPlanExtractor.Core;

// Stores extracted fields from an Operational Flight Plan page.
public sealed record OperationalFlightPlanData(
    int PageNumber,
    DateOnly? Date,
    string? AircraftRegistration,
    string? RouteFrom,
    string? RouteTo,
    string? AlternateAirdrome1,
    string? AlternateAirdrome2,
    string? FlightNumber,
    string? AtcCallSign,
    string? DepartureTime,
    string? ArrivalTime,
    int? ZeroFuelMass,
    string? TimeToDestination,
    decimal? FuelToDestination,
    string? TimeToAlternate,
    decimal? FuelToAlternate,
    string? MinimumFuelTime,
    decimal? MinimumFuelRequired,
    string? RouteFirstAndLastNavigationPoint,
    decimal? GainLoss);
