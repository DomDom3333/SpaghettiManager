using SpaghettiManager.App.Services;
using SpaghettiManager.Model;
using SpaghettiManager.Model.Records;

namespace SpaghettiManager.App.ViewModels;

public partial class ScanReviewViewModel : ObservableObject, IQueryAttributable
{
    private readonly SpaghettiDatabase database;
    private readonly EanSearchBarcodeService barcodeService;

    [ObservableProperty]
    private string summaryTitle = "Unknown filament";

    [ObservableProperty]
    private string summarySubtitle = "Review and complete details";

    [ObservableProperty]
    private bool saveEanMapping;

    [ObservableProperty]
    private bool hasExistingMapping;

    [ObservableProperty]
    private string mappingStatusTitle = "No catalog match yet";

    [ObservableProperty]
    private string mappingStatusSubtitle = "Save this barcode mapping to speed up future scans.";

    [ObservableProperty]
    private Spool scannedSpool = CreateSampleSpool();

    public ScanReviewViewModel(SpaghettiDatabase database, EanSearchBarcodeService barcodeService)
    {
        this.database = database;
        this.barcodeService = barcodeService;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("barcode", out var barcodeValue))
        {
            _ = LoadFromBarcodeAsync(barcodeValue?.ToString());
            return;
        }

        LoadSample();
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

    private void LoadSample()
    {
        ScannedSpool = CreateSampleSpool();
        SummaryTitle = $"{ScannedSpool.Manufacturer} {ScannedSpool.Material.Name}";
        SummarySubtitle = $"{ScannedSpool.Material.Color} • {ScannedSpool.Material.DiameterMm} mm";
        SaveEanMapping = true;
        HasExistingMapping = false;
        MappingStatusTitle = "No catalog match yet";
        MappingStatusSubtitle = "Save this barcode mapping to speed up future scans.";
    }

    private async Task LoadFromBarcodeAsync(string? barcodeValue)
    {
        if (!long.TryParse(barcodeValue, out var barcode))
        {
            LoadSample();
            return;
        }

        var lookup = await barcodeService.LookupAndMapAsync(barcodeValue!);
        var spool = CreateSampleSpool();
        spool.Barcode = barcode;
        spool.BarcodeType = lookup.BarcodeType;

        if (lookup.Material is not null)
        {
            spool.Material = lookup.Material;
            spool.MaterialId = lookup.Material.Id;
        }

        if (!string.IsNullOrWhiteSpace(lookup.Brand))
        {
            spool.Manufacturer = lookup.Brand;
        }

        ScannedSpool = spool;
        SummaryTitle = !string.IsNullOrWhiteSpace(lookup.ProductName)
            ? lookup.ProductName
            : $"{ScannedSpool.Manufacturer} {ScannedSpool.Material.Name}";
        SummarySubtitle = !string.IsNullOrWhiteSpace(lookup.Category)
            ? lookup.Category
            : $"{ScannedSpool.Material.Color} • {ScannedSpool.Material.DiameterMm} mm";

        if (lookup.AddedMapping)
        {
            SaveEanMapping = false;
            HasExistingMapping = true;
            MappingStatusTitle = "Catalog match saved";
            MappingStatusSubtitle = "Barcode mapping stored for future scans.";
            return;
        }

        if (lookup.Material is not null)
        {
            SaveEanMapping = false;
            HasExistingMapping = true;
            MappingStatusTitle = "Existing catalog match";
            MappingStatusSubtitle = "This barcode already has a saved mapping.";
            return;
        }

        SaveEanMapping = false;
        HasExistingMapping = false;
        MappingStatusTitle = "No catalog match yet";
        MappingStatusSubtitle = lookup.ErrorMessage ?? "No matching catalog item was found.";
    }

    private static Spool CreateSampleSpool()
    {
        return new Spool
        {
            Manufacturer = "Overture",
            Barcode = 1234567890,
            BarcodeType = Enums.BarcodeType.Ean,
            Material = new Material
            {
                Name = "PLA Professional",
                Color = "Fire Engine Red",
                Family = Enums.MaterialFamily.Pla,
                Finish = Enums.Finish.Glossy,
                Opacity = Enums.Opacity.Opaque,
                Manufacturer = "Overture",
                DiameterMm = 1.75m,
                Notes = "Review batch/lot before saving."
            },
            Carrier = new Carrier
            {
                Manufacturer = "Overture",
                SpoolType = Enums.SpoolType.GenericPlastic,
                EmptyWeightGrams = 200,
                SpoolRadius = 100,
                SpoolHubRadius = 30,
                SpoolHeight = 70
            }
        };
    }
}
