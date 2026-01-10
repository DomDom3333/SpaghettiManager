using BarcodeScanning;
using SpaghettiManager.App.Infrastructure;
using SpaghettiManager.App.ViewModels;
using UraniumUI.Pages;

namespace SpaghettiManager.App.Pages;

public partial class ScanPage : UraniumContentPage
{
    public ScanPage() : this(ServiceHelper.GetRequiredService<ScanPageViewModel>()) {}

    public ScanPage(ScanPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await Methods.AskForRequiredPermissionAsync();

        if (BindingContext is ScanPageViewModel viewModel)
        {
            viewModel.SetScanning(true);
        }
    }

    protected override void OnDisappearing()
    {
        if (BindingContext is ScanPageViewModel viewModel)
        {
            viewModel.SetScanning(false);
        }

        base.OnDisappearing();
    }
}
