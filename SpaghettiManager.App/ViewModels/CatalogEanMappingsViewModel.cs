using SpaghettiManager.App.Services;
using SpaghettiManager.Model;
using SpaghettiManager.Model.Records;

namespace SpaghettiManager.App.ViewModels;

public partial class CatalogEanMappingsViewModel : ObservableObject
{
    private readonly SpaghettiDatabase database;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private string summary = string.Empty;

    [ObservableProperty]
    private string emptyMessage = "No barcode mappings have been added yet.";

    public ObservableCollection<Spool> Mappings { get; } = new();

    public CatalogEanMappingsViewModel(SpaghettiDatabase database)
    {
        this.database = database;
        _ = LoadMappingsAsync();
    }

    [RelayCommand]
    private Task AddMappingAsync()
    {
        return Task.CompletedTask;
    }

    [RelayCommand]
    private Task EditMappingAsync(Spool item)
    {
        return Task.CompletedTask;
    }

    [RelayCommand]
    private Task MergeMappingAsync(Spool item)
    {
        return Task.CompletedTask;
    }

    private async Task LoadMappingsAsync()
    {
        IsLoading = true;
        try
        {
            Mappings.Clear();

            var mappings = await database.GetSpoolsAsync();
            foreach (var mapping in mappings.OrderByDescending(item => item.LastUpdatedAt))
            {
                Mappings.Add(mapping);
            }

            Summary = $"{Mappings.Count} barcode mappings";
        }
        finally
        {
            IsLoading = false;
        }
    }
}
