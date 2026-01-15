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

    public virtual Task SeedAsync()
        => Task.CompletedTask;

    public async Task<IReadOnlyList<Manufacturer>> GetManufacturersAsync()
    {
        await InitializeAsync();
        return await connection.Table<Manufacturer>().ToListAsync();
    }

    public async Task<int> SaveManufacturerAsync(Manufacturer manufacturer)
    {
        await InitializeAsync();
        return await connection.InsertOrReplaceAsync(manufacturer);
    }

    public async Task<IReadOnlyList<Material>> GetMaterialsAsync()
    {
        await InitializeAsync();
        return await connection.Table<Material>().ToListAsync();
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

    public async Task<int> SaveCarrierAsync(Carrier carrier)
    {
        await InitializeAsync();
        return await connection.InsertOrReplaceAsync(carrier);
    }

    public async Task<IReadOnlyList<Spool>> GetSpoolsAsync()
    {
        await InitializeAsync();

        var spools = await connection.Table<Spool>().ToListAsync();
        if (spools.Count == 0)
        {
            return spools;
        }

        var materials = await connection.Table<Material>().ToListAsync();
        var carriers = await connection.Table<Carrier>().ToListAsync();

        var materialLookup = materials.ToDictionary(item => item.Id);
        var carrierLookup = carriers.ToDictionary(item => item.Id);

        foreach (var spool in spools)
        {
            if (materialLookup.TryGetValue(spool.MaterialId, out var material))
            {
                spool.Material = material;
            }
            else
            {
                spool.Material = new Material { Id = spool.MaterialId };
            }

            if (carrierLookup.TryGetValue(spool.CarrierId, out var carrier))
            {
                spool.Carrier = carrier;
            }
            else
            {
                spool.Carrier = new Carrier { Id = spool.CarrierId };
            }
        }

        return spools;
    }

    public async Task<Spool?> GetSpoolAsync(Guid spoolId)
    {
        await InitializeAsync();

        var spool = await connection.FindAsync<Spool>(spoolId);
        if (spool is null)
        {
            return null;
        }

        spool.Material = await connection.FindAsync<Material>(spool.MaterialId)
            ?? new Material { Id = spool.MaterialId };
        spool.Carrier = await connection.FindAsync<Carrier>(spool.CarrierId)
            ?? new Carrier { Id = spool.CarrierId };

        return spool;
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
