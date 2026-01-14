using SpaghettiManager.Model;
using SpaghettiManager.Model.Records;

namespace SpaghettiManager.App.ViewModels;

public partial class CatalogSectionViewModel : ObservableObject, IQueryAttributable
{
    [ObservableProperty]
    private string sectionTitle = string.Empty;

    public ObservableCollection<object> Items { get; } = new();

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
                Items.Add(new Manufacturer
                {
                    Id = "prusament",
                    Name = "Prusament",
                    Country = "Czech Republic",
                    Website = "https://prusament.com",
                    Aliases = new[] { "Prusa Research" }
                });
                Items.Add(new Manufacturer
                {
                    Id = "overture",
                    Name = "Overture",
                    Country = "China",
                    Website = "https://overture3d.com",
                    Aliases = new[] { "Overture 3D" }
                });
                Items.Add(new Manufacturer
                {
                    Id = "polymaker",
                    Name = "Polymaker",
                    Country = "China",
                    Website = "https://polymaker.com",
                    Aliases = new[] { "PolyMaker" }
                });
                break;
            case "spools":
                SectionTitle = "Spools / carriers";
                Items.Add(new Carrier
                {
                    Manufacturer = "Prusa",
                    SpoolType = Enums.SpoolType.GenericPlastic,
                    EmptyWeightGrams = 245,
                    SpoolRadius = 100,
                    SpoolHubRadius = 30,
                    SpoolHeight = 70,
                    HighTemp = false
                });
                Items.Add(new Carrier
                {
                    Manufacturer = "Overture",
                    SpoolType = Enums.SpoolType.Cardboard,
                    EmptyWeightGrams = 180,
                    SpoolRadius = 100,
                    SpoolHubRadius = 32,
                    SpoolHeight = 65,
                    HighTemp = false
                });
                Items.Add(new Carrier
                {
                    Manufacturer = "Bambu Lab",
                    SpoolType = Enums.SpoolType.Reusable,
                    EmptyWeightGrams = 215,
                    SpoolRadius = 98,
                    SpoolHubRadius = 30,
                    SpoolHeight = 70,
                    HighTemp = true
                });
                break;
            case "materials":
                SectionTitle = "Materials";
                Items.Add(new Material
                {
                    Name = "PLA",
                    Family = Enums.MaterialFamily.Pla,
                    Finish = Enums.Finish.Glossy,
                    Opacity = Enums.Opacity.Opaque,
                    Manufacturer = "Prusament",
                    DiameterMm = 1.75m
                });
                Items.Add(new Material
                {
                    Name = "PETG",
                    Family = Enums.MaterialFamily.PetCopolyester,
                    Finish = Enums.Finish.Silk,
                    Opacity = Enums.Opacity.Opaque,
                    Manufacturer = "Overture",
                    DiameterMm = 1.75m
                });
                Items.Add(new Material
                {
                    Name = "TPU",
                    Family = Enums.MaterialFamily.FlexibleTpe,
                    Finish = Enums.Finish.Matte,
                    Opacity = Enums.Opacity.Transparent,
                    Manufacturer = "Polymaker",
                    DiameterMm = 1.75m
                });
                break;
            case "additives":
                SectionTitle = "Additives";
                Items.Add(new Material
                {
                    Name = "Carbon Fiber PLA",
                    Family = Enums.MaterialFamily.Pla,
                    AdditiveMaterial = Enums.AdditiveMaterial.CarbonFiber,
                    Finish = Enums.Finish.Textured,
                    Opacity = Enums.Opacity.Opaque,
                    Manufacturer = "Generic",
                    DiameterMm = 1.75m
                });
                Items.Add(new Material
                {
                    Name = "Glow-in-the-dark PETG",
                    Family = Enums.MaterialFamily.PetCopolyester,
                    AdditiveMaterial = Enums.AdditiveMaterial.Phosphorescent,
                    Finish = Enums.Finish.Sparkle,
                    Opacity = Enums.Opacity.Opaque,
                    Manufacturer = "Generic",
                    DiameterMm = 1.75m
                });
                break;
            default:
                SectionTitle = "Catalog";
                Items.Add(new Material
                {
                    Name = "No data loaded",
                    Family = Enums.MaterialFamily.Unknown,
                    Opacity = Enums.Opacity.Unknown,
                    Finish = Enums.Finish.Unknown,
                    Manufacturer = "Unknown",
                    DiameterMm = 1.75m
                });
                break;
        }
    }
}
