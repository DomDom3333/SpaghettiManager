namespace SpaghettiManager.App.Services.Entities;

public class ManufacturerEntity
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Country { get; set; }
    public string? Website { get; set; }
}
