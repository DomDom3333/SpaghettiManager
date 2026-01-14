namespace SpaghettiManager.Model.Records;

public sealed record Material
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public Enums.MaterialFamily Family { get; set; }
    public Enums.AdditiveMaterial AdditiveMaterial { get; set; } = Enums.AdditiveMaterial.None;
    
    public string Name { get; set; } = null!;
    public string? Color { get; set; }
    public Enums.Opacity Opacity { get; set; }
    public Enums.Finish Finish { get; set; }
    public string? Manufacturer { get; set; }
    public decimal DiameterMm { get; set; } = 1.75m;
    
    public decimal? Density_g_cm3 { get; set; }
    public int? GlassTransitionC { get; set; }

    public string? Notes { get; set; }
}