using Microsoft.EntityFrameworkCore;
using SpaghettiManager.Model.Records;

namespace SpaghettiManager.App.Services;

public class InventoryDbContext : DbContext
{
    private readonly IPlatform _platform;

    public InventoryDbContext(IPlatform platform)
    {
        this._platform = platform;
    }

    public DbSet<Item> InventoryItems => Set<Item>();
    public DbSet<CatalogItem> CatalogEntries => Set<CatalogItem>();
    public DbSet<Manufacturer> Manufacturers => Set<Manufacturer>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var dbPath = Path.Combine(_platform.AppData.FullName, "spaghetti.db");
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
                    // Ignore unsupported System.Drawing.Color? property on FilamentLot
                    lot.Ignore(l => l.ColorApprox);
                });

                winding.OwnsOne(value => value.Carrier, carrier =>
                {
                    // Configure owned Spool and ignore unsupported Color? property
                    carrier.OwnsOne(value => value.Spool, spool =>
                    {
                        spool.Ignore(s => s.SpoolColor);
                    });
                });
            });

        modelBuilder.Entity<CatalogItem>()
            .HasKey(entry => entry.Barcode);

        modelBuilder.Entity<CatalogItem>()
            .OwnsOne(entry => entry.TemplateLot, lot =>
            {
                lot.OwnsOne(value => value.Material);
                // Ignore unsupported System.Drawing.Color? property on FilamentLot
                lot.Ignore(l => l.ColorApprox);
            });

        modelBuilder.Entity<CatalogItem>()
            .OwnsOne(entry => entry.DefaultCarrier, carrier =>
            {
                // Configure owned Spool and ignore unsupported Color? property
                carrier.OwnsOne(value => value.Spool, spool =>
                {
                    spool.Ignore(s => s.SpoolColor);
                });
            });

        modelBuilder.Entity<Manufacturer>()
            .HasKey(manufacturer => manufacturer.Id);

        base.OnModelCreating(modelBuilder);
    }
}
