using SpaghettiManager.Model;
using SpaghettiManager.Model.Records;

namespace SpaghettiManager.App.ViewModels;

public partial class CatalogEanMappingsViewModel : ObservableObject
{
    public ObservableCollection<Spool> Mappings { get; } = new();

    public CatalogEanMappingsViewModel()
    {
        Mappings.Add(new Spool
        {
            Manufacturer = "Overture",
            Barcode = 1234567890,
            BarcodeType = Enums.BarcodeType.Ean,
            Material = new Material
            {
                Name = "PLA Professional",
                Family = Enums.MaterialFamily.Pla,
                Manufacturer = "Overture",
                DiameterMm = 1.75m,
                Finish = Enums.Finish.Glossy,
                Opacity = Enums.Opacity.Opaque
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
        });
        Mappings.Add(new Spool
        {
            Manufacturer = "Prusament",
            Barcode = 987654321,
            BarcodeType = Enums.BarcodeType.Ean,
            Material = new Material
            {
                Name = "PETG",
                Family = Enums.MaterialFamily.PetCopolyester,
                Manufacturer = "Prusament",
                DiameterMm = 1.75m,
                Finish = Enums.Finish.Silk,
                Opacity = Enums.Opacity.Opaque
            },
            Carrier = new Carrier
            {
                Manufacturer = "Prusa",
                SpoolType = Enums.SpoolType.GenericPlastic,
                EmptyWeightGrams = 245,
                SpoolRadius = 100,
                SpoolHubRadius = 30,
                SpoolHeight = 70
            }
        });
    }

    [RelayCommand]
    private Task AddMappingAsync()
    {
        return Task.CompletedTask;
    }

    [RelayCommand]
    private Task EditMappingAsync(Spool item)
    {
        return Task.CompletedTask;
    }

    [RelayCommand]
    private Task MergeMappingAsync(Spool item)
    {
        return Task.CompletedTask;
    }
}
