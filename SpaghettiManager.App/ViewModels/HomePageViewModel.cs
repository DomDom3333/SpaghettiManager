using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

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

    public HomePageViewModel()
    {
        ScanNewSpoolCommand = new AsyncRelayCommand(() => NavigateAsync("//scan"));
        AddManuallyCommand = new AsyncRelayCommand(() => NavigateAsync("//inventory/action?mode=add"));
        WeighFilamentCommand = new AsyncRelayCommand(() => NavigateAsync("//inventory/action?mode=weigh"));
        RespoolFilamentCommand = new AsyncRelayCommand(() => NavigateAsync("//inventory/action?mode=respool"));
        EditEanMappingsCommand = new AsyncRelayCommand(() => NavigateAsync("//catalog/ean-mappings"));
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

    public Task AppearingAsync()
    {
        TotalItems = 12;
        TotalRemainingGrams = 3560;
        UnknownRemainingCount = 3;
        ItemsInUseCount = 2;

        Alerts.Clear();
        Alerts.Add(new AlertItem
        {
            Title = "Low remaining filament",
            Description = "3 spools under 150 g",
            TargetRoute = "//inventory?filter=low"
        });
        Alerts.Add(new AlertItem
        {
            Title = "Unknown remaining",
            Description = "2 spools need weighing",
            TargetRoute = "//inventory?filter=unknown"
        });
        Alerts.Add(new AlertItem
        {
            Title = "Needs drying",
            Description = "1 hygroscopic spool overdue",
            TargetRoute = "//inventory?filter=dry"
        });
        Alerts.Add(new AlertItem
        {
            Title = "Unmapped barcode",
            Description = "1 scan needs catalog mapping",
            TargetRoute = "//catalog/ean-mappings"
        });

        Highlights.Clear();
        Highlights.Add(new HighlightItem
        {
            Title = "Recently added",
            Subtitle = "4 spools this week",
            TargetRoute = "//inventory?filter=recent"
        });
        Highlights.Add(new HighlightItem
        {
            Title = "In use",
            Subtitle = "2 active spools",
            TargetRoute = "//inventory?filter=in-use"
        });
        Highlights.Add(new HighlightItem
        {
            Title = "Favorites",
            Subtitle = "PLA essentials",
            TargetRoute = "//inventory?filter=favorite"
        });

        Stats.Clear();
        Stats.Add(new StatItem
        {
            Title = "By material",
            Value = "PLA 6 • PETG 4 • TPU 2",
            TargetRoute = "//inventory?filter=material"
        });
        Stats.Add(new StatItem
        {
            Title = "By manufacturer",
            Value = "Prusament 5 • Overture 4",
            TargetRoute = "//inventory?filter=manufacturer"
        });
        Stats.Add(new StatItem
        {
            Title = "By status",
            Value = "Sealed 5 • Opened 5 • In use 2",
            TargetRoute = "//inventory?filter=status"
        });

        return Task.CompletedTask;
    }

    public Task DisappearingAsync()
    {
        return Task.CompletedTask;
    }
}
