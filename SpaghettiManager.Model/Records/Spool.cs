using System.Drawing;

namespace SpaghettiManager.Model.Records;

public record Spool
{
    public string Manufacturer { get; init; } = "";     // e.g. "Bambu", "Polymaker", or "Generic"
    public string Model { get; init; } = "";            // e.g. "Reusable Spool", "Cardboard 1kg"
    public int Version { get; init; } = 1;

    public Enums.SpoolMaterial Material { get; init; } = Enums.SpoolMaterial.Unknown;

    // Dimensions (mm) — optional, but useful for AMS/compat checks
    public int? OuterDiameterMm { get; init; }
    public int? WidthMm { get; init; }
    public int? InnerHubDiameterMm { get; init; }

    // If you want to track physical spool colors (not filament color)
    public Color? SpoolColor { get; init; }

    // Mass of empty spool (helps remaining calc)
    public int? TareGrams { get; init; }

    // Compatibility notes (optional)
    public string? Notes { get; init; }                 // e.g. “Fits AMS with adapter ring”
}