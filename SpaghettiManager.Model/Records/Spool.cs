using SQLite;

namespace SpaghettiManager.Model.Records;

[Table("spools")]
public sealed record Spool
{
    [PrimaryKey]
    public Guid Id { get; init; } = Guid.NewGuid();
    public int Barcode { get; set; }
    public Enums.BarcodeType BarcodeType { get; set; }

    [Indexed]
    public Guid MaterialId { get; set; }

    [Indexed]
    public Guid CarrierId { get; set; }

    [Ignore]
    public Material Material { get; set; } = null!;

    public string? Manufacturer { get; set; }

    [Ignore]
    public Carrier Carrier { get; set; } = null!;
    public DateTimeOffset LastUpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public void Refill(Spool spool)
    {
        Material = spool.Material;
        Barcode = spool.Barcode;
        BarcodeType = spool.BarcodeType;
    }
}
