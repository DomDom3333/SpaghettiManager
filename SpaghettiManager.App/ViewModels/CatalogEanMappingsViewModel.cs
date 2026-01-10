using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace SpaghettiManager.App.ViewModels;

public partial class CatalogEanMappingsViewModel : ObservableObject
{
    public class EanMappingItem
    {
        public string Ean { get; set; } = string.Empty;
        public string Manufacturer { get; set; } = string.Empty;
        public string ProductLine { get; set; } = string.Empty;
        public string Diameter { get; set; } = string.Empty;
        public string DefaultSpool { get; set; } = string.Empty;
    }

    public ObservableCollection<EanMappingItem> Mappings { get; } = new();

    public CatalogEanMappingsViewModel()
    {
        Mappings.Add(new EanMappingItem
        {
            Ean = "0123456789012",
            Manufacturer = "Overture",
            ProductLine = "PLA Professional",
            Diameter = "1.75 mm",
            DefaultSpool = "Overture plastic spool"
        });
        Mappings.Add(new EanMappingItem
        {
            Ean = "0987654321098",
            Manufacturer = "Prusament",
            ProductLine = "PETG",
            Diameter = "1.75 mm",
            DefaultSpool = "Prusa spool (plastic)"
        });
    }

    [RelayCommand]
    private Task AddMappingAsync()
    {
        return Task.CompletedTask;
    }

    [RelayCommand]
    private Task EditMappingAsync(EanMappingItem item)
    {
        return Task.CompletedTask;
    }

    [RelayCommand]
    private Task MergeMappingAsync(EanMappingItem item)
    {
        return Task.CompletedTask;
    }
}
