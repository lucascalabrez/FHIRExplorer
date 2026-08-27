# FHIRExplorer snapshot

This is the clean shared-code snapshot at the current milestone.

Implemented:
- Read a FHIR R4 CapabilityStatement from `[base]/metadata`.
- Deserialize with the Firely .NET SDK.
- List advertised FHIR resource types.
- Inspect resource interactions and search parameters.
- Perform a single-parameter FHIR search and deserialize the returned Bundle.
- Select a search result and perform the FHIR `read` interaction.
- Display a small Patient summary for Patient resources.

## Firely dependency

`Hl7.Fhir.R4` version `6.4.0`.

This snapshot uses the current SDK 6 API:
- `FhirJsonDeserializer.DEFAULT.Deserialize<T>(json)`
- `FhirJsonDeserializer.DEFAULT.DeserializeResource(json)`

## Important

The shared files in this archive are ready to replace the corresponding files in the existing Visual Studio MAUI project.

Keep the template-generated `Platforms` and `Resources` files already present in the existing project. An Android manifest snippet is included because Android needs the INTERNET permission for HAPI access.

The next planned refactoring is to extract HTTP/FHIR work from `MainPage` into a dedicated `FhirService`.
