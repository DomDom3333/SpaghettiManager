using System;
using System.Linq;
using System.Text.Json.Serialization;
using SQLite;

namespace SpaghettiManager.Model.Records;

[Table("manufacturers")]
public record Manufacturer
{
    [PrimaryKey]
    [JsonPropertyName("id")]
    public string Id { get; set; }
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    [JsonPropertyName("country")]
    public string? Country { get; set; }
    [JsonPropertyName("website")]
    public string? Website { get; set; }

    // Stored as a semicolon-separated string in JSON seed and in the DB column.
    [JsonPropertyName("aliases")]
    [Column("aliases")]
    public string? Aliases { get; set; }

    // Helper property for code usage: returns aliases as a trimmed string array and can set the Aliases string.
    [Ignore]
    public string[]? AliasesList
    {
        get => string.IsNullOrWhiteSpace(Aliases)
            ? []
            : Aliases.Split(';', StringSplitOptions.RemoveEmptyEntries);
        set
        {
            if (value == null || value.Length == 0)
            {
                Aliases = string.Empty;
                return;
            }

            Aliases = string.Join("; ", value.Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => s.Trim()));
        }
    }

}
