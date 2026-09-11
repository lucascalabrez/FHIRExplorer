using System.Text.Json;
using FHIRExplorer.Models;
using Microsoft.Maui.ApplicationModel.DataTransfer;

namespace FHIRExplorer.Pages;

public partial class SearchsetJsonPage : ContentPage
{
    public SearchsetJsonPage()
    {
        InitializeComponent();
    }

    public void LoadResponse(FhirSearchResponse response)
    {
        RequestUrlEntry.Text = response.RequestUrl;

        try
        {
            using var document = JsonDocument.Parse(response.RawJson);

            JsonEditor.Text = JsonSerializer.Serialize(
                document.RootElement,
                new JsonSerializerOptions
                {
                    WriteIndented = true
                });
        }
        catch
        {
            JsonEditor.Text = response.RawJson;
        }
    }

    private async void OnCopyRequestClicked(
        object? sender,
        EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(RequestUrlEntry.Text))
            return;

        await Clipboard.Default.SetTextAsync(
            RequestUrlEntry.Text);
    }
}
