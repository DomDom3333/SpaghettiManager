using SpaghettiManager.App.ViewModels;
using UraniumUI.Pages;

namespace SpaghettiManager.App.Pages;

public partial class CatalogHomePage : UraniumContentPage
{
    public CatalogHomePage(CatalogHomeViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
