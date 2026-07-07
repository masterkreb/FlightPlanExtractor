using FlightPlanExtractor.Core;

namespace FlightPlanExtractor.Tests;

public sealed class PdfFlightExtractorTests
{
    [Fact]
    public void ExtractPages_ReturnsMergedFlightData()
    {
        var pages = new List<PdfPageText>
        {
            new(1, "This page is not relevant for extraction."),
            new(2, """
                Operational Flight Plan
                Date: 19MAR24
                Reg.: HBABC
                From: LSZH ZRH
                To: LIMC LIN
                ALTN1: LIML LIN
                FltNr: LX1234
                ATC: SWR123A
                STD: 08:00
                STA: 08:55
                ZFM: 34066
                LIMC: 0:48 1.7
                LIML: 0:20 0.8
                MIN: 1:43 3.6
                To DEST: VEBIT ABCDE RIXUV To ALTN1:
                Gain / Loss: GAIN 0$/TON
                """),
            new(3, """
                Flight Assignment / Flight Crew Briefing
                19.Mar.2024
                LX1234
                SWR123A
                00:559/42Scheduled
                DOW:DOI:EZFW:28822kg034065kg
                CMDABCSteve KrebsCommander
                COPABCJane ExampleCopilot
                """)
        };

        var extractor = new PdfFlightExtractor();

        var result = extractor.ExtractPages(pages);

        var flight = Assert.Single(result.Flights);
        Assert.Empty(result.Issues);
        Assert.Equal(3, result.TotalPageCount);
        Assert.Equal(1, result.OperationalFlightPlanPageCount);
        Assert.Equal(1, result.CrewBriefingPageCount);
        Assert.Equal("LX1234", flight.FlightNumber);
        Assert.Equal("SWR123A", flight.AtcCallSign);
        Assert.Equal("LSZH", flight.OperationalFlightPlan?.RouteFrom);
        Assert.Equal(9, flight.CrewBriefing?.BusinessPassengers);
        Assert.Equal(42, flight.CrewBriefing?.EconomyPassengers);
    }
}
