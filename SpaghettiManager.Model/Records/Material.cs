namespace SpaghettiManager.Model.Records;

public record MaterialDefinition
{
    public string Name { get; init; } = "";           // e.g. "PLA Basic", "ASA", "PA6-CF"
    public string? Notes { get; init; }               // free text, e.g. “Prints best on textured PEI”

    public Enums.MaterialFamily MaterialFamily { get; init; } = Enums.MaterialFamily.Unknown;
    public Enums.MaterialCategory MaterialCategory { get; init; } = Enums.MaterialCategory.Unknown;
    public Enums.AdditiveMaterial Additives { get; init; } = Enums.AdditiveMaterial.None;

    public Enums.Hygroscopicity Hygroscopicity { get; init; } = Enums.Hygroscopicity.Unknown;
    public Enums.NozzleAbrasiveness Abrasiveness { get; init; } = Enums.NozzleAbrasiveness.Unknown;

    // Reasonable defaults (you can override per filament lot)
    public int? RecommendedNozzleTempMinC { get; init; }
    public int? RecommendedNozzleTempMaxC { get; init; }
    public int? RecommendedBedTempMinC { get; init; }
    public int? RecommendedBedTempMaxC { get; init; }
    public bool? EnclosureRecommended { get; init; }
}