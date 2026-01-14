namespace SpaghettiManager.App.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    public ObservableCollection<string> UnitsOptions { get; } = new() { "g", "kg" };

    [ObservableProperty]
    private string selectedUnits = "g";

    [ObservableProperty]
    private string defaultDiameter = "1.75";

    [ObservableProperty]
    private string defaultStorageHumidity = "35%";

    [ObservableProperty]
    private bool autoSaveEanMappings = true;

    [ObservableProperty]
    private bool enableAdvancedCatalogEditing;

    [RelayCommand]
    private Task ImportExportAsync()
    {
        return Task.CompletedTask;
    }

    [RelayCommand]
    private Task ResetCatalogAsync()
    {
        return Task.CompletedTask;
    }

    [RelayCommand]
    private Task MergeDuplicatesAsync()
    {
        return Task.CompletedTask;
    }
}
