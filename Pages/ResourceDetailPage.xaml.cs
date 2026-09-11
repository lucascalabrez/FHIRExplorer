using FHIRExplorer.Services;
using Patient = Hl7.Fhir.Model.Patient;

namespace FHIRExplorer.Pages;

public partial class ResourceDetailPage : ContentPage
{
    private readonly IFhirService fhirService;

    public ResourceDetailPage(IFhirService fhirService)
    {
        InitializeComponent();
        this.fhirService = fhirService;
    }

    public async Task LoadResourceAsync(
        string baseUrl,
        string resourceType,
        string id)
    {
        try
        {
            StatusLabel.Text = $"Reading {resourceType}/{id}...";

            var resource =
                await fhirService.ReadAsync(
                    baseUrl,
                    resourceType,
                    id);

            ResourceTitleLabel.Text =
                $"{resource.GetType().Name}/{resource.Id}";

            if (resource is Patient patient)
            {
                DisplayPatient(patient);
            }
            else
            {
                ResourceEditor.Text =
                    $"FHIR resource type: {resource.GetType().Name}\n" +
                    $"ID: {resource.Id}";
            }

            StatusLabel.Text = "Resource loaded.";
        }
        catch (Exception ex)
        {
            StatusLabel.Text = "Read failed.";
            ResourceEditor.Text = ex.Message;
        }
    }

    private void DisplayPatient(Patient patient)
    {
        var name = patient.Name.FirstOrDefault();

        var givenNames = name is null
            ? string.Empty
            : string.Join(" ", name.Given);

        var familyName = name?.Family ?? string.Empty;
        var fullName = $"{givenNames} {familyName}".Trim();

        ResourceEditor.Text =
            $"ID: {patient.Id}\n" +
            $"Name: {(string.IsNullOrWhiteSpace(fullName) ? "unknown" : fullName)}\n" +
            $"Birth date: {patient.BirthDate ?? "unknown"}\n" +
            $"Active: {patient.Active?.ToString() ?? "unknown"}";
    }
}
