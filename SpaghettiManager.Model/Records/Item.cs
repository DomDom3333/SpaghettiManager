namespace SpaghettiManager.Model.Records;

public record Item
{
    public string Name { get; init; } = "";               // UI name, e.g. "Elegoo PLA+ Black"
    public string? InternalId { get; init; }              // optional UUID/string key
    public string? Barcode { get; init; }                 // inventory barcode, if YOU label it

    public FilamentWinding Winding { get; init; } = new();

    // Convenience defaults
    public DateTime CreatedAt { get; init; } = DateTime.Now;
}