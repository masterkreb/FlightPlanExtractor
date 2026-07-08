using FlightPlanExtractor.Core;

namespace FlightPlanExtractor.Tests;

public sealed class CrewBriefingParserTests
{
    [Fact]
    public void Parse_ShouldExtractCrewBriefingFields()
    {
        var page = new PdfPageText(
            PageNumber: 97,
            Text: """
                Flight Assignment / Flight Crew Briefing
                19.Mar.2024
                LX1612
                SWR612Q
                00:559/42Scheduled
                DOW:DOI:EZFW:28822kg034065kg
                CMDABCSteve KrebsCommander
                COPABCGregory GilliozCopilot
                CABABCLuisa Quadros VissottoCabin Attendant
                SENABCRegine Kathrin Schumacher-HornSenior Cabin Attendant
                """);

        var parser = new CrewBriefingParser();

        var result = parser.Parse(page);

        Assert.Equal(97, result.PageNumber);
        Assert.Equal(new DateOnly(2024, 3, 19), result.Date);
        Assert.Equal("LX1612", result.FlightNumber);
        Assert.Equal("SWR612Q", result.AtcCallSign);
        Assert.Equal(9, result.BusinessPassengers);
        Assert.Equal(42, result.EconomyPassengers);
        Assert.Equal(28822, result.DryOperatingWeight);
        Assert.Equal(0, result.DryOperatingIndex);
        Assert.Collection(
            result.CrewMembers,
            member =>
            {
                Assert.Equal("Steve Krebs", member.Name);
                Assert.Equal("CMD", member.Function);
            },
            member =>
            {
                Assert.Equal("Gregory Gillioz", member.Name);
                Assert.Equal("COP", member.Function);
            },
            member =>
            {
                Assert.Equal("Luisa Quadros Vissotto", member.Name);
                Assert.Equal("CAB", member.Function);
            },
            member =>
            {
                Assert.Equal("Regine Kathrin Schumacher-Horn", member.Name);
                Assert.Equal("SEN", member.Function);
            });
    }
}
