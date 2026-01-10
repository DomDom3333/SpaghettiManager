using SpaghettiManager.App.ViewModels;
using UraniumUI.Pages;
using SpaghettiManager.App.Infrastructure;

namespace SpaghettiManager.App.Pages;

public partial class CatalogHomePage : UraniumContentPage
{
    public CatalogHomePage(): this(ServiceHelper.GetRequiredService<CatalogHomeViewModel>()) {}

    public CatalogHomePage(CatalogHomeViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
