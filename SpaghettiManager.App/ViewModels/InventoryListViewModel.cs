using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

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

    public InventoryListViewModel()
    {
        Filters.Add(new FilterChip { Label = "All", QueryValue = "all" });
        Filters.Add(new FilterChip { Label = "Low", QueryValue = "low" });
        Filters.Add(new FilterChip { Label = "Unknown", QueryValue = "unknown" });
        Filters.Add(new FilterChip { Label = "In use", QueryValue = "in-use" });
        Filters.Add(new FilterChip { Label = "Recent", QueryValue = "recent" });

        RefreshItems();
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
    private Task MarkEmptyAsync(InventoryItemSummary item)
    {
        return Task.CompletedTask;
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
            _ => "All inventory"
        };

        RefreshItems();
    }

    private void RefreshItems()
    {
        Items.Clear();
        Items.Add(new InventoryItemSummary
        {
            Id = "INV-1001",
            Manufacturer = "Prusament",
            Material = "PLA",
            ColorName = "Galaxy Black",
            SwatchColor = Color.FromArgb("#1C1C1C"),
            RemainingDisplay = "320 g",
            Status = "In use",
            IsAmsCompatible = true
        });
        Items.Add(new InventoryItemSummary
        {
            Id = "INV-1002",
            Manufacturer = "Overture",
            Material = "PETG",
            ColorName = "Fire Engine Red",
            SwatchColor = Color.FromArgb("#C62828"),
            RemainingDisplay = "Unknown",
            Status = "Opened",
            IsAmsCompatible = false
        });
        Items.Add(new InventoryItemSummary
        {
            Id = "INV-1003",
            Manufacturer = "Polymaker",
            Material = "TPU",
            ColorName = "Clear",
            SwatchColor = Color.FromArgb("#E0E0E0"),
            RemainingDisplay = "150 g",
            Status = "Sealed",
            IsAmsCompatible = true
        });
    }
}
