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
var result = merger.Merge(operationalFlightPlans, crewBriefings);
var flights = result.Flights;

Console.WriteLine($"Read {pages.Count} pages from:");
Console.WriteLine(pdfPath);

Console.WriteLine();
Console.WriteLine($"Operational Flight Plan pages: {operationalFlightPlanPages.Count}");
Console.WriteLine($"Crew Briefing pages: {crewBriefingPages.Count}");

Console.WriteLine();
Console.WriteLine("Extracted flights:");

foreach (var flight in flights)
{
    var ofp = flight.OperationalFlightPlan;
    var crew = flight.CrewBriefing;

    Console.WriteLine();
    Console.WriteLine($"Flight {flight.FlightNumber} / {flight.AtcCallSign}");
    Console.WriteLine("Operational Flight Plan:");
    Console.WriteLine($"  Date: {ofp?.Date}");
    Console.WriteLine($"  Aircraft registration: {ofp?.AircraftRegistration}");
    Console.WriteLine($"  Route: from {ofp?.RouteFrom} to {ofp?.RouteTo}");
    Console.WriteLine($"  Alternate airdrome 1: {ofp?.AlternateAirdrome1}");
    Console.WriteLine($"  Alternate airdrome 2: {ofp?.AlternateAirdrome2 ?? "none"}");
    Console.WriteLine($"  Departure time: {ofp?.DepartureTime}");
    Console.WriteLine($"  Arrival time: {ofp?.ArrivalTime}");
    Console.WriteLine($"  Zero fuel mass: {ofp?.ZeroFuelMass}");
    Console.WriteLine($"  Time to destination: {ofp?.TimeToDestination}");
    Console.WriteLine($"  Fuel to destination: {ofp?.FuelToDestination}");
    Console.WriteLine($"  Time to alternate: {ofp?.TimeToAlternate}");
    Console.WriteLine($"  Fuel to alternate: {ofp?.FuelToAlternate}");
    Console.WriteLine($"  Minimum fuel required: {ofp?.MinimumFuelRequired}");
    Console.WriteLine($"  Route first and last navigation point: {ofp?.RouteFirstAndLastNavigationPoint}");
    Console.WriteLine($"  Gain/loss: {ofp?.GainLoss}");
    Console.WriteLine($"  Source page: {ofp?.PageNumber}");
    Console.WriteLine("Crew Briefing:");
    Console.WriteLine($"  Business passengers: {crew?.BusinessPassengers}");
    Console.WriteLine($"  Economy passengers: {crew?.EconomyPassengers}");
    Console.WriteLine($"  Dry operating weight: {crew?.DryOperatingWeight}");
    Console.WriteLine($"  Dry operating index: {crew?.DryOperatingIndex}");
    Console.WriteLine("  Crew members:");

    foreach (var member in crew?.CrewMembers ?? [])
    {
        Console.WriteLine($"    - {member.Name}, {member.Function}");
    }

    Console.WriteLine($"  Source page: {crew?.PageNumber}");
}

if (result.Issues.Count > 0)
{
    Console.WriteLine();
    Console.WriteLine("Issues:");

    foreach (var issue in result.Issues)
    {
        Console.WriteLine($"  [{issue.Severity}] Page {issue.PageNumber}: {issue.Message}");
    }
}
