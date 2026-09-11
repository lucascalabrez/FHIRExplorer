using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using FHIRExplorer.Services;
using FhirResource = Hl7.Fhir.Model.Resource;

namespace FHIRExplorer;

public partial class MainPage : ContentPage
{
    private CapabilityStatement? capabilityStatement;
    private string? selectedResourceType;

    private readonly FhirService fhirService;
    private class SearchResultItem
    {
        public string ResourceType { get; set; } = string.Empty;
        public string Id { get; set; } = string.Empty;
        public string DisplayText => $"{ResourceType}/{Id}";
    }

    public MainPage(FhirService fhirService)
    {
        InitializeComponent();

        this.fhirService = fhirService;
    }

    private async void OnReadCapabilityClicked(object? sender, EventArgs e)
    {
        try
        {
            StatusLabel.Text = "Contacting FHIR server...";

            var baseUrl = ServerUrlEntry.Text?.Trim();

            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                StatusLabel.Text = "Please enter a FHIR server URL.";
                return;
            }

            capabilityStatement =
               await fhirService
                   .GetCapabilityStatementAsync(baseUrl);

            var resourceTypes = capabilityStatement.Rest
                .SelectMany(rest => rest.Resource)
                .Select(resource => resource.Type?.ToString())
                .OfType<string>()
                .Distinct()
                .OrderBy(type => type)
                .ToList();

            ResourceCollectionView.ItemsSource = resourceTypes;

            StatusLabel.Text =
                
                $"loaded {resourceTypes.Count} supported resource types.";
        }
        catch (Exception ex)
        {
            StatusLabel.Text = $"Request failed: {ex.Message}";
        }
    }

    private void OnResourceSelected(object? sender, SelectionChangedEventArgs e)
    {
        if (capabilityStatement is null)
            return;

        if (e.CurrentSelection.FirstOrDefault() is not string selectedResource)
            return;

        selectedResourceType = selectedResource;
        SelectedResourceLabel.Text = selectedResource;

        var resourceCapability = capabilityStatement.Rest
            .SelectMany(rest => rest.Resource)
            .FirstOrDefault(resource =>
                resource.Type?.ToString() == selectedResource);

        if (resourceCapability is null)
            return;

        var interactions = resourceCapability.Interaction
            .Select(interaction => interaction.Code?.ToString())
            .OfType<string>()
            .ToList();

        InteractionCollectionView.ItemsSource = interactions;

        var searchParameters = resourceCapability.SearchParam
            .Select(parameter => $"{parameter.Name} — {parameter.Type}")
            .OrderBy(parameter => parameter)
            .ToList();

        SearchParameterCollectionView.ItemsSource = searchParameters;

        var searchParameterNames = resourceCapability.SearchParam
            .Select(parameter => parameter.Name)
            .OfType<string>()
            .OrderBy(name => name)
            .ToList();

        SearchParameterPicker.ItemsSource = searchParameterNames;
        SearchParameterPicker.SelectedIndex = -1;

        SearchValueEntry.Text = string.Empty;
        SearchResultCollectionView.ItemsSource = null;
        SearchStatusLabel.Text = "Ready to search.";

        ResourceDetailLabel.Text = "Select a search result to read it.";
        ResourceDetailEditor.Text = string.Empty;
    }

    private async void OnSearchClicked(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(selectedResourceType))
        {
            SearchStatusLabel.Text = "Select a resource first.";
            return;
        }

        if (SearchParameterPicker.SelectedItem is not string selectedParameter)
        {
            SearchStatusLabel.Text = "Choose a search parameter.";
            return;
        }

        var searchValue = SearchValueEntry.Text?.Trim();

        if (string.IsNullOrWhiteSpace(searchValue))
        {
            SearchStatusLabel.Text = "Enter a search value.";
            return;
        }

        var baseUrl = ServerUrlEntry.Text?.Trim();

        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            SearchStatusLabel.Text = "Enter a FHIR server URL.";
            return;
        }

        try
        {
            SearchStatusLabel.Text = "Searching...";



            var bundle =
               await fhirService.SearchAsync(
                   baseUrl,
                   selectedResourceType,
                   selectedParameter,
                   searchValue);

            ViewSearchsetJsonButton.IsEnabled = true;
            var results = bundle.Entry
                .Select(entry => entry.Resource)
                .OfType<FhirResource>()
                .Where(resource => !string.IsNullOrWhiteSpace(resource.Id))
                .Select(resource =>
                    new SearchResultItem
                    {
                        ResourceType = resource.GetType().Name,
                        Id = resource.Id!
                    })
                .ToList();

            SearchResultCollectionView.ItemsSource = results;

            SearchStatusLabel.Text =
                $"Returned {results.Count} entries. " +
                $"Total matches: {bundle.Total?.ToString() ?? "unknown"}.";
        }
        catch (Exception ex)
        {
            SearchStatusLabel.Text = "Search failed.";
            SearchResultCollectionView.ItemsSource =
                new List<string> { ex.Message };
        }
    }

    private async void OnSearchResultSelected(
        object? sender,
        SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault()
            is not SearchResultItem selectedResult)
        {
            return;
        }

        var baseUrl = ServerUrlEntry.Text?.Trim();

        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            ResourceDetailLabel.Text = "FHIR server URL is missing.";
            return;
        }

        try
        {
            ResourceDetailLabel.Text =
                $"Reading {selectedResult.DisplayText}...";

            var resource =
               await fhirService.ReadAsync(
                   baseUrl,
                   selectedResult.ResourceType,
                   selectedResult.Id);

            ResourceDetailLabel.Text =
                $"{resource.GetType().Name}/{resource.Id}";

            if (resource is Patient patient)
            {
                var name = patient.Name.FirstOrDefault();

                var givenNames =
                    name is null
                        ? string.Empty
                        : string.Join(" ", name.Given);

                var familyName = name?.Family ?? string.Empty;
                var fullName = $"{givenNames} {familyName}".Trim();

                ResourceDetailEditor.Text =
                    $"ID: {patient.Id}\n" +
                    $"Name: {(string.IsNullOrWhiteSpace(fullName) ? "unknown" : fullName)}\n" +
                    $"Birth date: {patient.BirthDate ?? "unknown"}\n" +
                    $"Active: {patient.Active?.ToString() ?? "unknown"}";
            }
            else
            {
                ResourceDetailEditor.Text =
                    $"FHIR resource type: {resource.GetType().Name}\n" +
                    $"ID: {resource.Id}";
            }
        }
        catch (Exception ex)
        {
            ResourceDetailLabel.Text = "Read failed.";
            ResourceDetailEditor.Text = ex.Message;
        }
    }
}
