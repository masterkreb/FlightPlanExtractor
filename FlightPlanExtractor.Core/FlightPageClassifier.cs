namespace FlightPlanExtractor.Core;

// Classifies PDF pages as OFP, Crew Briefing or irrelevant.
public sealed class FlightPageClassifier
{
    public FlightPageType Classify(PdfPageText page)
    {
        var text = page.Text;

        // The main OFP data page contains flight labels such as FltNr and ATC.
        if (ContainsText(text, "Operational Flight Plan")
            && ContainsText(text, "FltNr")
            && ContainsText(text, "ATC"))
        {
            return FlightPageType.OperationalFlightPlan;
        }

        // The relevant crew page contains the briefing header and weight/index fields.
        if (ContainsText(text, "Flight Assignment / Flight Crew Briefing")
            && ContainsText(text, "DOW:")
            && ContainsText(text, "DOI:"))
        {
            return FlightPageType.CrewBriefing;
        }

        return FlightPageType.Irrelevant;
    }

    private static bool ContainsText(string text, string value)
    {
        return text.Contains(value, StringComparison.OrdinalIgnoreCase);
    }
}
