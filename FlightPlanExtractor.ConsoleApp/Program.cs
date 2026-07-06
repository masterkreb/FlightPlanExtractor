using FlightPlanExtractor.Core;

if (args.Length == 0)
{
    Console.WriteLine("Please provide a path to a PDF file.");
    Console.WriteLine("Example:");
    Console.WriteLine("  dotnet run --project .\\FlightPlanExtractor.ConsoleApp\\FlightPlanExtractor.ConsoleApp.csproj -- \"C:\\Files\\sample.pdf\"");
    return;
}

var pdfPath = args[0];

var reader = new PdfTextReader();
var pages = reader.ReadPages(pdfPath);

Console.WriteLine($"Read {pages.Count} pages from:");
Console.WriteLine(pdfPath);

Console.WriteLine();

foreach (var page in pages.Take(5))
{
    var preview = page.Text.ReplaceLineEndings(" ");

    if (preview.Length > 250)
    {
        preview = preview[..250] + "...";
    }

    Console.WriteLine($"Page {page.PageNumber}: {preview}");
}