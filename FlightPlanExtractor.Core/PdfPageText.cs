namespace FlightPlanExtractor.Core;

// Stores the text extracted from one PDF page.
public sealed record PdfPageText(int PageNumber, string Text);
