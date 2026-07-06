using UglyToad.PdfPig;

namespace FlightPlanExtractor.Core;

public sealed class PdfTextReader
{
    public IReadOnlyList<PdfPageText> ReadPages(string pdfPath)
    {
        var pages = new List<PdfPageText>();

        using var document = PdfDocument.Open(pdfPath);

        foreach (var page in document.GetPages())
        {
            pages.Add(new PdfPageText(page.Number, page.Text));
        }

        return pages;
    }
}