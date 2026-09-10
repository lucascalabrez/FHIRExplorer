using Hl7.Fhir.Model;

namespace FHIRExplorer.Models;

public sealed class FhirSearchResponse
{
    public Bundle Bundle { get; }

    public string RawJson { get; }

    public FhirSearchResponse(
        Bundle bundle,
        string rawJson)
    {
        Bundle = bundle;
        RawJson = rawJson;
    }
}