using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using SpaghettiManager.App.Services;
using SpaghettiManager.Model;
using SpaghettiManager.Model.Records;

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

    private readonly InventoryDataService inventoryData;

    public InventoryDetailViewModel(InventoryDataService inventoryData)
    {
        this.inventoryData = inventoryData;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("itemId", out var idValue))
        {
            ItemId = idValue?.ToString() ?? string.Empty;
        }

        _ = LoadItemAsync(ItemId);
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
    private async Task MarkEmptyAsync()
    {
        await inventoryData.MarkEmptyAsync(ItemId);
        await LoadItemAsync(ItemId);
    }

    private async Task LoadItemAsync(string? id)
    {
        var item = await inventoryData.GetItemByIdAsync(id);
        if (item is null)
        {
            Manufacturer = "Unknown";
            Material = "Unknown";
            ColorName = "";
            Status = "Unknown";
            RemainingGrams = "Unknown";
            SpoolCarrier = "";
            OpenedDate = "";
            LastWeighedDate = "";
            LastDriedDate = "";
            Notes = "";
            return;
        }

        Manufacturer = InventoryFormatting.GetManufacturer(item);
        Material = InventoryFormatting.GetMaterialName(item);
        ColorName = InventoryFormatting.GetColorName(item);
        Status = GetStatusLabel(InventoryFormatting.GetStatus(item));
        RemainingGrams = InventoryFormatting.GetRemainingGrams(item)?.ToString() ?? "Unknown";
        SpoolCarrier = InventoryFormatting.GetCarrierLabel(item);
        OpenedDate = FormatDate(InventoryFormatting.GetOpenedDate(item));
        LastWeighedDate = FormatDate(InventoryFormatting.GetLastMeasuredAt(item));
        LastDriedDate = FormatDate(InventoryFormatting.GetLastDriedAt(item));
        Notes = item.Overrides.Notes ?? item.Winding.Notes ?? string.Empty;
    }

    private static string FormatDate(DateTime? date)
    {
        return date?.ToString("yyyy-MM-dd") ?? "";
    }

    private static string GetStatusLabel(Enums.InventoryStatus status)
    {
        return status switch
        {
            Enums.InventoryStatus.InUse => "In use",
            Enums.InventoryStatus.Sealed => "Sealed",
            Enums.InventoryStatus.Opened => "Opened",
            Enums.InventoryStatus.Empty => "Empty",
            Enums.InventoryStatus.Discarded => "Discarded",
            _ => "Unknown"
        };
    }
}
