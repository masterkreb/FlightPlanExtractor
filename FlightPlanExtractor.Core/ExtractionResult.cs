namespace FlightPlanExtractor.Core;

// Contains the extracted flights and all issues found during extraction.
public sealed record ExtractionResult(
    IReadOnlyList<FlightData> Flights,
    IReadOnlyList<ExtractionIssue> Issues);