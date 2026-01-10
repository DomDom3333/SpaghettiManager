namespace SpaghettiManager.Model.Records;

public record Carrier
{
    public Enums.CarrierKind Kind { get; init; } = Enums.CarrierKind.Unknown;

    // Only set when Kind == Spool
    public Spool? Spool { get; init; }

    // For coils/refills: optionally store some physical info
    public int? TareGrams { get; init; } // coil core, refill ring, etc.
    public string? Notes { get; init; }
}