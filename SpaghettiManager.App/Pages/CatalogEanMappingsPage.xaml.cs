using SpaghettiManager.App.ViewModels;
using UraniumUI.Pages;

namespace SpaghettiManager.App.Pages;

public partial class CatalogEanMappingsPage : UraniumContentPage
{
    public CatalogEanMappingsPage(CatalogEanMappingsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
