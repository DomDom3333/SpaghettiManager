using System.Text.Json;
using GoogleGson;
using SpaghettiManager.Model;
using SpaghettiManager.Model.Records;

namespace SpaghettiManager.App.Services;

[Singleton]
public class SpaghettiDatabase
{
    private readonly MySqliteConnection connection;
    private readonly TimeProvider timeProvider;
    private readonly SemaphoreSlim initializationLock = new(1, 1);
    private bool initialized;

    public SpaghettiDatabase(
        MySqliteConnection connection,
        TimeProvider timeProvider
    )
    {
        this.connection = connection;
        this.timeProvider = timeProvider;
    }

    public async Task InitializeAsync()
    {
        if (initialized)
        {
            return;
        }

        await initializationLock.WaitAsync();
        try
        {
            if (initialized)
            {
                return;
            }

            await connection.CreateTableAsync<Manufacturer>();
            await connection.CreateTableAsync<Material>();
            await connection.CreateTableAsync<Carrier>();
            await connection.CreateTableAsync<Spool>();

            initialized = true;
        }
        finally
        {
            initializationLock.Release();
        }

        await SeedAsync();
    }

    public virtual async Task SeedAsync()
    {
        await SeedManufacturersAsync();
        await SeedFilamentsAsync();
    }

    private async Task SeedManufacturersAsync()
    {
        // Check if already seeded
        var existingCount = await connection.Table<Manufacturer>().CountAsync();
        if (existingCount > 0)
        {
            return;
        }

        await using var stream = await FileSystem.OpenAppPackageFileAsync("Data/Manufacturers.json");
        using var reader = new StreamReader(stream);
        var json = await reader.ReadToEndAsync();
        
        Manufacturer[]? data = JsonSerializer.Deserialize<Manufacturer[]>(json);
        if (data == null || data.Length == 0)
        {
            return;
        }

        await connection.InsertAllAsync(data);
    }

    private async Task SeedFilamentsAsync()
    {
        // Check if already seeded
        var existingCount = await connection.Table<Material>().CountAsync();
        if (existingCount > 0)
        {
            return;
        }

        await using var stream = await FileSystem.OpenAppPackageFileAsync("Data/thefilamentdb.jsonl");
        using var reader = new StreamReader(stream);

        var materials = new List<Material>();
        var manufacturers = new Dictionary<string, Manufacturer>(StringComparer.OrdinalIgnoreCase);

        // Get existing manufacturers for lookup
        var existingManufacturers = await connection.Table<Manufacturer>().ToListAsync();
        foreach (var m in existingManufacturers)
        {
            manufacturers[m.Id] = m;
            if (!string.IsNullOrEmpty(m.Name))
            {
                manufacturers[m.Name] = m;
            }
        }

        string? line;
        while ((line = await reader.ReadLineAsync()) != null)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            FilamentDbEntry? entry;
            try
            {
                entry = JsonSerializer.Deserialize<FilamentDbEntry>(line);
            }
            catch (JsonException)
            {
                continue;
            }

            if (entry == null)
            {
                continue;
            }

            // Parse material family from material string
            var materialFamily = ParseMaterialFamily(entry.Material);
            var additives = ParseAdditives(entry.Material, entry.MoreDetails);
            var opacity = ParseOpacity(entry.ColorName, entry.MoreDetails);
            var finish = ParseFinish(entry.ColorName, entry.MoreDetails);

            var material = new Material
            {
                Id = Guid.NewGuid(),
                Name = $"{entry.Brand} {entry.Name}".Trim(),
                Family = materialFamily,
                AdditiveMaterial = additives,
                Color = entry.ColorHex.StartsWith("#") ? entry.ColorHex : $"#{entry.ColorHex}",
                Opacity = opacity,
                Finish = finish,
                Manufacturer = entry.Brand,
                DiameterMm = entry.Diameter > 0 ? entry.Diameter : 1.75m,
                Notes = BuildNotes(entry)
            };

            materials.Add(material);

            // Track manufacturers not already in DB
            if (!string.IsNullOrWhiteSpace(entry.Brand) && !manufacturers.ContainsKey(entry.Brand))
            {
                var brandId = entry.Brand.ToLowerInvariant().Replace(" ", "-");
                var newManufacturer = new Manufacturer
                {
                    Id = brandId,
                    Name = entry.Brand,
                    Country = entry.Provenance != "Unknown" ? entry.Provenance : null,
                    Website = entry.FilamentPage != "Unknown" ? TryExtractDomain(entry.FilamentPage) : null
                };
                manufacturers[entry.Brand] = newManufacturer;
            }
        }

