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
PdfTextReader             → reads raw text per page
   │
   ▼
FlightPageClassifier      → determines page type (OFP start page / Crew Briefing start page / irrelevant)
   │
   ▼
OperationalFlightPlanParser
CrewBriefingParser        → extracts fields from recognized pages
   │
   ▼
FlightDataMerger          → merges OFP + Crew Briefing records by flight number and ATC call sign
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
FlightPlanExtractor.slnx
├── FlightPlanExtractor.Core        Class library: models, parsers, extraction logic
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
dotnet run --project FlightPlanExtractor.ConsoleApp -- "path/to/sample.pdf"
```

Example:

```bash
dotnet run --project FlightPlanExtractor.ConsoleApp -- "C:\Files\sample.pdf"
```

### Run the tests
```bash
dotnet test
```

NuGet packages are restored automatically on build; no separate install step is
required (unlike for example `npm install` in Node.js projects, .NET restores packages
referenced in the `.csproj` files automatically).

## Libraries Used

- **PdfPig** – used for reading raw text per page from the PDF. I compared a few
  open-source .NET options for PDF text extraction (for example PdfPig,
  iText, PDFsharp) and chose PdfPig because it is free and open-source (MIT license),
  actively maintained, and focused specifically on reading/extracting content rather
  than PDF creation or editing, which fits this use case well.
- **xUnit** – test framework used for unit testing. It's free, actively maintained, and
  the most common choice in modern .NET projects.

## Error Handling

The extractor returns an `ExtractionResult` object that contains both extracted
flights and extraction issues. This allows the caller to continue working with
partial results instead of stopping the whole process immediately.

At the moment, the merger creates warnings for unmatched records:

- an OFP entry without a matching Crew Briefing entry
- a Crew Briefing entry without a matching OFP entry

The issue contains a severity, a message and the source page if available.

## Testing

The solution contains a separate xUnit test project. The current implementation is
structured so parser and merger logic can be tested with plain text samples without
requiring a real PDF for every test case.

Current tests cover:

- page classification
- merging OFP and Crew Briefing records by flight number and ATC call sign

Additional tests should be added for:

- OFP field extraction
- Crew Briefing field extraction
- unmatched-record issues

## Known Limitations

PDF text extraction does not always follow the visual layout exactly. For example, an
empty `ALTN2` field in the sample file is followed by the word `Delay`, so a parser
that simply takes the next four letters after `ALTN2:` would incorrectly read `Dela`
as an airport code. The current OFP parser therefore expects an airport pattern such
as `LIML LIN` instead of only any four letters.

The current implementation is focused on the provided sample PDF. It demonstrates the
approach and extracts the requested fields from that sample, but additional PDF
variants may require more parsing rules.

Dates are currently kept in the format found in the PDF text, for example `19MAR24`
for OFP pages and `19.Mar.2024` for Crew Briefing pages. A production version should
normalize these values.

Some parser rules are regex-based and depend on labels such as `FltNr`, `ATC`, `DOW`
and `DOI`. If the external PDF generator changes these labels, the parser should
report missing fields and the rules may need to be adjusted.

## Possible Improvements

- Normalize dates into a common `DateOnly` value.
- Add more unit tests for parser edge cases.
- Add structured JSON output.
- Add more detailed issues for missing individual fields.
- Make field definitions more configurable.
- Support additional PDF layout variants.

