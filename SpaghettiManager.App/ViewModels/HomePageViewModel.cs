using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace SpaghettiManager.App.ViewModels;

public partial class HomePageViewModel : ObservableObject
{
    public class AlertItem
    {
        public string Title { get; set; } = string.Empty;
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

    public IRelayCommand ScanNewSpoolCommand { get; }
    public IRelayCommand AddManuallyCommand { get; }
    public IRelayCommand WeighFilamentCommand { get; }
    public IRelayCommand RespoolFilamentCommand { get; }
    public IRelayCommand EditEanMappingsCommand { get; }

    public HomePageViewModel()
    {
        ScanNewSpoolCommand = new AsyncRelayCommand(() => NavigateAsync("//Scan"));
        AddManuallyCommand = new AsyncRelayCommand(() => NavigateAsync("//Inventory/Add"));
        WeighFilamentCommand = new AsyncRelayCommand(() => NavigateAsync("//Inventory/Weigh"));
        RespoolFilamentCommand = new AsyncRelayCommand(() => NavigateAsync("//Inventory/Respool"));
        EditEanMappingsCommand = new AsyncRelayCommand(() => NavigateAsync("//Catalog/EanMappings"));
    }

    private static Task NavigateAsync(string route)
    {
        return Shell.Current.GoToAsync(route, true);
    }

    public Task AppearingAsync()
    {
        // TODO: load real data from services; sample placeholders for now
        TotalItems = 12;
        TotalRemainingGrams = 3560;
        UnknownRemainingCount = 3;
        ItemsInUseCount = 2;
        Alerts.Clear();
        Alerts.Add(new AlertItem { Title = "3 low remaining spools", TargetRoute = "//Inventory?filter=low" });
        Alerts.Add(new AlertItem { Title = "2 items need drying", TargetRoute = "//Inventory?filter=dry" });
        Alerts.Add(new AlertItem { Title = "1 unmapped barcode", TargetRoute = "//Catalog/EanMappings" });
        return Task.CompletedTask;
    }

    public Task DisappearingAsync()
    {
        return Task.CompletedTask;
    }
}
