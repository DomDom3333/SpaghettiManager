using System.Drawing;
using SpaghettiManager.Model;
using SpaghettiManager.Model.Records;
using Color = System.Drawing.Color;

namespace SpaghettiManager.App.Services;

public static class InventoryFormatting
{
    public static string GetManufacturer(Item item)
        => GetEffectiveLot(item)?.Manufacturer ?? string.Empty;

    public static string GetProductLine(Item item)
        => GetEffectiveLot(item)?.ProductLine ?? string.Empty;

    public static string GetMaterialName(Item item)
        => GetEffectiveLot(item)?.Material.Name ?? string.Empty;

    public static string GetColorName(Item item)
        => GetEffectiveLot(item)?.ColorName ?? string.Empty;

    public static Enums.InventoryStatus GetStatus(Item item) => item.Winding.Status;

    public static int? GetRemainingGrams(Item item)
    {
        var carrier = GetEffectiveCarrier(item);
        return item.Winding.LastMeasuredTotalGrams - carrier?.TareGrams;
    }

    public static DateTime? GetOpenedDate(Item item) => item.Winding.OpenedDate;

    public static DateTime? GetLastMeasuredAt(Item item) => item.Winding.LastMeasuredAt;

    public static DateTime? GetLastDriedAt(Item item) => item.Winding.LastDriedAt;

    public static Enums.Hygroscopicity GetHygroscopicity(Item item)
        => GetEffectiveLot(item)?.Material.Hygroscopicity ?? Enums.Hygroscopicity.Unknown;

    public static string? GetColorHex(Item item)
        => ToHex(GetEffectiveLot(item)?.ColorApprox);

    public static string GetCarrierLabel(Item item)
        => GetEffectiveCarrier(item) is { } carrier ? GetCarrierLabel(carrier) : "Unknown carrier";

    public static string GetCarrierNotes(Item item)
        => GetEffectiveCarrier(item) is { } carrier ? GetCarrierNotes(carrier) : string.Empty;

    public static string GetManufacturer(CatalogItem entry) => entry.TemplateLot.Manufacturer;

    public static string GetProductLine(CatalogItem entry) => entry.TemplateLot.ProductLine;

    public static string GetMaterialName(CatalogItem entry) => entry.TemplateLot.Material.Name;

    public static string GetColorName(CatalogItem entry) => entry.TemplateLot.ColorName;

    public static Enums.FilamentDiameter GetDiameter(CatalogItem entry) => entry.TemplateLot.Diameter;

    public static string GetCarrierLabel(CatalogItem entry)
        => GetCarrierLabel(entry.DefaultCarrier);

    public static string GetCarrierLabel(Carrier carrier)
    {
        if (carrier.Kind == Enums.CarrierKind.Spool && carrier.Spool is not null)
        {
            var label = string.Join(" ",
                new[] { carrier.Spool.Manufacturer, carrier.Spool.Model }
                    .Where(value => !string.IsNullOrWhiteSpace(value)));

            return string.IsNullOrWhiteSpace(label) ? "Spool" : label;
        }

        return carrier.Kind switch
        {
            Enums.CarrierKind.Spool => "Spool",
            Enums.CarrierKind.SpoollessCoil => "Spoolless coil",
            Enums.CarrierKind.MasterSpoolRefill => "Master spool refill",
            _ => "Unknown carrier"
        };
    }

    public static string GetCarrierNotes(Carrier carrier)
    {
        var notes = new List<string>();
        if (!string.IsNullOrWhiteSpace(carrier.Notes))
        {
            notes.Add(carrier.Notes);
        }

        if (!string.IsNullOrWhiteSpace(carrier.Spool?.Notes))
        {
            notes.Add(carrier.Spool.Notes!);
        }

        return string.Join(" ", notes);
    }

    private static string? ToHex(Color? color)
    {
        if (color is null)
        {
            return null;
        }

        var value = color.Value;
        return value.A < byte.MaxValue
            ? $"#{value.A:X2}{value.R:X2}{value.G:X2}{value.B:X2}"
            : $"#{value.R:X2}{value.G:X2}{value.B:X2}";
    }

    private static FilamentLot? GetEffectiveLot(Item item)
        => item.Overrides.Lot ?? item.CatalogItem?.TemplateLot;

    private static Carrier? GetEffectiveCarrier(Item item)
        => item.Overrides.Carrier ?? item.CatalogItem?.DefaultCarrier;
}
