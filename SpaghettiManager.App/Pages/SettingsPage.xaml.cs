using SpaghettiManager.App.ViewModels;
using UraniumUI.Pages;
using SpaghettiManager.App.Infrastructure;

namespace SpaghettiManager.App.Pages;

public partial class SettingsPage : UraniumContentPage
{
    public SettingsPage(): this(ServiceHelper.GetRequiredService<SettingsViewModel>()) {}

    public SettingsPage(SettingsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
