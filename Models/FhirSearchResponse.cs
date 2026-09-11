using Hl7.Fhir.Model;

namespace FHIRExplorer.Models;

public sealed class FhirSearchResponse
{
    public Bundle Bundle { get; }

    public string RawJson { get; }

    public string RequestUrl { get; }

    public FhirSearchResponse(
        Bundle bundle,
        string rawJson,
        string requestUrl)
    {
        Bundle = bundle;
        RawJson = rawJson;
        RequestUrl = requestUrl;
    }
}
