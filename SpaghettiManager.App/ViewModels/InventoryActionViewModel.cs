using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;

namespace SpaghettiManager.App.ViewModels;

public partial class InventoryActionViewModel : ObservableObject, IQueryAttributable
{
    [ObservableProperty]
    private string mode = "add";

    [ObservableProperty]
    private string title = "Add Filament";

    [ObservableProperty]
    private string description = "Create a new inventory item with minimal required data.";

    [ObservableProperty]
    private string spoolCarrier = string.Empty;

    [ObservableProperty]
    private string totalWeight = string.Empty;

    [ObservableProperty]
    private string emptySpoolWeight = string.Empty;

    [ObservableProperty]
    private string notes = string.Empty;

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("mode", out var modeValue))
        {
            Mode = modeValue?.ToString() ?? "add";
        }

        Configure();
    }

    [RelayCommand]
    private Task SaveAsync()
    {
        return Shell.Current.GoToAsync("..");
    }

    private void Configure()
    {
        (Title, Description) = Mode switch
        {
            "weigh" => ("Weigh filament", "Enter total weight to update remaining grams."),
            "respool" => ("Respool filament", "Move filament onto a new carrier and keep the same lot."),
            _ => ("Add filament manually", "Create a new inventory item with minimal required data.")
        };
    }
}
