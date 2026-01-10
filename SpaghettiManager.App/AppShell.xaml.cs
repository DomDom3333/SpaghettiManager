using SpaghettiManager.App.Pages;

namespace SpaghettiManager.App;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute("inventory/detail", typeof(InventoryDetailPage));
        Routing.RegisterRoute("inventory/action", typeof(InventoryActionPage));
        Routing.RegisterRoute("scan/review", typeof(ScanReviewPage));
        Routing.RegisterRoute("catalog/section", typeof(CatalogSectionPage));
        Routing.RegisterRoute("catalog/ean-mappings", typeof(CatalogEanMappingsPage));
    }
}
