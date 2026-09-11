# FHIRExplorer — full snapshot

Snapshot date: 2026-09-10.

This archive captures the current learning-project milestone as reconstructed from the live working state discussed in ChatGPT.

## Current features

- .NET MAUI multi-target project for Android, iOS, Mac Catalyst, and Windows.
- Dependency injection throughout the app.
- `IFhirService` abstraction mapped to `FhirService` as a singleton.
- Read a FHIR R4 `CapabilityStatement` from `[base]/metadata`.
- List advertised resource types, interactions, and search parameters.
- Execute a single-parameter FHIR search against HAPI FHIR R4.
- Preserve both representations of a search response:
  - parsed Firely `Bundle`
  - raw server JSON
- Preserve the exact request URL used for the search.
- Select a search result and perform a FHIR `read` interaction.
- Open a `ResourceDetailPage`; Patient resources receive a small semantic summary.
- Open a `SearchsetJsonPage` showing pretty-printed JSON for the complete searchset Bundle.
- Copy the search request URL to the platform clipboard.

## Key dependencies

- .NET 10 MAUI
- `Microsoft.Maui.Controls` via `$(MauiVersion)`
- Firely SDK: `Hl7.Fhir.R4` 6.4.0

## Architecture at this milestone

```text
App
  -> AppShell
       -> MainPage
            -> IFhirService -> FhirService
            -> ResourceDetailPage -> IFhirService
            -> SearchsetJsonPage
```

`FhirService` owns HTTP/FHIR transport and deserialization. UI pages own presentation and navigation.

## Notes

- Android manifest includes `INTERNET` and `ACCESS_NETWORK_STATE`.
- The code avoids the `Task` naming collision between `System.Threading.Tasks.Task` and the FHIR `Task` resource by importing only the specific FHIR model types needed in files that use asynchronous `Task`.
- The project intentionally still uses code-behind. MVVM has not yet been introduced because the current teaching path is keeping abstractions earned and visible.
- No generated build outputs (`bin/`, `obj/`) are included.
