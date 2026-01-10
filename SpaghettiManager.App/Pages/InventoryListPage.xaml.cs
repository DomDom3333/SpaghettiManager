using SpaghettiManager.App.ViewModels;
using UraniumUI.Pages;

namespace SpaghettiManager.App.Pages;

public partial class InventoryListPage : UraniumContentPage
{
    public InventoryListPage(InventoryListViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
