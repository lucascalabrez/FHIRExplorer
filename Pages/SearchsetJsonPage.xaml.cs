using System.Text.Json;

namespace FHIRExplorer.Pages;

public partial class SearchsetJsonPage : ContentPage
{
    public SearchsetJsonPage()
    {
        InitializeComponent();
    }

    public void LoadJson(string rawJson)
    {
        try
        {
            using var document =
                JsonDocument.Parse(rawJson);

            JsonEditor.Text =
                JsonSerializer.Serialize(
                    document.RootElement,
                    new JsonSerializerOptions
                    {
                        WriteIndented = true
                    });
        }
        catch
        {
            JsonEditor.Text = rawJson;
        }
    }
}