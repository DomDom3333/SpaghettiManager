using SpaghettiManager.App.Pages;

namespace SpaghettiManager.App;

public partial class AppShell : Shell
{
    public AppShell()
    {
        this.InitializeComponent();
        Routing.RegisterRoute("main", typeof(MainPage));
        Routing.RegisterRoute("inventory", typeof(InventoryListPage));
        Routing.RegisterRoute("catalog", typeof(CatalogHomePage));
        Routing.RegisterRoute("scan", typeof(ScanPage));
        Routing.RegisterRoute("settings", typeof(SettingsPage));
    }
}