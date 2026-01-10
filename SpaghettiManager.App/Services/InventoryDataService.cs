using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Maui.Storage;
using SpaghettiManager.App.Services.Entities;
using SpaghettiManager.Model;

namespace SpaghettiManager.App.Services;

public record CatalogEntryDto(
    string Barcode,
    string Manufacturer,
    string ProductLine,
    string MaterialName,
    Enums.MaterialFamily MaterialFamily,
    Enums.Hygroscopicity Hygroscopicity,
    Enums.FilamentDiameter Diameter,
    string ColorName,
    string CarrierLabel,
    int? NominalWeightGrams);

public record InventoryItemDto(
    string Id,
    string Name,
    string? Barcode,
    string Manufacturer,
    string ProductLine,
    string MaterialName,
    Enums.MaterialFamily MaterialFamily,
    Enums.Hygroscopicity Hygroscopicity,
    Enums.FilamentDiameter Diameter,
    string ColorName,
    string? ColorHex,
    Enums.InventoryStatus Status,
    int? RemainingGrams,
    string CarrierLabel,
    string? CarrierNotes,
    int? CarrierTareGrams,
    DateTime CreatedAt,
    DateTime? OpenedDate,
    DateTime? LastMeasuredAt,
    DateTime? LastDriedAt,
    string? Notes);

public record InventoryOptions(
    IReadOnlyList<string> Manufacturers,
    IReadOnlyList<string> ProductLines,
    IReadOnlyList<string> Materials,
    IReadOnlyList<string> Colors,
    IReadOnlyList<string> Carriers);

public class InventoryDataService
{
    private readonly InventoryDbContext dbContext;
    private readonly SemaphoreSlim initLock = new(1, 1);
    private bool initialized;

    public InventoryDataService(InventoryDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task InitializeAsync()
    {
        if (initialized)
        {
            return;
        }

        await initLock.WaitAsync();
        try
        {
            if (initialized)
            {
                return;
            }

            await dbContext.Database.EnsureCreatedAsync();
            await SeedManufacturersAsync();
            await SeedCatalogAsync();
            await SeedInventoryAsync();
            initialized = true;
        }
        finally
        {
            initLock.Release();
        }
    }

    public async Task<IReadOnlyList<InventoryItemDto>> GetItemsAsync()
    {
        await InitializeAsync();
        return await dbContext.InventoryItems
            .AsNoTracking()
            .OrderBy(item => item.Manufacturer)
            .ThenBy(item => item.ProductLine)
            .Select(item => new InventoryItemDto(
                item.Id,
                item.Name,
                item.Barcode,
                item.Manufacturer,
                item.ProductLine,
                item.MaterialName,
                item.MaterialFamily,
                item.Hygroscopicity,
                item.Diameter,
                item.ColorName,
                item.ColorHex,
                item.Status,
                item.RemainingGrams,
                item.CarrierLabel,
                item.CarrierNotes,
                item.CarrierTareGrams,
                item.CreatedAt,
                item.OpenedDate,
                item.LastMeasuredAt,
                item.LastDriedAt,
                item.Notes))
            .ToListAsync();
    }

    public async Task<InventoryItemDto?> GetItemByIdAsync(string? id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return null;
        }

        await InitializeAsync();
        var entity = await dbContext.InventoryItems
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == id);

