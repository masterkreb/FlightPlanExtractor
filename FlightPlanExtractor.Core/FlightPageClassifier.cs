namespace FlightPlanExtractor.Core;

public sealed class FlightPageClassifier
{
    public FlightPageType Classify(PdfPageText page)
    {
        var text = page.Text;

        if (Contains(text, "Operational Flight Plan")
            && Contains(text, "FltNr")
            && Contains(text, "ATC"))
        {
            return FlightPageType.OperationalFlightPlan;
        }

        if (Contains(text, "Flight Assignment / Flight Crew Briefing")
            && Contains(text, "DOW:")
            && Contains(text, "DOI:"))
        {
            return FlightPageType.CrewBriefing;
        }

        return FlightPageType.Irrelevant;
    }

    private static bool Contains(string text, string value)
    {
        return text.Contains(value, StringComparison.OrdinalIgnoreCase);
    }
}