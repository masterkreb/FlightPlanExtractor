using FlightPlanExtractor.Core;

namespace FlightPlanExtractor.Tests;

public sealed class FlightDataMergerTests
{
    [Fact]
    public void Merge_ShouldCombineOfpAndCrewBriefing_WhenFlightNumberAndAtcMatch()
    {
        var ofp = new OperationalFlightPlanData(
            PageNumber: 5,
            Date: "19MAR24",
            AircraftRegistration: "HBJVY",
            RouteFrom: "LSZH",
            RouteTo: "LIMC",
            AlternateAirdrome1: "LIML",
            AlternateAirdrome2: null,
            FlightNumber: "LX1612",
            AtcCallSign: "SWR612Q",
            DepartureTime: "08:00",
            ArrivalTime: "08:55",
            ZeroFuelMass: 34066,
            TimeToDestination: "0:48",
            FuelToDestination: 1.7m,
            TimeToAlternate: "0:20",
            FuelToAlternate: 0.8m,
            MinimumFuelRequired: 3.6m,
            RouteFirstAndLastNavigationPoint: "VEBIT - RIXUV",
            GainLoss: 0);

        var crew = new CrewBriefingData(
            PageNumber: 97,
            Date: "19.Mar.2024",
            FlightNumber: "LX1612",
            AtcCallSign: "SWR612Q",
            BusinessPassengers: 9,
            EconomyPassengers: 42,
            DryOperatingWeight: 28822,
            DryOperatingIndex: 0,
            CrewMembers: []);

        var merger = new FlightDataMerger();

        var result = merger.Merge([ofp], [crew]);

        Assert.Single(result.Flights);
        Assert.Empty(result.Issues);

        var flight = result.Flights[0];

        Assert.Equal("LX1612", flight.FlightNumber);
        Assert.Equal("SWR612Q", flight.AtcCallSign);
        Assert.Same(ofp, flight.OperationalFlightPlan);
        Assert.Same(crew, flight.CrewBriefing);
    }
}