        return entity is null
            ? null
            : new InventoryItemDto(
                entity.Id,
                entity.Name,
                entity.Barcode,
                entity.Manufacturer,
                entity.ProductLine,
                entity.MaterialName,
                entity.MaterialFamily,
                entity.Hygroscopicity,
                entity.Diameter,
                entity.ColorName,
                entity.ColorHex,
                entity.Status,
                entity.RemainingGrams,
                entity.CarrierLabel,
                entity.CarrierNotes,
                entity.CarrierTareGrams,
                entity.CreatedAt,
                entity.OpenedDate,
                entity.LastMeasuredAt,
                entity.LastDriedAt,
                entity.Notes);
    }

    public async Task<CatalogEntryDto?> GetCatalogEntryAsync(string? barcode)
    {
        if (string.IsNullOrWhiteSpace(barcode))
        {
            return null;
        }

        await InitializeAsync();
        var entity = await dbContext.CatalogEntries
            .AsNoTracking()
            .FirstOrDefaultAsync(entry => entry.Barcode == barcode);

        return entity is null
            ? null
            : new CatalogEntryDto(
                entity.Barcode,
                entity.Manufacturer,
                entity.ProductLine,
                entity.MaterialName,
                entity.MaterialFamily,
                entity.Hygroscopicity,
                entity.Diameter,
                entity.ColorName,
                entity.CarrierLabel,
                entity.NominalWeightGrams);
    }

    public async Task<InventoryOptions> GetOptionsAsync()
    {
        await InitializeAsync();
        var manufacturers = await BuildOptionsAsync(dbContext.InventoryItems.Select(item => item.Manufacturer),
            dbContext.CatalogEntries.Select(entry => entry.Manufacturer),
            dbContext.Manufacturers.Select(manufacturer => manufacturer.Name));

        var productLines = await BuildOptionsAsync(dbContext.InventoryItems.Select(item => item.ProductLine),
            dbContext.CatalogEntries.Select(entry => entry.ProductLine));

        var materials = await BuildOptionsAsync(dbContext.InventoryItems.Select(item => item.MaterialName),
            dbContext.CatalogEntries.Select(entry => entry.MaterialName));

        var colors = await BuildOptionsAsync(dbContext.InventoryItems.Select(item => item.ColorName),
            dbContext.CatalogEntries.Select(entry => entry.ColorName));

        var carriers = await BuildOptionsAsync(dbContext.InventoryItems.Select(item => item.CarrierLabel),
            dbContext.CatalogEntries.Select(entry => entry.CarrierLabel));

        return new InventoryOptions(manufacturers, productLines, materials, colors, carriers);
    }

    public async Task MarkEmptyAsync(string? id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return;
        }

        await InitializeAsync();
        var entity = await dbContext.InventoryItems.FirstOrDefaultAsync(item => item.Id == id);
        if (entity is null)
        {
            return;
        }

        entity.Status = Enums.InventoryStatus.Empty;
        entity.RemainingGrams = 0;
        entity.LastMeasuredAt = DateTime.Now;
        await dbContext.SaveChangesAsync();
    }

    private async Task SeedManufacturersAsync()
    {
        if (await dbContext.Manufacturers.AnyAsync())
        {
            return;
        }

        using var stream = await FileSystem.OpenAppPackageFileAsync("Data/Manufacturers.json");
        using var reader = new StreamReader(stream);
        var json = await reader.ReadToEndAsync();

        var manufacturers = JsonSerializer.Deserialize<List<ManufacturerSeed>>(json, JsonOptions())
            ?? new List<ManufacturerSeed>();

        var entities = manufacturers
            .Where(item => !string.IsNullOrWhiteSpace(item.Id) && !string.IsNullOrWhiteSpace(item.Name))
            .Select(item => new ManufacturerEntity
            {
                Id = item.Id!,
                Name = item.Name!,
                Country = item.Country,
                Website = item.Website
            })
            .ToList();

        if (entities.Count == 0)
        {
            return;
        }

        dbContext.Manufacturers.AddRange(entities);
        await dbContext.SaveChangesAsync();
    }

    private async Task SeedCatalogAsync()
    {
        if (await dbContext.CatalogEntries.AnyAsync())
        {
            return;
        }

        dbContext.CatalogEntries.AddRange(BuildSeedCatalog());
        await dbContext.SaveChangesAsync();
    }

    private async Task SeedInventoryAsync()
    {
        if (await dbContext.InventoryItems.AnyAsync())
        {
            return;
        }

        dbContext.InventoryItems.AddRange(BuildSeedInventory());
        await dbContext.SaveChangesAsync();
    }

    private static IEnumerable<CatalogEntryEntity> BuildSeedCatalog()
    {
        return new List<CatalogEntryEntity>
        {
            new()
            {
                Barcode = "8594185641234",
                Manufacturer = "Prusament",
                ProductLine = "PLA",
                MaterialName = "PLA",
                MaterialFamily = Enums.MaterialFamily.Pla,
                Hygroscopicity = Enums.Hygroscopicity.Low,
                Diameter = Enums.FilamentDiameter.Mm175,
                ColorName = "Galaxy Black",
                CarrierLabel = "Prusa Plastic spool",
                NominalWeightGrams = 1000
            },
            new()
            {
                Barcode = "0606529823541",
                Manufacturer = "Overture",
                ProductLine = "PETG",
                MaterialName = "PETG",
                MaterialFamily = Enums.MaterialFamily.PetCopolyester,
                Hygroscopicity = Enums.Hygroscopicity.Medium,
                Diameter = Enums.FilamentDiameter.Mm175,
                ColorName = "Fire Engine Red",
                CarrierLabel = "Overture Plastic spool",
                NominalWeightGrams = 1000
            },
            new()
            {
                Barcode = "4567890123456",
                Manufacturer = "Polymaker",
                ProductLine = "TPU 95A",
                MaterialName = "TPU",
                MaterialFamily = Enums.MaterialFamily.FlexibleTpe,
                Hygroscopicity = Enums.Hygroscopicity.High,
                Diameter = Enums.FilamentDiameter.Mm175,
                ColorName = "Clear",
                CarrierLabel = "Polymaker Cardboard spool",
                NominalWeightGrams = 1000
            }
        };
    }

    private static IEnumerable<InventoryItemEntity> BuildSeedInventory()
    {
        return new List<InventoryItemEntity>
        {
            new()
            {
                Id = "INV-1001",
                Name = "Prusament PLA Galaxy Black",
                Barcode = "8594185641234",
                Manufacturer = "Prusament",
                ProductLine = "PLA",
                MaterialName = "PLA",
                MaterialFamily = Enums.MaterialFamily.Pla,
                Hygroscopicity = Enums.Hygroscopicity.Low,
                Diameter = Enums.FilamentDiameter.Mm175,
                ColorName = "Galaxy Black",
                ColorHex = "#FF1C1C1C",
                Status = Enums.InventoryStatus.InUse,
                RemainingGrams = 320,
                CarrierLabel = "Prusa Plastic spool",
                CarrierNotes = "Fits AMS with standard adapter",
                CarrierTareGrams = 240,
                CreatedAt = DateTime.Today.AddDays(-30),
                OpenedDate = DateTime.Today.AddDays(-18),
                LastMeasuredAt = DateTime.Today.AddDays(-2),
                LastDriedAt = DateTime.Today.AddDays(-20),
                Notes = "Keep in dry box after use."
            },
            new()
            {
                Id = "INV-1002",
                Name = "Overture PETG Fire Engine Red",
                Barcode = "0606529823541",
                Manufacturer = "Overture",
                ProductLine = "PETG",
                MaterialName = "PETG",
                MaterialFamily = Enums.MaterialFamily.PetCopolyester,
                Hygroscopicity = Enums.Hygroscopicity.Medium,
                Diameter = Enums.FilamentDiameter.Mm175,
                ColorName = "Fire Engine Red",
                ColorHex = "#FFC62828",
                Status = Enums.InventoryStatus.Opened,
                RemainingGrams = null,
                CarrierLabel = "Overture Plastic spool",
                CarrierNotes = "AMS compatible",
                CarrierTareGrams = 220,
                CreatedAt = DateTime.Today.AddDays(-14),
                OpenedDate = DateTime.Today.AddDays(-7),
                LastMeasuredAt = null,
                LastDriedAt = DateTime.Today.AddDays(-35),
                Notes = "Needs drying before long print."
            },
            new()
            {
                Id = "INV-1003",
                Name = "Polymaker TPU Clear",
                Barcode = "4567890123456",
                Manufacturer = "Polymaker",
                ProductLine = "TPU 95A",
                MaterialName = "TPU",
                MaterialFamily = Enums.MaterialFamily.FlexibleTpe,
                Hygroscopicity = Enums.Hygroscopicity.High,
                Diameter = Enums.FilamentDiameter.Mm175,
                ColorName = "Clear",
                ColorHex = "#FFE0E0E0",
                Status = Enums.InventoryStatus.Sealed,
                RemainingGrams = 1000,
                CarrierLabel = "Polymaker Cardboard spool",
                CarrierNotes = "Use AMS ring for cardboard",
                CarrierTareGrams = 180,
                CreatedAt = DateTime.Today.AddDays(-10),
                OpenedDate = null,
                LastMeasuredAt = DateTime.Today.AddDays(-12),
                LastDriedAt = DateTime.Today.AddDays(-60),
                Notes = "Store sealed with desiccant."
            }
        };
    }

    private static async Task<IReadOnlyList<string>> BuildOptionsAsync(params IQueryable<string?>[] sources)
    {
        var results = new List<string>();
        foreach (var source in sources)
        {
            var values = await source
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Select(value => value!.Trim())
                .Distinct()
                .ToListAsync();
            results.AddRange(values);
        }

        var options = results
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(value => value, StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (!options.Contains("Unknown", StringComparer.OrdinalIgnoreCase))
        {
            options.Add("Unknown");
        }

        return options;
    }

    private static JsonSerializerOptions JsonOptions()
    {
        return new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    private sealed class ManufacturerSeed
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? Country { get; set; }
        public string? Website { get; set; }
    }
}
