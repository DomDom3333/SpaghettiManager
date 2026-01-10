using SpaghettiManager.Model;

namespace SpaghettiManager.App.Services.Entities;

public class CatalogEntryEntity
{
    public string Barcode { get; set; } = string.Empty;
    public string Manufacturer { get; set; } = string.Empty;
    public string ProductLine { get; set; } = string.Empty;
    public string MaterialName { get; set; } = string.Empty;
    public Enums.MaterialFamily MaterialFamily { get; set; }
    public Enums.Hygroscopicity Hygroscopicity { get; set; }
    public Enums.FilamentDiameter Diameter { get; set; }
    public string ColorName { get; set; } = string.Empty;
    public string CarrierLabel { get; set; } = string.Empty;
    public int? NominalWeightGrams { get; set; }
}
