# FlightPlanExtractor

## Overview

This application reads PDF flight planning documents and extracts predefined text
fields into structured objects. Each PDF can contain data for one or more flights,
spread across two chapters: the Operational Flight Plan (OFP) and the Crew Briefing.
The result is a list of flight objects, each combining the relevant data from both
chapters.

## Task Understanding

The task requires a module that:
- Extracts a predefined set of fields from two chapters (OFP and Crew Briefing) in a
  PDF that may contain data for multiple flights.
- Ignores all other, unrelated pages/chapters in the document.
- Is easy to extend with new fields.
- Handles errors appropriately and reports them to the caller with enough context to
  diagnose the issue (PDFs come from external systems and can be malformed, changed
  without notice, etc.).
- Is structured so it can be extended, integrated into a larger system, and adequately
  tested.
- Is written in the latest version of .NET Core.
- Comes with a sample console app to demonstrate usage, while the core logic is built
  for integration into a larger project.

## How I Approached the Task

1. I first read the task description carefully and summarized the expected input and
   output in my own words.
2. I inspected the provided sample PDF to find where the required OFP and Crew Briefing
   fields appear.
3. I compared the OFP and Crew Briefing pages for the same flights and identified
   shared values such as flight number, ATC call sign and date.
4. Based on that, I decided not to rely only on page order, but to merge records using
   a flight key.
5. I structured the solution into a core library, a console app and a test project so
   the extraction logic can be reused and tested independently.
6. I planned the implementation in small steps: read PDF text, classify relevant pages,
   extract fields, merge flight records and collect issues for missing or unexpected
   data.

## Architecture

I modeled each flight as a composite object rather than a single flat object, since OFP
and Crew Briefing data come from different sources (different page layouts, extracted
by different logic):

```
FlightData
├── OperationalFlightPlan   (all OFP fields)
└── CrewBriefing            (all Crew Briefing fields)
```

This keeps parsing responsibilities separated and makes it straightforward to add a
third chapter/source later without changing the existing structure.

Processing pipeline:

```
PDF file
   │
   ▼
IPdfTextReader   → reads raw text per page
   │
   ▼
IPageClassifier  → determines page type (OFP start page / Crew Briefing start page / irrelevant)
   │
   ▼
IFieldExtractor  → extracts fields from a recognized page
   │
   ▼
IFlightMerger    → merges OFP + Crew Briefing records by flight number
   │
   ▼
List<FlightData>
```

The PDF library is only responsible for reading raw text per page. Recognizing and
extracting the specific fields (for example flight number, departure time) is implemented as
separate parsing logic on top of that raw text, which keeps the two concerns
independently testable (field-extraction logic can be unit tested with plain strings,
without needing a real PDF file).

### Project Structure

```
FlightPlanExtractor.sln
├── FlightPlanExtractor.Core        Class library: interfaces, models, extraction logic
├── FlightPlanExtractor.ConsoleApp  Sample console app demonstrating usage
└── FlightPlanExtractor.Tests       Unit tests
```

## Matching Strategy

Each flight appears once in the OFP chapter and once in the Crew Briefing chapter, as
separate page blocks in the document. By inspecting the full sample file, I found that
the flight number (and the ATC callsign) appears identically on both the OFP page and
the corresponding Crew Briefing page for the same flight, so I use it as the key to
match and merge the two records into one `FlightData` object.

The implementation does not assume that the first OFP page belongs to the first Crew
Briefing page. It matches records by values found in the document, not by page order.

## How to Run

### Prerequisites
- .NET 10 SDK

### Run the console app
```bash
dotnet run --project FlightPlanExtractor.ConsoleApp
```

### Run the tests
```bash
dotnet test
```

NuGet packages are restored automatically on build; no separate install step is
required (unlike e.g. `npm install` in Node.js projects, .NET restores packages
referenced in the `.csproj` files automatically).

## Libraries Used

- **PdfPig** – used for reading raw text (and layout/position data) per page from the
  PDF. I compared a few open-source .NET options for PDF text extraction (for example PdfPig,
  iText, PDFsharp) and chose PdfPig because it is free and open-source (MIT license),
  actively maintained, and focused specifically on reading/extracting content rather
  than PDF creation or editing, which fits this use case well.
- **Microsoft.Extensions.DependencyInjection** – used for IoC/dependency injection. The
  architecture relies on interfaces (`IPdfTextReader`, `IPageClassifier`,
  `IFieldExtractor`, `IFlightMerger`), so a DI container is needed to wire concrete
  implementations together at runtime. I chose Microsoft's own package since it's free,
  officially supported, and the de-facto standard in the .NET ecosystem, avoiding an
  extra third-party dependency for something the framework already provides.
- **xUnit** – test framework used for unit testing. It's free, actively maintained, and
  the most common choice in modern .NET projects.
- **Moq** – mocking library used together with xUnit to fake dependencies (for example
  `IPdfTextReader`) in unit tests, so extraction logic can be tested with plain strings
  instead of real PDF files.

## Error Handling

## Testing

## Known Limitations

PDF text extraction does not always follow the visual layout exactly. For example, an
empty `ALTN2` field in the sample file is followed by the word `Delay`, so a parser
that simply takes the next four letters after `ALTN2:` would incorrectly read `Dela`
as an airport code. The current OFP parser therefore expects an airport pattern such
as `LIML LIN` instead of only any four letters.

## Possible Improvements

