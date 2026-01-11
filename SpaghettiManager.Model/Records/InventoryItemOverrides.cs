namespace SpaghettiManager.Model.Records;

public record InventoryItemOverrides : BaseEntity
{
    public FilamentLot? Lot { get; init; }
    public Carrier? Carrier { get; init; }
    public int? NetWeightGrams { get; init; }
    public string? Notes { get; init; }
}
