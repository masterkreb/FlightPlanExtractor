using UglyToad.PdfPig;

namespace FlightPlanExtractor.Core;

// Reads text from each PDF page using PdfPig.
public sealed class PdfTextReader
{
    public IReadOnlyList<PdfPageText> ReadPages(string pdfPath)
    {
        var pages = new List<PdfPageText>();

        // PdfPig opens the PDF and gives access to every page.
        using var document = PdfDocument.Open(pdfPath);

        foreach (var page in document.GetPages())
        {
            pages.Add(new PdfPageText(page.Number, page.Text));
        }

        return pages;
    }
}
