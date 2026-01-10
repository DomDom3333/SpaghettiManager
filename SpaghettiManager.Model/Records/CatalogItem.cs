namespace SpaghettiManager.Model.Records;

public record CatalogItem : BaseEntity
{
    /// <summary>
    /// The retail name of the product, e.g., "Elegoo Rapid PLA+ Black".
    /// </summary>
    public string Name { get; init; } = "";

    /// <summary>
    /// The manufacturer's barcode (EAN, UPC, or GTIN) scanned from the packaging.
    /// </summary>
    public string Barcode { get; init; } = "";

    /// <summary>
    /// Reuses the FilamentLot record to store all technical and visual data
    /// (Manufacturer, ProductLine, Material, Color, Diameter, etc.).
    /// </summary>
    public FilamentLot TemplateLot { get; init; } = new();

    /// <summary>
    /// Defines the default carrier this product comes with (e.g., a specific Spool type).
    /// This allows the system to automatically know the TareGrams when the item is added.
    /// </summary>
    public Carrier DefaultCarrier { get; init; } = new();

    /// <summary>
    /// The standard net weight of the filament for this catalog entry (e.g., 1000g, 2500g).
    /// </summary>
    public int DefaultNetWeightGrams { get; init; } = 1000;

    public string? Notes { get; init; }
}