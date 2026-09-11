using FHIRExplorer.Models;
using Hl7.Fhir.Serialization;
using Bundle = Hl7.Fhir.Model.Bundle;
using CapabilityStatement = Hl7.Fhir.Model.CapabilityStatement;
using FhirResource = Hl7.Fhir.Model.Resource;

namespace FHIRExplorer.Services;

public sealed class FhirService : IFhirService
{
    private readonly HttpClient httpClient;

    public FhirService()
    {
        httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Accept.ParseAdd("application/fhir+json");
    }

    public async Task<CapabilityStatement> GetCapabilityStatementAsync(string baseUrl)
    {
        var requestUrl = $"{NormalizeBaseUrl(baseUrl)}/metadata";
        var content = await GetJsonAsync(requestUrl);

        return FhirJsonDeserializer.DEFAULT
            .Deserialize<CapabilityStatement>(content);
    }

    public async Task<FhirSearchResponse> SearchAsync(
        string baseUrl,
        string resourceType,
        string searchParameter,
        string searchValue)
    {
        var encodedValue = Uri.EscapeDataString(searchValue);

        var requestUrl =
            $"{NormalizeBaseUrl(baseUrl)}/{resourceType}?{searchParameter}={encodedValue}";

        var content = await GetJsonAsync(requestUrl);

        var bundle = FhirJsonDeserializer.DEFAULT
            .Deserialize<Bundle>(content);

        return new FhirSearchResponse(
            bundle,
            content,
            requestUrl);
    }

    public async Task<FhirResource> ReadAsync(
        string baseUrl,
        string resourceType,
        string id)
    {
        var requestUrl =
            $"{NormalizeBaseUrl(baseUrl)}/{resourceType}/{id}";

        var content = await GetJsonAsync(requestUrl);

        return FhirJsonDeserializer.DEFAULT
            .DeserializeResource(content);
    }

    private async Task<string> GetJsonAsync(string requestUrl)
    {
        var response = await httpClient.GetAsync(requestUrl);
        var content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"FHIR server returned HTTP {(int)response.StatusCode} {response.StatusCode}.",
                null,
                response.StatusCode);
        }

        return content;
    }

    private static string NormalizeBaseUrl(string baseUrl)
    {
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            throw new ArgumentException(
                "FHIR base URL cannot be empty.",
                nameof(baseUrl));
        }

        return baseUrl.Trim().TrimEnd('/');
    }
}
