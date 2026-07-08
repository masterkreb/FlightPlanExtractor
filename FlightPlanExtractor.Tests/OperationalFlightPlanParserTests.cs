using FlightPlanExtractor.Core;

namespace FlightPlanExtractor.Tests;

public sealed class OperationalFlightPlanParserTests
{
    [Fact]
    public void Parse_ShouldExtractOperationalFlightPlanFields()
    {
        var page = new PdfPageText(
            PageNumber: 5,
            Text: """
                Operational Flight Plan
                Date: 19MAR24
                Reg.: HBJVY
                From: LSZH ZRH
                To: LIMC LIN
                ALTN1: LIML LIN
                ALTN2: Delay
                FltNr: LX1612
                ATC: SWR612Q
                STD: 08:00
                STA: 08:55
                ZFM: 34066
                LIMC: 0:48 1.7
                LIML: 0:20 0.8
                MIN: 1:43 3.6
                To DEST: VEBIT DCT RIXUV To ALTN1:
                Gain / Loss: GAIN 0$/TON
                """);

        var parser = new OperationalFlightPlanParser();

        var result = parser.Parse(page);

        Assert.Equal(5, result.PageNumber);
        Assert.Equal(new DateOnly(2024, 3, 19), result.Date);
        Assert.Equal("HBJVY", result.AircraftRegistration);
        Assert.Equal("LSZH", result.RouteFrom);
        Assert.Equal("LIMC", result.RouteTo);
        Assert.Equal("LIML", result.AlternateAirdrome1);
        Assert.Null(result.AlternateAirdrome2);
        Assert.Equal("LX1612", result.FlightNumber);
        Assert.Equal("SWR612Q", result.AtcCallSign);
        Assert.Equal("08:00", result.DepartureTime);
        Assert.Equal("08:55", result.ArrivalTime);
        Assert.Equal(34066, result.ZeroFuelMass);
        Assert.Equal("0:48", result.TimeToDestination);
        Assert.Equal(1.7m, result.FuelToDestination);
        Assert.Equal("0:20", result.TimeToAlternate);
        Assert.Equal(0.8m, result.FuelToAlternate);
        Assert.Equal("1:43", result.MinimumFuelTime);
        Assert.Equal(3.6m, result.MinimumFuelRequired);
        Assert.Equal("VEBIT - RIXUV", result.RouteFirstAndLastNavigationPoint);
        Assert.Equal(0, result.GainLoss);
    }
}
