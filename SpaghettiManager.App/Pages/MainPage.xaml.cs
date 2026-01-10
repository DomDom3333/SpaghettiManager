using BarcodeScanning;
using SpaghettiManager.App.ViewModels;
using UraniumUI.Pages;

namespace SpaghettiManager.App.Pages;

public partial class MainPage : UraniumContentPage
{
    public MainPage(MainPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        // Delegate lifecycle to ViewModel command to keep MVVM separation
        if (BindingContext is MainPageViewModel vm)
        {
            await vm.AppearingAsync();
        }
        base.OnAppearing();
    }

    protected override void OnDisappearing()
    {
        if (BindingContext is MainPageViewModel vm)
        {
            vm.Disappearing();
        }
        base.OnDisappearing();
    }

    private void CameraView_OnDetectionFinished(object sender, OnDetectionFinishedEventArg e)
    {
        if (BindingContext is not MainPageViewModel vm) return;

        // Forward results to ViewModel; UI thread marshal handled by underlying binding updates
        var results = e.BarcodeResults;
        if (results is { Count: > 0 })
        {
            vm.DetectionFinished(results);
        }
    }
}