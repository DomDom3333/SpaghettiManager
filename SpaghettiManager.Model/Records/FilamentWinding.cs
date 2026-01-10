namespace SpaghettiManager.Model.Records;

public record FilamentWinding : BaseEntity
{
    public FilamentLot Lot { get; init; } = new();
    public Carrier Carrier { get; init; } = new() { Kind = Enums.CarrierKind.Spool };

    public Enums.WindingDirection WindingDirection { get; init; } = Enums.WindingDirection.Unknown;

    // Tracking usage
    public Enums.InventoryStatus Status { get; init; } = Enums.InventoryStatus.Sealed;
    public DateTime? OpenedDate { get; init; }
    
    // Purchase / traceability
    public DateTime PurchaseDate { get; init; } = DateTime.Now;
    public string? PurchaseSource { get; init; }       // shop/site
    public decimal? PurchasePrice { get; init; }

    // Remaining tracking (pick one strategy; you can support both)
    public int? RemainingFilamentGrams => LastMeasuredTotalGrams - Carrier.TareGrams; // if you weigh / estimate
    public int? LastMeasuredTotalGrams { get; init; }       // total mass incl carrier at last weigh
    public DateTime? LastMeasuredAt { get; init; }

    // Optional: storage/drying metadata (kept minimal, extend later)
    public DateTime? LastDriedAt { get; init; }

    public string? Notes { get; init; }
}