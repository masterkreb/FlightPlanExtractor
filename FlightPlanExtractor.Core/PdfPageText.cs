namespace FlightPlanExtractor.Core;

// Stores the page number and text extracted from one PDF page.
public sealed record PdfPageText(int PageNumber, string Text);
