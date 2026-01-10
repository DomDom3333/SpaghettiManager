using Microsoft.EntityFrameworkCore;
using SpaghettiManager.Model.Records;

namespace SpaghettiManager.App.Services;

public class InventoryDbContext : DbContext
{
    private readonly IPlatform platform;

    public InventoryDbContext(IPlatform platform)
    {
        this.platform = platform;
    }

    public DbSet<Item> InventoryItems => Set<Item>();
    public DbSet<CatalogItem> CatalogEntries => Set<CatalogItem>();
    public DbSet<Manufacturer> Manufacturers => Set<Manufacturer>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var dbPath = Path.Combine(platform.AppData.FullName, "spaghetti.db");
        optionsBuilder.UseSqlite($"Data Source={dbPath}");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Item>()
            .HasKey(item => item.Id);

        modelBuilder.Entity<Item>()
            .OwnsOne(item => item.Winding, winding =>
            {
                winding.OwnsOne(value => value.Lot, lot =>
                {
                    lot.OwnsOne(value => value.Material);
                });

                winding.OwnsOne(value => value.Carrier, carrier =>
                {
                    carrier.OwnsOne(value => value.Spool);
                });
            });

        modelBuilder.Entity<CatalogItem>()
            .HasKey(entry => entry.Barcode);

        modelBuilder.Entity<CatalogItem>()
            .OwnsOne(entry => entry.TemplateLot, lot =>
            {
                lot.OwnsOne(value => value.Material);
            });

        modelBuilder.Entity<CatalogItem>()
            .OwnsOne(entry => entry.DefaultCarrier, carrier =>
            {
                carrier.OwnsOne(value => value.Spool);
            });

        modelBuilder.Entity<Manufacturer>()
            .HasKey(manufacturer => manufacturer.Id);

        base.OnModelCreating(modelBuilder);
    }
}
