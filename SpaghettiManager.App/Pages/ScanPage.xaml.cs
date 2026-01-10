using SpaghettiManager.App.ViewModels;
using UraniumUI.Pages;
using SpaghettiManager.App.Infrastructure;

namespace SpaghettiManager.App.Pages;

public partial class ScanPage : UraniumContentPage
{
    public ScanPage(): this(ServiceHelper.GetRequiredService<ScanPageViewModel>()) {}

    public ScanPage(ScanPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
