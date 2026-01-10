using SpaghettiManager.App.ViewModels;
using UraniumUI.Pages;

namespace SpaghettiManager.App.Pages;

public partial class CatalogSectionPage : UraniumContentPage
{
    public CatalogSectionPage(CatalogSectionViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
