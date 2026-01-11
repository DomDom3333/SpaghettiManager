using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Maui.Storage;
using SpaghettiManager.Model.Records;

namespace SpaghettiManager.App.Services;

public class InventoryDataService
{
    private const int SchemaVersion = 3; // bump when model changes requiring rebuild

    private readonly InventoryDbContext dbContext;
    private readonly ILogger<InventoryDataService> logger;
    private readonly Task initializationTask;
    private IReadOnlyList<string>? cachedManufacturers;

    public InventoryDataService(
        InventoryDbContext dbContext,
        ILogger<InventoryDataService> logger)
    {
        this.dbContext = dbContext;
        this.logger = logger;
        initializationTask = InitializeAsync();
    }

    public async Task<IReadOnlyList<Item>> GetItemsAsync()
    {
        await initializationTask;
        return await dbContext.InventoryItems
            .Include(item => item.CatalogItem)
            .AsNoTracking()
            .OrderByDescending(item => item.CreatedAt)
            .ToListAsync();
    }

    public async Task<Item?> GetItemByIdAsync(string? id)
    {
        await initializationTask;
        if (string.IsNullOrWhiteSpace(id))
        {
            return null;
        }

        if (Guid.TryParse(id, out var guid))
        {
            return await dbContext.InventoryItems
                .Include(item => item.CatalogItem)
                .AsNoTracking()
                .FirstOrDefaultAsync(item => item.Id == guid);
        }

        return await dbContext.InventoryItems
            .Include(item => item.CatalogItem)
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.InternalId == id);
    }

    public async Task<CatalogItem?> GetCatalogEntryAsync(string barcode)
    {
        await initializationTask;
        if (string.IsNullOrWhiteSpace(barcode))
        {
            return null;
        }

        return await dbContext.CatalogEntries
            .AsNoTracking()
            .FirstOrDefaultAsync(entry => entry.Barcode == barcode);
    }

    public async Task MarkEmptyAsync(string id)
    {
        await initializationTask;
        if (string.IsNullOrWhiteSpace(id))
        {
            return;
        }

        Item? item;
        if (Guid.TryParse(id, out var guid))
        {
            item = await dbContext.InventoryItems
                .AsNoTracking()
                .FirstOrDefaultAsync(value => value.Id == guid);
        }
        else
        {
            item = await dbContext.InventoryItems
                .AsNoTracking()
                .FirstOrDefaultAsync(value => value.InternalId == id);
        }

        if (item is null)
        {
            return;
        }

        var updated = item with
        {
            UpdatedAt = DateTime.UtcNow,
            Winding = item.Winding with
            {
                Status = SpaghettiManager.Model.Enums.InventoryStatus.Empty
            }
        };

        dbContext.InventoryItems.Update(updated);
        await dbContext.SaveChangesAsync();
    }

    public async Task<InventoryOptions> GetOptionsAsync()
    {
        await initializationTask;
        var items = await dbContext.InventoryItems
            .Include(item => item.CatalogItem)
            .AsNoTracking()
            .ToListAsync();
        var catalogEntries = await dbContext.CatalogEntries.AsNoTracking().ToListAsync();
        var manufacturers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var name in await GetManufacturerOptionsAsync())
        {
            manufacturers.Add(name);
        }

        foreach (var item in items)
        {
            AddIfNotEmpty(manufacturers, InventoryFormatting.GetManufacturer(item));
        }

        foreach (var entry in catalogEntries)
        {
            AddIfNotEmpty(manufacturers, InventoryFormatting.GetManufacturer(entry));
        }

        var productLines = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var item in items)
        {
            AddIfNotEmpty(productLines, InventoryFormatting.GetProductLine(item));
        }

        foreach (var entry in catalogEntries)
        {
            AddIfNotEmpty(productLines, InventoryFormatting.GetProductLine(entry));
        }

        var materials = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var item in items)
        {
            AddIfNotEmpty(materials, InventoryFormatting.GetMaterialName(item));
        }

        foreach (var entry in catalogEntries)
        {
            AddIfNotEmpty(materials, InventoryFormatting.GetMaterialName(entry));
        }

        var colors = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var item in items)
        {
            AddIfNotEmpty(colors, InventoryFormatting.GetColorName(item));
        }

        foreach (var entry in catalogEntries)
        {
            AddIfNotEmpty(colors, InventoryFormatting.GetColorName(entry));
        }

        var carriers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var item in items)
        {
            AddIfNotEmpty(carriers, InventoryFormatting.GetCarrierLabel(item));
        }

        foreach (var entry in catalogEntries)
        {
            AddIfNotEmpty(carriers, InventoryFormatting.GetCarrierLabel(entry));
        }

        return new InventoryOptions(
            manufacturers.OrderBy(value => value).ToList(),
            productLines.OrderBy(value => value).ToList(),
            materials.OrderBy(value => value).ToList(),
            colors.OrderBy(value => value).ToList(),
            carriers.OrderBy(value => value).ToList());
    }

    private async Task InitializeAsync()
    {
        try
        {
            var current = Preferences.Get("InventoryDb.SchemaVersion", 0);
            if (current != SchemaVersion)
            {
                // Schema changed; rebuild the local database
                await dbContext.Database.EnsureDeletedAsync();
                await dbContext.Database.EnsureCreatedAsync();
                Preferences.Set("InventoryDb.SchemaVersion", SchemaVersion);
                return;
            }

            await dbContext.Database.EnsureCreatedAsync();
        }
        catch (Exception ex)
        {
            // If anything went wrong (e.g., schema drift), nuke and recreate once
            try
            {
                await dbContext.Database.EnsureDeletedAsync();
                await dbContext.Database.EnsureCreatedAsync();
                Preferences.Set("InventoryDb.SchemaVersion", SchemaVersion);
            }
            catch (Exception inner)
            {
                // Last resort: log and rethrow the original
                logger.LogError(inner, "Failed to rebuild database after initialization error: {Message}", ex.Message);
                throw;
            }
        }
    }

    private async Task<IReadOnlyList<string>> GetManufacturerOptionsAsync()
    {
        if (cachedManufacturers is not null)
        {
            return cachedManufacturers;
        }

        try
        {
            await using var stream = await FileSystem.OpenAppPackageFileAsync("Data/Manufacturers.json");
            var list = await JsonSerializer.DeserializeAsync<List<ManufacturerSeed>>(
                stream,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            cachedManufacturers = list?
                .Select(item => item.Name)
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Select(name => name!.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(name => name)
                .ToList()
                ?? new List<string>();
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to load manufacturer data from packaged JSON.");
            cachedManufacturers = Array.Empty<string>();
        }

        return cachedManufacturers;
    }

    private static void AddIfNotEmpty(ISet<string> set, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            set.Add(value.Trim());
        }
    }

    private record ManufacturerSeed(string? Name);
}

public record InventoryOptions(
    IReadOnlyList<string> Manufacturers,
    IReadOnlyList<string> ProductLines,
    IReadOnlyList<string> Materials,
    IReadOnlyList<string> Colors,
    IReadOnlyList<string> Carriers);
