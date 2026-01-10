using SpaghettiManager.App.ViewModels;
using UraniumUI.Pages;

namespace SpaghettiManager.App.Pages;

public partial class ScanReviewPage : UraniumContentPage
{
    public ScanReviewPage(ScanReviewViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
