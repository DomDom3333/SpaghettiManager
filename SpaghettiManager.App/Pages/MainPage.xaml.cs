using SpaghettiManager.App.ViewModels;
using UraniumUI.Pages;

namespace SpaghettiManager.App.Pages;

public partial class MainPage : UraniumContentPage
{
    public MainPage(HomePageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        if (BindingContext is HomePageViewModel vm)
        {
            await vm.AppearingAsync();
        }
        base.OnAppearing();
    }

    protected override async void OnDisappearing()
    {
        if (BindingContext is HomePageViewModel vm)
        {
            await vm.DisappearingAsync();
        }
        base.OnDisappearing();
    }
}