        // Insert new manufacturers that weren't in the seed file
        var newManufacturers = manufacturers.Values
            .GroupBy(m => m.Id)
            .Select(g => g.First())
            .Where(m => !existingManufacturers.Any(e => e.Id == m.Id))
            .ToList();

        if (newManufacturers.Count > 0)
        {
            await connection.InsertAllAsync(newManufacturers);
        }

        if (materials.Count > 0)
        {
            await connection.InsertAllAsync(materials);
        }
    }

    private static Enums.MaterialFamily ParseMaterialFamily(string material)
    {
        if (string.IsNullOrWhiteSpace(material))
        {
            return Enums.MaterialFamily.Unknown;
        }

        var upper = material.ToUpperInvariant();
        
        return upper switch
        {
            _ when upper.Contains("PLA") => Enums.MaterialFamily.Pla,
            _ when upper.Contains("PETG") || upper.Contains("PET-G") => Enums.MaterialFamily.PetCopolyester,
            _ when upper.Contains("PET") && !upper.Contains("PETG") => Enums.MaterialFamily.PetCopolyester,
            _ when upper.Contains("PCTG") || upper.Contains("CPE") => Enums.MaterialFamily.PetCopolyester,
            _ when upper.Contains("ABS") => Enums.MaterialFamily.Styrenics,
            _ when upper.Contains("ASA") => Enums.MaterialFamily.Styrenics,
            _ when upper.Contains("HIPS") => Enums.MaterialFamily.Styrenics,
            _ when upper.Contains("PC-ABS") || upper.Contains("PC/ABS") => Enums.MaterialFamily.Styrenics,
            _ when upper.Contains("PC") && !upper.Contains("ABS") => Enums.MaterialFamily.Polycarbonate,
            _ when upper.Contains("NYLON") || upper.Contains("PA") => Enums.MaterialFamily.Polyamide,
            _ when upper.Contains("TPU") || upper.Contains("TPE") || upper.Contains("TPC") => Enums.MaterialFamily.FlexibleTpe,
            _ when upper.Contains("PP") && upper.Length <= 4 => Enums.MaterialFamily.Polypropylene,
            _ when upper.Contains("PMMA") => Enums.MaterialFamily.Acrylic,
            _ when upper.Contains("POM") => Enums.MaterialFamily.Acetal,
            _ when upper.Contains("PEEK") || upper.Contains("PEKK") => Enums.MaterialFamily.Paek,
            _ when upper.Contains("PEI") || upper.Contains("ULTEM") => Enums.MaterialFamily.Pei,
            _ when upper.Contains("PPS") || upper.Contains("PPSU") => Enums.MaterialFamily.Sulfone,
            _ when upper.Contains("PVA") || upper.Contains("BVOH") => Enums.MaterialFamily.WaterSoluble,
            _ => Enums.MaterialFamily.Unknown
        };
    }

    private static Enums.AdditiveMaterial ParseAdditives(string material, string[]? moreDetails)
    {
        var additives = Enums.AdditiveMaterial.None;
        var text = $"{material} {string.Join(" ", moreDetails ?? [])}".ToUpperInvariant();

        if (text.Contains("CARBON") || text.Contains("CF"))
        {
            additives |= Enums.AdditiveMaterial.CarbonFiber;
        }

        if (text.Contains("GLASS") || text.Contains("GF"))
        {
            additives |= Enums.AdditiveMaterial.GlassFiber;
        }

        if (text.Contains("WOOD"))
        {
            additives |= Enums.AdditiveMaterial.Wood;
        }

        if (text.Contains("METAL") || text.Contains("STEEL") || text.Contains("COPPER") || text.Contains("BRONZE"))
        {
            additives |= Enums.AdditiveMaterial.MetalFilled;
        }

        if (text.Contains("GLOW") || text.Contains("PHOSPHOR"))
        {
            additives |= Enums.AdditiveMaterial.Phosphorescent;
        }

        if (text.Contains("CONDUCTIVE"))
        {
            additives |= Enums.AdditiveMaterial.Conductive;
        }

        if (text.Contains("ESD"))
        {
            additives |= Enums.AdditiveMaterial.EsdSafe;
        }

        if (text.Contains("GLITTER") || text.Contains("SPARKLE"))
        {
            additives |= Enums.AdditiveMaterial.Glitter;
        }

        if (text.Contains("CERAMIC"))
        {
            additives |= Enums.AdditiveMaterial.Ceramic;
        }

        if (text.Contains("STONE") || text.Contains("MARBLE"))
        {
            additives |= Enums.AdditiveMaterial.Stone;
        }

        return additives;
    }

    private static Enums.Opacity ParseOpacity(string colorName, string[]? moreDetails)
    {
        var text = $"{colorName} {string.Join(" ", moreDetails ?? [])}".ToUpperInvariant();

        if (text.Contains("TRANSPARENT") || text.Contains("CLEAR"))
        {
            return Enums.Opacity.Transparent;
        }

        if (text.Contains("TRANSLUCENT"))
        {
            return Enums.Opacity.Translucent;
        }

        if (text.Contains("OPAQUE"))
        {
            return Enums.Opacity.Opaque;
        }

        return Enums.Opacity.Unknown;
    }

    private static Enums.Finish ParseFinish(string colorName, string[]? moreDetails)
    {
        var text = $"{colorName} {string.Join(" ", moreDetails ?? [])}".ToUpperInvariant();

        if (text.Contains("SILK") || text.Contains("SATIN"))
        {
            return Enums.Finish.Silk;
        }

        if (text.Contains("MATTE") || text.Contains("MAT"))
        {
            return Enums.Finish.Matte;
        }

        if (text.Contains("GLOSS") || text.Contains("SHINY"))
        {
            return Enums.Finish.Glossy;
        }

        if (text.Contains("SPARKLE") || text.Contains("GLITTER") || text.Contains("GALAXY"))
        {
            return Enums.Finish.Sparkle;
        }

        if (text.Contains("TEXTURE"))
        {
            return Enums.Finish.Textured;
        }

        return Enums.Finish.Unknown;
    }

    private static string BuildNotes(FilamentDbEntry entry)
    {
        var notes = new List<string>();

        if (!string.IsNullOrWhiteSpace(entry.ColorName))
        {
            notes.Add($"Color: {entry.ColorName}");
        }

        if (entry.IsColorBlended)
        {
            notes.Add("Blended/Multi-color");
        }

        if (entry.Provenance != "Unknown" && !string.IsNullOrWhiteSpace(entry.Provenance))
        {
            notes.Add($"Made in: {entry.Provenance}");
        }

        if (entry.FactoryInfo != "Unknown" && !string.IsNullOrWhiteSpace(entry.FactoryInfo))
        {
            notes.Add($"Factory: {entry.FactoryInfo}");
        }

        if (entry.MoreDetails is { Length: > 0 })
        {
            notes.AddRange(entry.MoreDetails);
        }

        return string.Join("; ", notes);
    }

    private static string? TryExtractDomain(string url)
    {
        if (string.IsNullOrWhiteSpace(url) || url == "Unknown")
        {
            return null;
        }

        try
        {
            var uri = new Uri(url);
            return $"{uri.Scheme}://{uri.Host}";
        }
        catch
        {
            return null;
        }
    }


    // Removed non-yield manufacturer retrieval methods in favor of streaming-only APIs

    // Stream manufacturers in batches, optionally with a search query
    public async IAsyncEnumerable<Manufacturer> StreamManufacturersAsync(
        string? query = null,
        int pageSize = 200,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await InitializeAsync();
        var offset = 0;
        while (true)
        {
            IReadOnlyList<Manufacturer> batch;
            if (string.IsNullOrWhiteSpace(query))
            {
                batch = await connection.QueryAsync<Manufacturer>(
                    "SELECT * FROM manufacturers ORDER BY Name LIMIT ? OFFSET ?",
                    pageSize, offset);
            }
            else
            {
                var like = BuildLikeQuery(query!);
                batch = await connection.QueryAsync<Manufacturer>(
                    "SELECT * FROM manufacturers WHERE Name LIKE ? OR Country LIKE ? OR Website LIKE ? OR Aliases LIKE ? ORDER BY Name LIMIT ? OFFSET ?",
                    like, like, like, like, pageSize, offset);
            }

            if (batch.Count == 0)
            {
                yield break;
            }

            foreach (var item in batch)
            {
                cancellationToken.ThrowIfCancellationRequested();
                yield return item;
            }

            offset += batch.Count;
            if (batch.Count < pageSize)
            {
                yield break;
            }
        }
    }

    public async Task<int> SaveManufacturerAsync(Manufacturer manufacturer)
    {
        await InitializeAsync();
        return await connection.InsertOrReplaceAsync(manufacturer);
    }

    // Removed non-yield materials retrieval methods in favor of streaming-only APIs

    // Stream materials in batches, optionally with a search query
    public async IAsyncEnumerable<Material> StreamMaterialsAsync(
        string? query = null,
        int pageSize = 200,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await InitializeAsync();
        var offset = 0;
        while (true)
        {
            IReadOnlyList<Material> batch;
            if (string.IsNullOrWhiteSpace(query))
            {
                batch = await connection.QueryAsync<Material>(
                    "SELECT * FROM materials ORDER BY Manufacturer, Name LIMIT ? OFFSET ?",
                    pageSize, offset);
            }
            else
            {
                var like = BuildLikeQuery(query!);
                batch = await connection.QueryAsync<Material>(
                    "SELECT * FROM materials WHERE Name LIKE ? OR Manufacturer LIKE ? OR Color LIKE ? OR Notes LIKE ? ORDER BY Manufacturer, Name LIMIT ? OFFSET ?",
                    like, like, like, like, pageSize, offset);
            }

            if (batch.Count == 0)
            {
                yield break;
            }

            foreach (var item in batch)
            {
                cancellationToken.ThrowIfCancellationRequested();
                yield return item;
            }

            offset += batch.Count;
            if (batch.Count < pageSize)
            {
                yield break;
            }
        }
    }

    // Removed MaterialsSummary; consumers should derive summaries from streamed data if needed

    private static string BuildLikeQuery(string query)
    {
        return $"%{query}%";
    }

    // Helper DTO for COUNT(*) queries. Must have a parameterless constructor for SQLite mapper
    private sealed class CountResult
    {
        public int Count { get; set; }
    }

    public async Task<int> SaveMaterialAsync(Material material)
    {
        await InitializeAsync();
        return await connection.InsertOrReplaceAsync(material);
    }

    public async Task<IReadOnlyList<Carrier>> GetCarriersAsync()
    {
        await InitializeAsync();
        return await connection.Table<Carrier>().ToListAsync();
    }

    // Removed non-yield carrier retrieval methods in favor of streaming-only APIs

    // Stream carriers in batches, optionally with a search query
    public async IAsyncEnumerable<Carrier> StreamCarriersAsync(
        string? query = null,
        int pageSize = 200,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await InitializeAsync();
        var offset = 0;
        while (true)
        {
            IReadOnlyList<Carrier> batch;
            if (string.IsNullOrWhiteSpace(query))
            {
                batch = await connection.QueryAsync<Carrier>(
                    "SELECT * FROM carriers ORDER BY Manufacturer, SpoolType LIMIT ? OFFSET ?",
                    pageSize, offset);
            }
            else
            {
                var like = BuildLikeQuery(query!);
                batch = await connection.QueryAsync<Carrier>(
                    "SELECT * FROM carriers WHERE Manufacturer LIKE ? ORDER BY Manufacturer, SpoolType LIMIT ? OFFSET ?",
                    like, pageSize, offset);
            }

            if (batch.Count == 0)
            {
                yield break;
            }

            foreach (var item in batch)
            {
                cancellationToken.ThrowIfCancellationRequested();
                yield return item;
            }

            offset += batch.Count;
            if (batch.Count < pageSize)
            {
                yield break;
            }
        }
    }

    public async Task<int> SaveCarrierAsync(Carrier carrier)
    {
        await InitializeAsync();
        return await connection.InsertOrReplaceAsync(carrier);
    }

    // Streaming API for spools with optional filters; yields hydrated spools incrementally
    public async IAsyncEnumerable<Spool> StreamSpoolsAsync(
        Guid? materialId = null,
        Guid? carrierId = null,
        long? barcode = null,
        string? query = null,
        int pageSize = 200,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await InitializeAsync();

        var whereClauses = new List<string>();
        var parameters = new List<object>();

        if (materialId is { } mid)
        {
            whereClauses.Add("MaterialId = ?");
            parameters.Add(mid);
        }
        if (carrierId is { } cid)
        {
            whereClauses.Add("CarrierId = ?");
            parameters.Add(cid);
        }
        if (barcode is { } bc)
        {
            whereClauses.Add("Barcode = ?");
            parameters.Add(bc);
        }
        if (!string.IsNullOrWhiteSpace(query))
        {
            // Simple text filter over MaterialId/CarrierId is not helpful; keep for future columns
            // For now, ignore generic query unless specific columns exist
        }

        var whereSql = whereClauses.Count > 0 ? (" WHERE " + string.Join(" AND ", whereClauses)) : string.Empty;

        var offset = 0;
        while (true)
        {
            var sql = $"SELECT * FROM spools{whereSql} ORDER BY LastUpdatedAt DESC, Id LIMIT ? OFFSET ?";
            var batchParams = new List<object>(parameters) { pageSize, offset };
            var spools = await connection.QueryAsync<Spool>(sql, batchParams.ToArray());

            if (spools.Count == 0)
            {
                yield break;
            }

            // Hydrate related entities per batch
            var materialIds = spools.Select(s => s.MaterialId).Distinct().ToList();
            var carrierIds = spools.Select(s => s.CarrierId).Distinct().ToList();

            Dictionary<Guid, Material> materialLookup = new();
            Dictionary<Guid, Carrier> carrierLookup = new();

            if (materialIds.Count > 0)
            {
                var matPlaceholders = string.Join(",", Enumerable.Repeat("?", materialIds.Count));
                var mats = await connection.QueryAsync<Material>($"SELECT * FROM materials WHERE Id IN ({matPlaceholders})", materialIds.Cast<object>().ToArray());
                materialLookup = mats.ToDictionary(m => m.Id);
            }

            if (carrierIds.Count > 0)
            {
                var carPlaceholders = string.Join(",", Enumerable.Repeat("?", carrierIds.Count));
                var cars = await connection.QueryAsync<Carrier>($"SELECT * FROM carriers WHERE Id IN ({carPlaceholders})", carrierIds.Cast<object>().ToArray());
                carrierLookup = cars.ToDictionary(c => c.Id);
            }

            foreach (var spool in spools)
            {
                cancellationToken.ThrowIfCancellationRequested();
                spool.Material = materialLookup.TryGetValue(spool.MaterialId, out var m)
                    ? m
                    : new Material { Id = spool.MaterialId };
                spool.Carrier = carrierLookup.TryGetValue(spool.CarrierId, out var c)
                    ? c
                    : new Carrier { Id = spool.CarrierId };
                yield return spool;
            }

            offset += spools.Count;
            if (spools.Count < pageSize)
            {
                yield break;
            }
        }
    }

    public async Task<int> SaveSpoolAsync(Spool spool)
    {
        await InitializeAsync();

        if (spool.Material is not null)
        {
            await SaveMaterialAsync(spool.Material);
            spool.MaterialId = spool.Material.Id;
        }

        if (spool.Carrier is not null)
        {
            await SaveCarrierAsync(spool.Carrier);
            spool.CarrierId = spool.Carrier.Id;
        }

        spool.LastUpdatedAt = timeProvider.GetUtcNow();
        return await connection.InsertOrReplaceAsync(spool);
    }

    public async Task<int> DeleteSpoolAsync(Spool spool)
    {
        await InitializeAsync();
        return await connection.DeleteAsync(spool);
    }
}
