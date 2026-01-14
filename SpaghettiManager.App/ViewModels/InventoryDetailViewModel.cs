using SpaghettiManager.Model;
using SpaghettiManager.Model.Records;

namespace SpaghettiManager.App.ViewModels;

public partial class InventoryDetailViewModel : ObservableObject, IQueryAttributable
{
    [ObservableProperty]
    private string itemId = string.Empty;

    [ObservableProperty]
    private Spool spool = CreateSampleSpool();

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("itemId", out var idValue))
        {
            ItemId = idValue?.ToString() ?? string.Empty;
        }

        LoadSample();
    }

    [RelayCommand]
    private Task RespoolAsync()
    {
        return Shell.Current.GoToAsync("///inventory/action?mode=respool");
    }

    [RelayCommand]
    private Task WeighNowAsync()
    {
        return Shell.Current.GoToAsync("///inventory/action?mode=weigh");
    }

    [RelayCommand]
    private Task MarkEmptyAsync()
    {
        return Task.CompletedTask;
    }

    private void LoadSample()
    {
        Spool = CreateSampleSpool();
    }

    private static Spool CreateSampleSpool()
    {
        return new Spool
        {
            Manufacturer = "Prusament",
            Barcode = 1234567890,
            BarcodeType = Enums.BarcodeType.Ean,
            Material = new Material
            {
                Name = "PLA",
                Color = "Galaxy Black",
                Family = Enums.MaterialFamily.Pla,
                Finish = Enums.Finish.Glossy,
                Opacity = Enums.Opacity.Opaque,
                Manufacturer = "Prusament",
                DiameterMm = 1.75m,
                Density_g_cm3 = 1.24m,
                GlassTransitionC = 60,
                Notes = "Keep in dry box after use."
            },
            Carrier = new Carrier
            {
                Manufacturer = "Prusa",
                SpoolType = Enums.SpoolType.GenericPlastic,
                EmptyWeightGrams = 245,
                SpoolRadius = 100,
                SpoolHubRadius = 30,
                SpoolHeight = 70,
                HighTemp = false
            }
        };
    }
}
