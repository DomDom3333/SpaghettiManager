using SpaghettiManager.Model;

namespace SpaghettiManager.App.Services.Entities;

public record Manufacturer : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Country { get; set; }
    public string? Website { get; set; }
}
