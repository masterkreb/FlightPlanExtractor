namespace FlightPlanExtractor.Core;

public sealed class FlightPageClassifier
{
    public FlightPageType Classify(PdfPageText page)
    {
        if (page.Text.Contains("Operational Flight Plan", StringComparison.OrdinalIgnoreCase))
        {
            return FlightPageType.OperationalFlightPlan;
        }

        if (page.Text.Contains("Flight Assignment / Flight Crew Briefing", StringComparison.OrdinalIgnoreCase))
        {
            return FlightPageType.CrewBriefing;
        }

        return FlightPageType.Irrelevant;
    }
}