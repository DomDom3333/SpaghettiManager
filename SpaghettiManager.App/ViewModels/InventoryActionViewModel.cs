using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using SpaghettiManager.App.Services;

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
    private string manufacturer = string.Empty;

    [ObservableProperty]
    private string productLine = string.Empty;

    [ObservableProperty]
    private string material = string.Empty;

    [ObservableProperty]
    private string colorName = string.Empty;

    [ObservableProperty]
    private string spoolCarrier = string.Empty;

    [ObservableProperty]
    private string totalWeight = string.Empty;

    [ObservableProperty]
    private string emptySpoolWeight = string.Empty;

    [ObservableProperty]
    private string notes = string.Empty;

    [ObservableProperty]
    private bool showCatalogFields = true;

    public ObservableCollection<string> ManufacturerOptions { get; } = new();
    public ObservableCollection<string> ProductLineOptions { get; } = new();
    public ObservableCollection<string> MaterialOptions { get; } = new();
    public ObservableCollection<string> ColorOptions { get; } = new();
    public ObservableCollection<string> CarrierOptions { get; } = new();

    private readonly InventoryDataService inventoryData;

    public InventoryActionViewModel(InventoryDataService inventoryData)
    {
        this.inventoryData = inventoryData;
        _ = LoadOptionsAsync();
    }

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
        (Title, Description, ShowCatalogFields) = Mode switch
        {
            "weigh" => ("Weigh filament", "Enter total weight to update remaining grams.", false),
            "respool" => ("Respool filament", "Move filament onto a new carrier and keep the same lot.", false),
            _ => ("Add filament manually", "Create a new inventory item with minimal required data.", true)
        };

        if (ShowCatalogFields)
        {
            Manufacturer = ManufacturerOptions.FirstOrDefault() ?? string.Empty;
            Material = MaterialOptions.FirstOrDefault() ?? string.Empty;
            ProductLine = ProductLineOptions.FirstOrDefault() ?? string.Empty;
            ColorName = ColorOptions.FirstOrDefault() ?? string.Empty;
        }

        SpoolCarrier = CarrierOptions.FirstOrDefault() ?? string.Empty;
    }

    private async Task LoadOptionsAsync()
    {
        var options = await inventoryData.GetOptionsAsync();
        ManufacturerOptions.Clear();
        foreach (var option in options.Manufacturers)
        {
            ManufacturerOptions.Add(option);
        }

        ProductLineOptions.Clear();
        foreach (var option in options.ProductLines)
        {
            ProductLineOptions.Add(option);
        }

        MaterialOptions.Clear();
        foreach (var option in options.Materials)
        {
            MaterialOptions.Add(option);
        }

        ColorOptions.Clear();
        foreach (var option in options.Colors)
        {
            ColorOptions.Add(option);
        }

        CarrierOptions.Clear();
        foreach (var option in options.Carriers)
        {
            CarrierOptions.Add(option);
        }

        Configure();
    }
}
