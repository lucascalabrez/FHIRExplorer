using FHIRExplorer.Models;
using Bundle = Hl7.Fhir.Model.Bundle;
using CapabilityStatement = Hl7.Fhir.Model.CapabilityStatement;
using FhirResource = Hl7.Fhir.Model.Resource;

namespace FHIRExplorer.Services;

public interface IFhirService
{
    Task<CapabilityStatement> GetCapabilityStatementAsync(string baseUrl);

    Task<FhirSearchResponse> SearchAsync(
        string baseUrl,
        string resourceType,
        string searchParameter,
        string searchValue);

    Task<FhirResource> ReadAsync(
        string baseUrl,
        string resourceType,
        string id);
}
