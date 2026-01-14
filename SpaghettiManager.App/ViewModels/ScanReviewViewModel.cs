using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using SpaghettiManager.Model;
using SpaghettiManager.Model.Records;

namespace SpaghettiManager.App.ViewModels;

public partial class ScanReviewViewModel : ObservableObject, IQueryAttributable
{
    [ObservableProperty]
    private string summaryTitle = "Unknown filament";

    [ObservableProperty]
    private string summarySubtitle = "Review and complete details";

    [ObservableProperty]
    private bool saveEanMapping;

    [ObservableProperty]
    private Spool scannedSpool = CreateSampleSpool();

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
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
