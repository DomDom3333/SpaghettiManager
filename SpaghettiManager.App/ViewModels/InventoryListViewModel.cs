using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using SpaghettiManager.App.Services;
using SpaghettiManager.Model;

namespace SpaghettiManager.App.ViewModels;

public partial class InventoryListViewModel : ObservableObject, IQueryAttributable
{
    public class InventoryItemSummary
    {
        public string Id { get; set; } = string.Empty;
        public string Manufacturer { get; set; } = string.Empty;
        public string Material { get; set; } = string.Empty;
        public string ColorName { get; set; } = string.Empty;
        public Color SwatchColor { get; set; } = Colors.Gray;
        public string RemainingDisplay { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public bool IsAmsCompatible { get; set; }
    }

    public class FilterChip
    {
        public string Label { get; set; } = string.Empty;
        public string QueryValue { get; set; } = string.Empty;
    }

    public ObservableCollection<InventoryItemSummary> Items { get; } = new();

    public ObservableCollection<FilterChip> Filters { get; } = new();

    [ObservableProperty]
    private string activeFilter = "All";

    [ObservableProperty]
    private string filterDescription = "All inventory";

    private readonly InventoryDataService inventoryData;

    public InventoryListViewModel(InventoryDataService inventoryData)
    {
        this.inventoryData = inventoryData;
        Filters.Add(new FilterChip { Label = "All", QueryValue = "all" });
        Filters.Add(new FilterChip { Label = "Low", QueryValue = "low" });
        Filters.Add(new FilterChip { Label = "Unknown", QueryValue = "unknown" });
        Filters.Add(new FilterChip { Label = "In use", QueryValue = "in-use" });
        Filters.Add(new FilterChip { Label = "Recent", QueryValue = "recent" });

        _ = RefreshItemsAsync();
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("filter", out var filterValue))
        {
            SetFilter(filterValue?.ToString());
        }
    }

    [RelayCommand]
    private void ApplyFilter(FilterChip chip)
    {
        SetFilter(chip?.QueryValue);
    }

    [RelayCommand]
    private Task OpenDetailAsync(InventoryItemSummary item)
    {
        if (item == null)
        {
            return Task.CompletedTask;
        }

        return Shell.Current.GoToAsync($"///inventory/detail?itemId={item.Id}");
    }

    [RelayCommand]
    private async Task MarkEmptyAsync(InventoryItemSummary item)
    {
        if (item == null)
        {
            return;
        }

        await inventoryData.MarkEmptyAsync(item.Id);
        await RefreshItemsAsync();
    }

    [RelayCommand]
    private Task UpdateRemainingAsync(InventoryItemSummary item)
    {
        return Shell.Current.GoToAsync("///inventory/action?mode=weigh");
    }

    [RelayCommand]
    private Task ChangeSpoolAsync(InventoryItemSummary item)
    {
        return Shell.Current.GoToAsync("///inventory/action?mode=respool");
    }

    [RelayCommand]
    private Task AddManualAsync()
    {
        return Shell.Current.GoToAsync("///inventory/action?mode=add");
    }

    [RelayCommand]
    private Task ScanBarcodeAsync()
    {
        return Shell.Current.GoToAsync("///scan");
    }

    private void SetFilter(string? filter)
    {
        var selected = filter ?? "all";
        ActiveFilter = selected switch
        {
            "low" => "Low",
            "unknown" => "Unknown",
            "in-use" => "In use",
            "recent" => "Recent",
            "material" => "Material",
            "manufacturer" => "Manufacturer",
            "status" => "Status",
            "favorite" => "Favorite",
            "dry" => "Needs drying",
            _ => "All"
        };

        FilterDescription = selected switch
        {
            "low" => "Low remaining spools",
            "unknown" => "Unknown remaining",
            "in-use" => "Currently in use",
            "recent" => "Recently added",
            "material" => "Grouped by material",
            "manufacturer" => "Grouped by manufacturer",
            "status" => "Grouped by status",
            "favorite" => "Favorite spools",
            "dry" => "Needs drying",
            _ => "All inventory"
        };

        _ = RefreshItemsAsync();
    }

    private async Task RefreshItemsAsync()
    {
        Items.Clear();
        var items = await GetFilteredItemsAsync();
        foreach (var item in items)
        {
            Items.Add(CreateSummary(item));
        }
    }

    private async Task<IEnumerable<InventoryItemDto>> GetFilteredItemsAsync()
    {
        IReadOnlyList<InventoryItemDto> items = await inventoryData.GetItemsAsync();
        return ActiveFilter switch
        {
            "Low" => items.Where(item => item.RemainingGrams is > 0 and < 200),
            "Unknown" => items.Where(item => !item.RemainingGrams.HasValue),
            "In use" => items.Where(item => item.Status == Enums.InventoryStatus.InUse),
            "Recent" => items.Where(item => item.CreatedAt >= DateTime.Today.AddDays(-14)),
            "Needs drying" => items.Where(item =>
                item.Hygroscopicity >= Enums.Hygroscopicity.Medium
                && item.LastDriedAt is not null
                && item.LastDriedAt < DateTime.Today.AddDays(-30)),
            _ => items
        };
    }

    private static InventoryItemSummary CreateSummary(InventoryItemDto item)
    {
        var remaining = item.RemainingGrams;
        var color = ToMauiColor(item.ColorHex, Colors.Gray);

        return new InventoryItemSummary
        {
            Id = item.Id,
            Manufacturer = item.Manufacturer,
            Material = item.MaterialName,
            ColorName = item.ColorName,
            SwatchColor = color,
            RemainingDisplay = remaining.HasValue ? $"{remaining.Value} g" : "Unknown",
            Status = GetStatusLabel(item.Status),
            IsAmsCompatible = IsAmsCompatible(item)
        };
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

    private static Color ToMauiColor(string? colorHex, Color fallback)
    {
        return string.IsNullOrWhiteSpace(colorHex) ? fallback : Color.FromArgb(colorHex);
    }

    private static bool IsAmsCompatible(InventoryItemDto item)
    {
        var notes = item.CarrierNotes ?? string.Empty;
        return notes.Contains("AMS", StringComparison.OrdinalIgnoreCase);
    }
}
