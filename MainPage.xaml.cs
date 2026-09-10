using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using FHIRExplorer.Services;
using FhirResource = Hl7.Fhir.Model.Resource;
using FHIRExplorer.Models;
using FHIRExplorer.Pages;
namespace FHIRExplorer;

public partial class MainPage : ContentPage
{
    private CapabilityStatement? capabilityStatement;
    private string? selectedResourceType;

    private FhirSearchResponse? lastSearchResponse;

    private class SearchResultItem
    {
        public string ResourceType { get; set; } = string.Empty;
        public string Id { get; set; } = string.Empty;
        public string DisplayText => $"{ResourceType}/{Id}";
    }

    private readonly IFhirService fhirService;
    private readonly ResourceDetailPage resourceDetailPage;
    private readonly SearchsetJsonPage searchsetJsonPage;

    public MainPage(
        IFhirService fhirService,
        ResourceDetailPage resourceDetailPage,
        SearchsetJsonPage searchsetJsonPage)
    {
        InitializeComponent();

        this.fhirService = fhirService;
        this.resourceDetailPage = resourceDetailPage;
        this.searchsetJsonPage = searchsetJsonPage;
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



            lastSearchResponse =
              await fhirService.SearchAsync(
                  baseUrl,
                  selectedResourceType,
                  selectedParameter,
                  searchValue);

            var bundle =
                lastSearchResponse.Bundle;

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

        var baseUrl =
            ServerUrlEntry.Text?.Trim();

        if (string.IsNullOrWhiteSpace(baseUrl))
            return;

        await resourceDetailPage.LoadResourceAsync(
            baseUrl,
            selectedResult.ResourceType,
            selectedResult.Id);

        await Navigation.PushAsync(
            resourceDetailPage);
    }

    private async void OnViewSearchsetJsonClicked(
    object? sender,
    EventArgs e)
    {
        if (lastSearchResponse is null)
            return;

        searchsetJsonPage.LoadJson(
            lastSearchResponse.RawJson);

        await Shell.Current.Navigation.PushAsync(
            searchsetJsonPage);
    }
}
