namespace SpaghettiManager.Model.Records;

public sealed record Spool
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public int Barcode { get; set; }
    public Enums.BarcodeType BarcodeType { get; set; }
    public Material Material { get; set; } = null!;

    public string? Manufacturer { get; set; }
    
    public Carrier Carrier { get; set; }
    public DateTimeOffset LastUpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public void Refill(Spool spool)
    {
        Material = spool.Material;
        Barcode = spool.Barcode;
        BarcodeType = spool.BarcodeType;
    }
}