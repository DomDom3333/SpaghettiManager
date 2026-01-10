using SpaghettiManager.App.ViewModels;
using UraniumUI.Pages;

namespace SpaghettiManager.App.Pages;

public partial class InventoryActionPage : UraniumContentPage
{
    public InventoryActionPage(InventoryActionViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
