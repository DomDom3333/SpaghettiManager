using SQLite;

namespace SpaghettiManager.Model.Records;

[Table("carriers")]
public sealed record Carrier
{
    [PrimaryKey]
    public Guid Id { get; init; } = Guid.NewGuid();
    public Enums.SpoolType SpoolType { get; set; }
    public int EmptyWeightGrams { get; set; }
    public string Manufacturer { get; set; }
    public int SpoolRadius { get; set; }
    public int SpoolHubRadius { get; set; }
    public int SpoolHeight { get; set; }

    public bool HighTemp { get; set; }
}
