namespace FlightPlanExtractor.Core;

// Coordinates the complete extraction pipeline for one PDF file.
public sealed class PdfFlightExtractor
{
    private readonly PdfTextReader reader = new();
    private readonly FlightPageClassifier classifier = new();
    private readonly OperationalFlightPlanParser operationalFlightPlanParser = new();
    private readonly CrewBriefingParser crewBriefingParser = new();
    private readonly FlightDataMerger merger = new();

    public ExtractionResult Extract(string pdfPath)
    {
        var pages = reader.ReadPages(pdfPath);

        return ExtractPages(pages);
    }

    public ExtractionResult ExtractPages(IReadOnlyList<PdfPageText> pages)
    {
        var operationalFlightPlanPages = pages
            .Where(page => classifier.Classify(page) == FlightPageType.OperationalFlightPlan)
            .ToList();

        var crewBriefingPages = pages
            .Where(page => classifier.Classify(page) == FlightPageType.CrewBriefing)
            .ToList();

        var operationalFlightPlans = operationalFlightPlanPages
            .Select(page => operationalFlightPlanParser.Parse(page))
            .ToList();

        var crewBriefings = crewBriefingPages
            .Select(page => crewBriefingParser.Parse(page))
            .ToList();

        var result = merger.Merge(operationalFlightPlans, crewBriefings);

        return result with
        {
            TotalPageCount = pages.Count,
            OperationalFlightPlanPageCount = operationalFlightPlanPages.Count,
            CrewBriefingPageCount = crewBriefingPages.Count
        };
    }
}
