using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;

namespace SpaghettiManager.App.ViewModels;

public partial class InventoryDetailViewModel : ObservableObject, IQueryAttributable
{
    [ObservableProperty]
    private string itemId = string.Empty;

    [ObservableProperty]
    private string manufacturer = string.Empty;

    [ObservableProperty]
    private string material = string.Empty;

    [ObservableProperty]
    private string colorName = string.Empty;

    [ObservableProperty]
    private string status = string.Empty;

    [ObservableProperty]
    private string remainingGrams = string.Empty;

    [ObservableProperty]
    private string spoolCarrier = string.Empty;

    [ObservableProperty]
    private string openedDate = string.Empty;

    [ObservableProperty]
    private string lastWeighedDate = string.Empty;

    [ObservableProperty]
    private string lastDriedDate = string.Empty;

    [ObservableProperty]
    private string notes = string.Empty;

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("itemId", out var idValue))
        {
            ItemId = idValue?.ToString() ?? string.Empty;
        }

        LoadSample();
    }

    [RelayCommand]
    private Task RespoolAsync()
    {
        return Shell.Current.GoToAsync("///inventory/action?mode=respool");
    }

    [RelayCommand]
    private Task WeighNowAsync()
    {
        return Shell.Current.GoToAsync("///inventory/action?mode=weigh");
    }

    [RelayCommand]
    private Task MarkEmptyAsync()
    {
        return Task.CompletedTask;
    }

    private void LoadSample()
    {
        Manufacturer = "Prusament";
        Material = "PLA";
        ColorName = "Galaxy Black";
        Status = "In use";
        RemainingGrams = "320";
        SpoolCarrier = "Prusa spool (plastic)";
        OpenedDate = "2024-02-01";
        LastWeighedDate = "2024-02-12";
        LastDriedDate = "2024-01-15";
        Notes = "Keep in dry box after use.";
    }
}
