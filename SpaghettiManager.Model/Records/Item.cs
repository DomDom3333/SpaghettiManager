namespace SpaghettiManager.Model.Records;

public record Item : BaseEntity
{
    public string? InternalId { get; init; }              // optional UUID/string key
    public string? InventoryBarcode { get; init; }        // inventory barcode, if YOU label it
    public string CatalogBarcode { get; init; } = "";     // links to fixed manufacturer catalog data

    public CatalogItem? CatalogItem { get; init; }
    public InventoryItemOverrides Overrides { get; init; } = new();

    public FilamentWinding Winding { get; init; } = new();
}
