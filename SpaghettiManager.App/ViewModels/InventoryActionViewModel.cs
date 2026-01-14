using SpaghettiManager.Model;
using SpaghettiManager.Model.Records;

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
    private Carrier carrier = CreateSampleCarrier();

    [ObservableProperty]
    private Material material = CreateSampleMaterial();

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

        Carrier = CreateSampleCarrier();
        Material = CreateSampleMaterial();
    }

    private static Carrier CreateSampleCarrier()
    {
        return new Carrier
        {
            Manufacturer = "Generic",
            SpoolType = Enums.SpoolType.GenericPlastic,
            EmptyWeightGrams = 230,
            SpoolRadius = 100,
            SpoolHubRadius = 30,
            SpoolHeight = 70,
            HighTemp = false
        };
    }

    private static Material CreateSampleMaterial()
    {
        return new Material
        {
            Name = "PLA",
            Family = Enums.MaterialFamily.Pla,
            Finish = Enums.Finish.Matte,
            Opacity = Enums.Opacity.Opaque,
            Manufacturer = "Generic",
            DiameterMm = 1.75m,
            Notes = "Add handling notes."
        };
    }
}
