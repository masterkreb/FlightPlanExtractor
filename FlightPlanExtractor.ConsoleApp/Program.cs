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

var classifier = new FlightPageClassifier();

// For now the console app shows which pages are relevant for the next parser step.
var operationalFlightPlanPages = pages
    .Where(page => classifier.Classify(page) == FlightPageType.OperationalFlightPlan)
    .ToList();

var crewBriefingPages = pages
    .Where(page => classifier.Classify(page) == FlightPageType.CrewBriefing)
    .ToList();

var ofpParser = new OperationalFlightPlanParser();

var operationalFlightPlans = operationalFlightPlanPages
    .Select(page => ofpParser.Parse(page))
    .ToList();

var crewParser = new CrewBriefingParser();

var crewBriefings = crewBriefingPages
    .Select(page => crewParser.Parse(page))
    .ToList();

var merger = new FlightDataMerger();
var flights = merger.Merge(operationalFlightPlans, crewBriefings);

Console.WriteLine($"Read {pages.Count} pages from:");
Console.WriteLine(pdfPath);

Console.WriteLine();
Console.WriteLine($"Operational Flight Plan pages: {operationalFlightPlanPages.Count}");
Console.WriteLine($"Crew Briefing pages: {crewBriefingPages.Count}");

Console.WriteLine();

Console.WriteLine("Operational Flight Plan page numbers:");
Console.WriteLine(string.Join(", ", operationalFlightPlanPages.Select(page => page.PageNumber)));

Console.WriteLine("Crew Briefing page numbers:");
Console.WriteLine(string.Join(", ", crewBriefingPages.Select(page => page.PageNumber)));

Console.WriteLine();
Console.WriteLine("Operational Flight Plan data:");

foreach (var ofp in operationalFlightPlans)
{
    Console.WriteLine(
        $"Page {ofp.PageNumber}: {ofp.FlightNumber} / {ofp.AtcCallSign} / {ofp.RouteFrom}-{ofp.RouteTo} / {ofp.AircraftRegistration} / {ofp.Date}");
}

Console.WriteLine();
Console.WriteLine("Crew Briefing data:");

foreach (var crew in crewBriefings)
{
    Console.WriteLine(
        $"Page {crew.PageNumber}: {crew.FlightNumber} / {crew.AtcCallSign} / C:{crew.BusinessPassengers} Y:{crew.EconomyPassengers} / DOW:{crew.DryOperatingWeight} / DOI:{crew.DryOperatingIndex} / {crew.Date}");
}

Console.WriteLine();
Console.WriteLine("Merged flight data:");

foreach (var flight in flights)
{
    var ofp = flight.OperationalFlightPlan;
    var crew = flight.CrewBriefing;

    Console.WriteLine(
        $"{flight.FlightNumber} / {flight.AtcCallSign} / {ofp?.RouteFrom}-{ofp?.RouteTo} / C:{crew?.BusinessPassengers} Y:{crew?.EconomyPassengers}");
}

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
