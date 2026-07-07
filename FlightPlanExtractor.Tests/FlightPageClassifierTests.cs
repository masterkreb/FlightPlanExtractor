using FlightPlanExtractor.Core;

namespace FlightPlanExtractor.Tests;

public sealed class FlightPageClassifierTests
{
    [Fact]
    public void Classify_ShouldReturnOperationalFlightPlan_WhenTextContainsOfpMarkers()
    {
        var page = new PdfPageText(
            PageNumber: 5,
            Text: "Helvetic Airways - E-Jet Operational Flight Plan FltNr: LX1612 ATC: SWR612Q");

        var classifier = new FlightPageClassifier();

        var result = classifier.Classify(page);

        Assert.Equal(FlightPageType.OperationalFlightPlan, result);
    }

    [Fact]
    public void Classify_ShouldReturnCrewBriefing_WhenTextContainsCrewMarkers()
    {
        var page = new PdfPageText(
            PageNumber: 85,
            Text: "Flight Assignment / Flight Crew Briefing DOW: DOI:");

        var classifier = new FlightPageClassifier();

        var result = classifier.Classify(page);

        Assert.Equal(FlightPageType.CrewBriefing, result);
    }

    [Fact]
    public void Classify_ShouldReturnIrrelevant_WhenTextDoesNotContainKnownMarkers()
    {
        var page = new PdfPageText(
            PageNumber: 1,
            Text: "Some unrelated PDF page");

        var classifier = new FlightPageClassifier();

        var result = classifier.Classify(page);

        Assert.Equal(FlightPageType.Irrelevant, result);
    }
}