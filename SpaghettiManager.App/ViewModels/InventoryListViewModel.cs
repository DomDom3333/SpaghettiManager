using SpaghettiManager.Model;
using SpaghettiManager.Model.Records;

namespace SpaghettiManager.App.ViewModels;

public partial class InventoryListViewModel : ObservableObject, IQueryAttributable
{
    public class FilterChip
    {
        public string Label { get; set; } = string.Empty;
        public string QueryValue { get; set; } = string.Empty;
    }

    public ObservableCollection<Spool> Items { get; } = new();

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
    private Task OpenDetailAsync(Spool item)
    {
        if (item == null)
        {
            return Task.CompletedTask;
        }

        return Shell.Current.GoToAsync($"///inventory/detail?itemId={item.Id}");
    }

    [RelayCommand]
    private Task MarkEmptyAsync(Spool item)
    {
        return Task.CompletedTask;
    }

    [RelayCommand]
    private Task UpdateRemainingAsync(Spool item)
    {
        return Shell.Current.GoToAsync("///inventory/action?mode=weigh");
    }

    [RelayCommand]
    private Task ChangeSpoolAsync(Spool item)
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
        Items.Add(new Spool
        {
            Manufacturer = "Prusament",
            Barcode = 1234567890,
            BarcodeType = Enums.BarcodeType.Ean,
            Material = new Material
            {
                Name = "PLA",
                Color = "#1C1C1C",
                Family = Enums.MaterialFamily.Pla,
                Finish = Enums.Finish.Glossy,
                Opacity = Enums.Opacity.Opaque,
                Manufacturer = "Prusament",
                DiameterMm = 1.75m
            },
            Carrier = new Carrier
            {
                Manufacturer = "Prusa",
                SpoolType = Enums.SpoolType.GenericPlastic,
                EmptyWeightGrams = 245,
                SpoolRadius = 100,
                SpoolHubRadius = 30,
                SpoolHeight = 70,
                HighTemp = false
            }
        });
        Items.Add(new Spool
        {
            Manufacturer = "Overture",
            Barcode = 987654321,
            BarcodeType = Enums.BarcodeType.Ean,
            Material = new Material
            {
                Name = "PETG",
                Color = "#C62828",
                Family = Enums.MaterialFamily.PetCopolyester,
                Finish = Enums.Finish.Silk,
                Opacity = Enums.Opacity.Opaque,
                Manufacturer = "Overture",
                DiameterMm = 1.75m
            },
            Carrier = new Carrier
            {
                Manufacturer = "Overture",
                SpoolType = Enums.SpoolType.Cardboard,
                EmptyWeightGrams = 180,
                SpoolRadius = 105,
                SpoolHubRadius = 32,
                SpoolHeight = 68,
                HighTemp = false
            }
        });
        Items.Add(new Spool
        {
            Manufacturer = "Polymaker",
            Barcode = 112233445,
            BarcodeType = Enums.BarcodeType.Ean,
            Material = new Material
            {
                Name = "TPU",
                Color = "#E0E0E0",
                Family = Enums.MaterialFamily.FlexibleTpe,
                Finish = Enums.Finish.Matte,
                Opacity = Enums.Opacity.Translucent,
                Manufacturer = "Polymaker",
                DiameterMm = 1.75m
            },
            Carrier = new Carrier
            {
                Manufacturer = "Polymaker",
                SpoolType = Enums.SpoolType.Reusable,
                EmptyWeightGrams = 210,
                SpoolRadius = 100,
                SpoolHubRadius = 30,
                SpoolHeight = 70,
                HighTemp = true
            }
        });
    }
}
