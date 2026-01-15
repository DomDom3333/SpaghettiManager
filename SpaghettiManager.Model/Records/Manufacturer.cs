using System.Text.Json;
using SQLite;

namespace SpaghettiManager.Model.Records;

[Table("manufacturers")]
public record Manufacturer
{
    [Ignore]
    private string[]? aliases;

    [PrimaryKey]
    public string Id { get; set; }
    public string Name { get; set; }
    public string Country { get; set; }
    public string Website { get; set; }
    public string? AliasesJson { get; set; }

    [Ignore]
    public string[] Aliases
    {
        get
        {
            if (aliases is not null)
            {
                return aliases;
            }

            aliases = string.IsNullOrWhiteSpace(AliasesJson)
                ? Array.Empty<string>()
                : JsonSerializer.Deserialize<string[]>(AliasesJson) ?? Array.Empty<string>();
            return aliases;
        }
        set
        {
            aliases = value ?? Array.Empty<string>();
            AliasesJson = aliases.Length == 0
                ? null
                : JsonSerializer.Serialize(aliases);
        }
    }
}
