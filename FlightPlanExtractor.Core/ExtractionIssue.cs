namespace FlightPlanExtractor.Core;

// Describes a problem found during extraction without stopping the whole process.
public sealed record ExtractionIssue(
    int? PageNumber,
    string Severity,
    string Message);