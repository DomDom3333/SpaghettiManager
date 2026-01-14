namespace SpaghettiManager.Model.Records;

public record Manufacturer
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Country { get; set; }
    public string Website { get; set; }
    public string[] Aliases { get; set; }
}