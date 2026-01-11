using System.Drawing;
using System.ComponentModel.DataAnnotations.Schema;

namespace SpaghettiManager.Model.Records;

public record FilamentLot : BaseEntity
{
    public string Manufacturer { get; init; } = "";     // e.g. "Elegoo"
    public string ProductLine { get; init; } = "";      // e.g. "Rapid PLA+", "PLA Basic"
    public string? Sku { get; init; }                   // vendor/manufacturer SKU
    public string? BatchLotCode { get; init; }          // spool label / lot code

    public MaterialDefinition Material { get; init; } = new();

    public Enums.FilamentDiameter Diameter { get; init; } = Enums.FilamentDiameter.Mm175;
    public decimal DiameterToleranceMm { get; init; } = 0.03m; // common default

    // Visual / identification
    public string ColorName { get; init; } = "";        // "Black", "Jade White"
    [NotMapped]
    public Color? ColorApprox { get; init; }            // optional UI convenience
    public Enums.Opacity Opacity { get; init; } = Enums.Opacity.Unknown;
    public Enums.Finish Finish { get; init; } = Enums.Finish.Unknown;
}