using SpaghettiManager.App.ViewModels;
using UraniumUI.Pages;
using SpaghettiManager.App.Infrastructure;

namespace SpaghettiManager.App.Pages;

public partial class InventoryListPage : UraniumContentPage
{
    public InventoryListPage(): this(ServiceHelper.GetRequiredService<InventoryListViewModel>()) {}

    public InventoryListPage(InventoryListViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
