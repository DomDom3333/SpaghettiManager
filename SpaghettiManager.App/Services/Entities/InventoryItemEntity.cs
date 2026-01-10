using SpaghettiManager.Model;

namespace SpaghettiManager.App.Services.Entities;

public class InventoryItemEntity
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = string.Empty;
    public string? Barcode { get; set; }

    public string Manufacturer { get; set; } = string.Empty;
    public string ProductLine { get; set; } = string.Empty;
    public string MaterialName { get; set; } = string.Empty;
    public Enums.MaterialFamily MaterialFamily { get; set; }
    public Enums.Hygroscopicity Hygroscopicity { get; set; }
    public Enums.FilamentDiameter Diameter { get; set; }

    public string ColorName { get; set; } = string.Empty;
    public string? ColorHex { get; set; }

    public Enums.InventoryStatus Status { get; set; }
    public int? RemainingGrams { get; set; }

    public string CarrierLabel { get; set; } = string.Empty;
    public string? CarrierNotes { get; set; }
    public int? CarrierTareGrams { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? OpenedDate { get; set; }
    public DateTime? LastMeasuredAt { get; set; }
    public DateTime? LastDriedAt { get; set; }
    public string? Notes { get; set; }
}
