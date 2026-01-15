using System.Text.Json.Serialization;

namespace SpaghettiManager.Model.Records;

/// <summary>
/// Represents a single filament entry from thefilamentdb.jsonl seed file.
/// </summary>
public sealed record FilamentDbEntry
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("material")]
    public string Material { get; set; } = string.Empty;

    [JsonPropertyName("brand")]
    public string Brand { get; set; } = string.Empty;

    [JsonPropertyName("filamentPage")]
    public string FilamentPage { get; set; } = string.Empty;

    [JsonPropertyName("colorName")]
    public string ColorName { get; set; } = string.Empty;

    [JsonPropertyName("colorHex")]
    public string ColorHex { get; set; } = string.Empty;

    [JsonPropertyName("isColorBlended")]
    public bool IsColorBlended { get; set; }

    [JsonPropertyName("diameter")]
    public decimal Diameter { get; set; }

    [JsonPropertyName("provenance")]
    public string Provenance { get; set; } = string.Empty;

    [JsonPropertyName("factoryInfo")]
    public string FactoryInfo { get; set; } = string.Empty;

    [JsonPropertyName("moreDetails")]
    public string[]? MoreDetails { get; set; }
}
