using Microsoft.EntityFrameworkCore;
using SpaghettiManager.App.Services.Entities;

namespace SpaghettiManager.App.Services;

public class InventoryDbContext : DbContext
{
    private readonly IPlatform platform;

    public InventoryDbContext(IPlatform platform)
    {
        this.platform = platform;
    }

    public DbSet<InventoryItemEntity> InventoryItems => Set<InventoryItemEntity>();
    public DbSet<CatalogEntryEntity> CatalogEntries => Set<CatalogEntryEntity>();
    public DbSet<ManufacturerEntity> Manufacturers => Set<ManufacturerEntity>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var dbPath = Path.Combine(platform.AppData.FullName, "spaghetti.db");
        optionsBuilder.UseSqlite($"Data Source={dbPath}");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<InventoryItemEntity>()
            .HasKey(item => item.Id);

        modelBuilder.Entity<CatalogEntryEntity>()
            .HasKey(entry => entry.Barcode);

        modelBuilder.Entity<ManufacturerEntity>()
            .HasKey(manufacturer => manufacturer.Id);

        base.OnModelCreating(modelBuilder);
    }
}
