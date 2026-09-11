using FHIRExplorer.Models;
using FHIRExplorer.Pages;
using FHIRExplorer.Services;
using CapabilityStatement = Hl7.Fhir.Model.CapabilityStatement;

namespace FHIRExplorer;

public partial class MainPage : ContentPage
{
    private readonly IFhirService fhirService;
    private readonly ResourceDetailPage resourceDetailPage;
    private readonly SearchsetJsonPage searchsetJsonPage;

    private CapabilityStatement? capabilityStatement;
    private string? selectedResourceType;
    private FhirSearchResponse? lastSearchResponse;

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

    private async void OnLoadCapabilityClicked(
        object? sender,
        EventArgs e)
    {
        var baseUrl = ServerUrlEntry.Text?.Trim();

        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            CapabilityStatusLabel.Text = "Enter a FHIR server base URL.";
            return;
        }

        try
        {
            CapabilityStatusLabel.Text = "Loading CapabilityStatement...";

            capabilityStatement =
                await fhirService.GetCapabilityStatementAsync(baseUrl);

            var resourceTypes = capabilityStatement.Rest
                .SelectMany(rest => rest.Resource)
                .Select(resource => resource.Type)
                .Where(type => type is not null)
                .Select(type => type!)
                .Distinct()
                .OrderBy(type => type)
                .ToList();

            ResourceCollectionView.ItemsSource = resourceTypes;

            CapabilityStatusLabel.Text =
                $"Loaded {resourceTypes.Count} resource types.";
        }
        catch (Exception ex)
        {
            CapabilityStatusLabel.Text = $"Load failed: {ex.Message}";
        }
    }

    private void OnResourceSelected(
        object? sender,
        SelectionChangedEventArgs e)
    {
        selectedResourceType =
            e.CurrentSelection.FirstOrDefault() as string;

        ResetSearchState();

        if (string.IsNullOrWhiteSpace(selectedResourceType) ||
            capabilityStatement is null)
        {
            SelectedResourceLabel.Text = "Selected resource: none";
            InteractionsLabel.Text = "—";
            SearchParameterPicker.ItemsSource = null;
            SearchButton.IsEnabled = false;
            return;
        }

        SelectedResourceLabel.Text =
            $"Selected resource: {selectedResourceType}";

        var resourceDefinition = capabilityStatement.Rest
            .SelectMany(rest => rest.Resource)
            .FirstOrDefault(resource =>
                string.Equals(
                    resource.Type,
                    selectedResourceType,
                    StringComparison.Ordinal));

        if (resourceDefinition is null)
        {
            InteractionsLabel.Text = "No capability details found.";
            SearchParameterPicker.ItemsSource = null;
            SearchButton.IsEnabled = false;
            return;
        }

        var interactions = resourceDefinition.Interaction
            .Select(interaction => interaction.Code?.ToString())
            .Where(code => !string.IsNullOrWhiteSpace(code))
            .ToList();

        InteractionsLabel.Text = interactions.Count == 0
            ? "None advertised."
            : string.Join(", ", interactions);

        var searchParameters = resourceDefinition.SearchParam
            .Select(parameter => parameter.Name)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Select(name => name!)
            .Distinct()
            .OrderBy(name => name)
            .ToList();

        SearchParameterPicker.ItemsSource = searchParameters;
        SearchParameterPicker.SelectedIndex = -1;

        SearchButton.IsEnabled = searchParameters.Count > 0;
        SearchStatusLabel.Text = searchParameters.Count > 0
            ? "Choose a search parameter and enter a value."
            : "This resource advertises no search parameters.";
    }

    private async void OnSearchClicked(
        object? sender,
        EventArgs e)
    {
        var baseUrl = ServerUrlEntry.Text?.Trim();
        var selectedParameter =
            SearchParameterPicker.SelectedItem as string;
        var searchValue = SearchValueEntry.Text?.Trim();

        if (string.IsNullOrWhiteSpace(baseUrl) ||
            string.IsNullOrWhiteSpace(selectedResourceType) ||
            string.IsNullOrWhiteSpace(selectedParameter) ||
            string.IsNullOrWhiteSpace(searchValue))
        {
            SearchStatusLabel.Text =
                "Choose a resource, search parameter, and value.";
            return;
        }

        try
        {
            SearchStatusLabel.Text = "Searching...";
            ViewSearchsetJsonButton.IsEnabled = false;

            lastSearchResponse =
                await fhirService.SearchAsync(
                    baseUrl,
                    selectedResourceType,
                    selectedParameter,
                    searchValue);

            var bundle = lastSearchResponse.Bundle;

            var results = bundle.Entry
                .Where(entry => entry.Resource is not null)
                .Select(entry => new SearchResultItem
                {
                    ResourceType = entry.Resource.GetType().Name,
                    Id = entry.Resource.Id ?? string.Empty
                })
                .ToList();

            SearchResultCollectionView.ItemsSource = results;
            SearchStatusLabel.Text =
                $"Search complete. {results.Count} result(s) in this Bundle.";

            ViewSearchsetJsonButton.IsEnabled = true;
        }
        catch (Exception ex)
        {
            lastSearchResponse = null;
            SearchResultCollectionView.ItemsSource = null;
            ViewSearchsetJsonButton.IsEnabled = false;
            SearchStatusLabel.Text = $"Search failed: {ex.Message}";
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
            return;

        await resourceDetailPage.LoadResourceAsync(
            baseUrl,
            selectedResult.ResourceType,
            selectedResult.Id);

        await Shell.Current.Navigation.PushAsync(
            resourceDetailPage);

        SearchResultCollectionView.SelectedItem = null;
    }

    private async void OnViewSearchsetJsonClicked(
        object? sender,
        EventArgs e)
    {
        if (lastSearchResponse is null)
            return;

        searchsetJsonPage.LoadResponse(
            lastSearchResponse);

        await Shell.Current.Navigation.PushAsync(
            searchsetJsonPage);
    }

    private void ResetSearchState()
    {
        lastSearchResponse = null;
        SearchValueEntry.Text = string.Empty;
        SearchResultCollectionView.ItemsSource = null;
        ViewSearchsetJsonButton.IsEnabled = false;
    }

    private sealed class SearchResultItem
    {
        public string ResourceType { get; init; } = string.Empty;

        public string Id { get; init; } = string.Empty;

        public string DisplayText => $"{ResourceType}/{Id}";
    }
}
