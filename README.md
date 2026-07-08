# FlightPlanExtractor

## Overview

This application reads flight planning PDF files and extracts the fields required by
the task into structured objects.

A PDF file can contain one or more flights. The required data is split between two
sections: the Operational Flight Plan (OFP) and the Crew Briefing.

The result is a list of flight objects. Each flight object combines the extracted OFP
data with the matching Crew Briefing data.

## Task Understanding

The task requires a module that:

- Extracts the required fields from the OFP and Crew Briefing sections of a PDF file.
- Supports PDF files that contain data for multiple flights.
- Ignores unrelated pages and sections in the document.
- Is easy to extend with new fields.
- Handles errors and reports them to the calling code with enough context to understand the
  problem.
- Is structured so it can be extended, integrated into a larger system and tested.
- Is written in the latest version of .NET Core.
- Includes a sample console app to demonstrate how the core logic can be used.

## How I Approached the Task

1. I first read the task description carefully and summarized the expected input and
   output in my own words.
2. I inspected the provided sample PDF and checked that the document contains
   machine-readable text.
3. I implemented the solution in small steps: first reading text from the PDF, then
   finding the relevant OFP and Crew Briefing pages, then extracting the required
   fields and finally merging matching OFP and Crew Briefing records.
4. I compared the OFP and Crew Briefing pages for the same flights and identified
   shared values such as flight number, ATC call sign and date.
5. Based on that, I decided not to rely on page order, but to merge records using
   stable values found in the document.
6. I structured the solution into a core library, a console app and a test project so
   the extraction logic can be reused and tested independently.
7. I added an extraction result object with issues, so the calling code can receive partial
   results together with warnings instead of only getting an error.
8. I ran the program with the provided sample PDF to check that the expected flights
   and fields are extracted.
9. I added unit tests for the classifier, parsers, merger and extraction pipeline.

## Architecture

I modeled each flight as one main object with two separate data parts: one for the OFP data
and one for the Crew Briefing data. This fits the document structure, because both
sections have different page layouts and are parsed by separate parser classes.

```
FlightData
├── OperationalFlightPlan
└── CrewBriefing
```

This keeps the parsing code separated and makes it easier to add another data part
later without changing the existing structure.

Processing pipeline:

```text
PDF file
   │
   ▼
PdfFlightExtractor        → runs the full extraction process
   │
   ▼
PdfTextReader             → reads text from each page
   │
   ▼
FlightPageClassifier      → checks if a page is OFP, Crew Briefing or irrelevant
   │
   ▼
OperationalFlightPlanParser → extracts OFP fields
CrewBriefingParser          → extracts Crew Briefing fields
   │
   ▼
FlightDataMerger          → combines matching OFP and Crew Briefing records
   │
   ▼
ExtractionResult
```

PdfPig extracts raw text from the PDF. The project code then decides which
pages are relevant and extracts the required fields. This also makes testing easier,
because parser tests can use plain strings instead of real PDF files.

### Project Structure

```
FlightPlanExtractor.slnx
├── FlightPlanExtractor.Core        Class library with models, parsers and extraction logic
├── FlightPlanExtractor.ConsoleApp  Console app that demonstrates how to use the core logic
└── FlightPlanExtractor.Tests       Unit tests
```

## Matching Strategy

Each relevant flight has an OFP record and a Crew Briefing record in separate page
blocks. By inspecting the full sample file, I found that the flight number, ATC call
sign and date appear on both records for the same flight, so I use these values as
the merge key.

| Source | Values used for matching |
|---|---|
| Operational Flight Plan | Flight number, ATC call sign, normalized date |
| Crew Briefing | Flight number, ATC call sign, normalized date |

The OFP and Crew Briefing dates have different formats in the extracted text, so the
parsers normalize them before merging.

The code does not assume that the first OFP page belongs to the first Crew
Briefing page. It matches records by values found in the document, not by page order.

## What the Code Expects

- The input PDF contains extractable text and is not only a scanned image.
- Relevant OFP pages contain labels such as `Operational Flight Plan`, `FltNr` and
  `ATC`.
- Relevant Crew Briefing pages contain labels such as `Flight Assignment / Flight Crew
  Briefing`, `DOW` and `DOI`.
- Dates are expected in formats similar to the sample file, for example `19MAR24` and
  `19.Mar.2024`.

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

The `PdfFlightExtractor` returns an `ExtractionResult` object that contains both extracted
flights and extraction issues. This allows the calling code to continue working with
partial results instead of only stopping with an error.

Each extracted OFP and Crew Briefing record keeps its source page number. This makes
it easier to trace extracted data or issues back to a specific page in the PDF.

The merger creates warnings for unmatched records:

- an OFP entry without a matching Crew Briefing entry
- a Crew Briefing entry without a matching OFP entry

Each issue contains a severity, a message and the source page if available.

Example issue:

```text
[Warning] Page 5: No matching crew briefing found for LX1612 / SWR612Q.
```

## Testing

The solution contains a separate xUnit test project. The current version is
structured so parser and merger logic can be tested with plain text samples without
requiring a real PDF for every test case.

Current tests cover:

- page classification
- OFP field extraction
- Crew Briefing field extraction
- merging OFP and Crew Briefing records by flight number, ATC call sign and date
- the extraction pipeline from already-read page text to merged flight data

Additional tests should be added for:

- unmatched-record issues

## Known Limitations

PDF text extraction does not always follow the visual layout exactly. For example, an
empty `ALTN2` field in the sample file is followed by the word `Delay`, so a parser
that simply takes the next four letters after `ALTN2:` would incorrectly read `Dela`
as an airport code. The current OFP parser therefore expects an airport pattern such
as `LIML LIN` instead of only any four letters.

The current version is focused on the provided sample PDF. It demonstrates the
approach and extracts the requested fields from that sample, but additional PDF
variants may require more parsing rules.

Dates are normalized and displayed as `yyyy-MM-dd`. The current date parser supports
the formats found in the sample file; other date formats may require more parser
rules.

Some parser rules are regex-based and depend on labels such as `FltNr`, `ATC`, `DOW`
and `DOI`. If the external PDF generator changes these labels, the parser should
report missing fields and the rules may need to be adjusted.

## Possible Improvements

- Add more unit tests for parser edge cases.
- Add structured JSON output.
- Add more detailed issues for missing individual fields.
- Make field definitions more configurable.
- Support additional PDF layout variants.
- Add OCR handling for scanned PDFs if required.
- Support additional date formats if external PDF files use different formats.
- Introduce interfaces and dependency injection if multiple implementations are needed.

