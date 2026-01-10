using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SpaghettiManager.App.Services;
using SpaghettiManager.Model;

namespace SpaghettiManager.App.ViewModels;

public partial class HomePageViewModel : ObservableObject
{
    public class AlertItem
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? TargetRoute { get; set; }
    }

    public class HighlightItem
    {
        public string Title { get; set; } = string.Empty;
        public string Subtitle { get; set; } = string.Empty;
        public string? TargetRoute { get; set; }
    }

    public class StatItem
    {
        public string Title { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string? TargetRoute { get; set; }
    }

    [ObservableProperty]
    private int totalItems;

    [ObservableProperty]
    private double totalRemainingGrams;

    [ObservableProperty]
    private int unknownRemainingCount;

    [ObservableProperty]
    private int itemsInUseCount;

    public ObservableCollection<AlertItem> Alerts { get; } = new();

    public ObservableCollection<HighlightItem> Highlights { get; } = new();

    public ObservableCollection<StatItem> Stats { get; } = new();

    public IRelayCommand ScanNewSpoolCommand { get; }
    public IRelayCommand AddManuallyCommand { get; }
    public IRelayCommand WeighFilamentCommand { get; }
    public IRelayCommand RespoolFilamentCommand { get; }
    public IRelayCommand EditEanMappingsCommand { get; }

    private readonly InventoryDataService inventoryData;

    public HomePageViewModel(InventoryDataService inventoryData)
    {
        this.inventoryData = inventoryData;
        ScanNewSpoolCommand = new AsyncRelayCommand(() => NavigateAsync("///scan"));
        AddManuallyCommand = new AsyncRelayCommand(() => NavigateAsync("///inventory/action?mode=add"));
        WeighFilamentCommand = new AsyncRelayCommand(() => NavigateAsync("///inventory/action?mode=weigh"));
        RespoolFilamentCommand = new AsyncRelayCommand(() => NavigateAsync("///inventory/action?mode=respool"));
        EditEanMappingsCommand = new AsyncRelayCommand(() => NavigateAsync("///catalog/ean-mappings"));
    }

    [RelayCommand]
    private Task OpenAlertAsync(AlertItem alert)
    {
        return NavigateToItemAsync(alert?.TargetRoute);
    }

    [RelayCommand]
    private Task OpenHighlightAsync(HighlightItem highlight)
    {
        return NavigateToItemAsync(highlight?.TargetRoute);
    }

    [RelayCommand]
    private Task OpenStatAsync(StatItem stat)
    {
        return NavigateToItemAsync(stat?.TargetRoute);
    }

    private static Task NavigateAsync(string route)
    {
        return Shell.Current.GoToAsync(route, true);
    }

    private static Task NavigateToItemAsync(string? route)
    {
        return string.IsNullOrWhiteSpace(route)
            ? Task.CompletedTask
            : Shell.Current.GoToAsync(route, true);
    }

    public async Task AppearingAsync()
    {
        var items = await inventoryData.GetItemsAsync();
        var itemsWithRemaining = items
            .Where(item => item.RemainingGrams.HasValue)
            .ToList();
        var unknownRemaining = items.Count(item => !item.RemainingGrams.HasValue);

        TotalItems = items.Count;
        TotalRemainingGrams = itemsWithRemaining.Sum(item => item.RemainingGrams ?? 0);
        UnknownRemainingCount = unknownRemaining;
        ItemsInUseCount = items.Count(item => item.Status == Enums.InventoryStatus.InUse);

        var lowRemainingCount = items.Count(item =>
            item.RemainingGrams is > 0 and < 200);
        var needsDryingCount = items.Count(item =>
            item.Hygroscopicity >= Enums.Hygroscopicity.Medium
            && item.LastDriedAt is not null
            && item.LastDriedAt < DateTime.Today.AddDays(-30));

        Alerts.Clear();
        if (lowRemainingCount > 0)
        {
            Alerts.Add(new AlertItem
            {
                Title = "Low remaining filament",
                Description = $"{lowRemainingCount} spool(s) under 200 g",
                TargetRoute = "///inventory?filter=low"
            });
        }

        if (unknownRemaining > 0)
        {
            Alerts.Add(new AlertItem
            {
                Title = "Unknown remaining",
                Description = $"{unknownRemaining} spool(s) need weighing",
                TargetRoute = "///inventory?filter=unknown"
            });
        }

        if (needsDryingCount > 0)
        {
            Alerts.Add(new AlertItem
            {
                Title = "Needs drying",
                Description = $"{needsDryingCount} hygroscopic spool(s) overdue",
                TargetRoute = "///inventory?filter=dry"
            });
        }

        Highlights.Clear();
        var recentlyAddedCount = items.Count(item => item.CreatedAt >= DateTime.Today.AddDays(-14));
        var inUseCount = items.Count(item => item.Status == Enums.InventoryStatus.InUse);

        Highlights.Add(new HighlightItem
        {
            Title = "Recently added",
            Subtitle = $"{recentlyAddedCount} spool(s) in 2 weeks",
            TargetRoute = "///inventory?filter=recent"
        });
        Highlights.Add(new HighlightItem
        {
            Title = "In use",
            Subtitle = $"{inUseCount} active spool(s)",
            TargetRoute = "///inventory?filter=in-use"
        });
        Highlights.Add(new HighlightItem
        {
            Title = "Material focus",
            Subtitle = FormatGroup(items, item => item.MaterialName),
            TargetRoute = "///inventory?filter=material"
        });

        Stats.Clear();
        Stats.Add(new StatItem
        {
            Title = "By material",
            Value = FormatGroup(items, item => item.MaterialName),
            TargetRoute = "///inventory?filter=material"
        });
        Stats.Add(new StatItem
        {
            Title = "By manufacturer",
            Value = FormatGroup(items, item => item.Manufacturer),
            TargetRoute = "///inventory?filter=manufacturer"
        });
        Stats.Add(new StatItem
        {
            Title = "By status",
            Value = FormatGroup(items, item => GetStatusLabel(item.Status)),
            TargetRoute = "///inventory?filter=status"
        });
    }

    public Task DisappearingAsync()
    {
        return Task.CompletedTask;
    }

    private static string FormatGroup(IEnumerable<InventoryItemDto> items, Func<InventoryItemDto, string> selector)
    {
        var groups = items
            .GroupBy(item => string.IsNullOrWhiteSpace(selector(item)) ? "Unknown" : selector(item))
            .Select(group => new { group.Key, Count = group.Count() })
            .OrderByDescending(group => group.Count)
            .ThenBy(group => group.Key)
            .ToList();

        return groups.Count == 0
            ? "No data"
            : string.Join(" • ", groups.Select(group => $"{group.Key} {group.Count}"));
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
