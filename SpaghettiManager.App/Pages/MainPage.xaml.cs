using SpaghettiManager.App.ViewModels;
using UraniumUI.Pages;
using SpaghettiManager.App.Infrastructure;

namespace SpaghettiManager.App.Pages;

public partial class MainPage : UraniumContentPage
{
    public MainPage(): this(ServiceHelper.GetRequiredService<HomePageViewModel>()) {}

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
