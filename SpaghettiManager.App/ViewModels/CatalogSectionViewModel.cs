using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Maui.Controls;

namespace SpaghettiManager.App.ViewModels;

public partial class CatalogSectionViewModel : ObservableObject, IQueryAttributable
{
    [ObservableProperty]
    private string sectionTitle = string.Empty;

    public ObservableCollection<string> Items { get; } = new();

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("key", out var value))
        {
            SectionTitle = value?.ToString() ?? string.Empty;
        }

        LoadSampleItems();
    }

    private void LoadSampleItems()
    {
        Items.Clear();
        switch (SectionTitle)
        {
            case "manufacturers":
                SectionTitle = "Manufacturers";
                Items.Add("Prusament");
                Items.Add("Overture");
                Items.Add("Polymaker");
                break;
            case "spools":
                SectionTitle = "Spools / carriers";
                Items.Add("Prusa plastic spool");
                Items.Add("Cardboard spool - 200 mm");
                Items.Add("Reusable carrier - AMS compatible");
                break;
            case "materials":
                SectionTitle = "Materials";
                Items.Add("PLA");
                Items.Add("PETG");
                Items.Add("TPU");
                break;
            case "additives":
                SectionTitle = "Additives";
                Items.Add("Carbon fiber blend");
                Items.Add("Glow additive");
                break;
            default:
                SectionTitle = "Catalog";
                Items.Add("No data loaded");
                break;
        }
    }
}
