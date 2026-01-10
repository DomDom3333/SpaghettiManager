using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using SpaghettiManager.App.Services;
using SpaghettiManager.Model;

namespace SpaghettiManager.App.ViewModels;

public partial class ScanReviewViewModel : ObservableObject, IQueryAttributable
{
    [ObservableProperty]
    private string barcode = string.Empty;

    [ObservableProperty]
    private string summaryTitle = "Unknown filament";

    [ObservableProperty]
    private string summarySubtitle = "Review and complete details";

    [ObservableProperty]
    private bool saveEanMapping;

    [ObservableProperty]
    private string manufacturer = string.Empty;

    [ObservableProperty]
    private string productLine = string.Empty;

    [ObservableProperty]
    private string material = string.Empty;

    [ObservableProperty]
    private string colorName = string.Empty;

    [ObservableProperty]
    private string batchLot = string.Empty;

    [ObservableProperty]
    private string carrier = string.Empty;

    [ObservableProperty]
    private string initialWeight = string.Empty;

    public ObservableCollection<string> ManufacturerOptions { get; } = new();
    public ObservableCollection<string> ProductLineOptions { get; } = new();
    public ObservableCollection<string> MaterialOptions { get; } = new();
    public ObservableCollection<string> ColorOptions { get; } = new();
    public ObservableCollection<string> CarrierOptions { get; } = new();

    private readonly InventoryDataService inventoryData;

    public ScanReviewViewModel(InventoryDataService inventoryData)
    {
        this.inventoryData = inventoryData;
        _ = LoadOptionsAsync();
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("barcode", out var barcodeValue))
        {
            Barcode = barcodeValue?.ToString() ?? string.Empty;
        }

        _ = LoadFromBarcodeAsync();
    }

    [RelayCommand]
    private Task AddToInventoryAsync()
    {
        return Shell.Current.GoToAsync("//inventory");
    }

    [RelayCommand]
    private Task EditCatalogAsync()
    {
        return Shell.Current.GoToAsync("//catalog/ean-mappings");
    }

    private async Task LoadFromBarcodeAsync()
    {
        var entry = await inventoryData.GetCatalogEntryAsync(Barcode);

        if (entry is null)
        {
            SummaryTitle = "Unknown filament";
            SummarySubtitle = "Review and complete details";
            SaveEanMapping = true;

            Manufacturer = ManufacturerOptions.FirstOrDefault() ?? string.Empty;
            ProductLine = string.Empty;
            Material = MaterialOptions.FirstOrDefault() ?? string.Empty;
            ColorName = string.Empty;
            BatchLot = string.Empty;
            Carrier = CarrierOptions.FirstOrDefault() ?? string.Empty;
            InitialWeight = string.Empty;
            return;
        }

        SummaryTitle = $"{entry.Manufacturer} {entry.ProductLine}".Trim();
        SummarySubtitle = $"{entry.ColorName} • {FormatDiameter(entry.Diameter)}";
        SaveEanMapping = false;

        Manufacturer = entry.Manufacturer;
        ProductLine = entry.ProductLine;
        Material = entry.MaterialName;
        ColorName = entry.ColorName;
        BatchLot = string.Empty;
        Carrier = entry.CarrierLabel;
        InitialWeight = entry.NominalWeightGrams?.ToString() ?? string.Empty;
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

        if (string.IsNullOrWhiteSpace(Barcode))
        {
            Manufacturer = ManufacturerOptions.FirstOrDefault() ?? string.Empty;
            Material = MaterialOptions.FirstOrDefault() ?? string.Empty;
            Carrier = CarrierOptions.FirstOrDefault() ?? string.Empty;
            return;
        }

        await LoadFromBarcodeAsync();
    }

    private static string FormatDiameter(Enums.FilamentDiameter diameter)
    {
        return diameter switch
        {
            Enums.FilamentDiameter.Mm175 => "1.75 mm",
            Enums.FilamentDiameter.Mm285 => "2.85 mm",
            _ => "Unknown diameter"
        };
    }
}
