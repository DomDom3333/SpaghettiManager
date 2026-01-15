using SpaghettiManager.App.Services;
using SpaghettiManager.Model;
using SpaghettiManager.Model.Records;

namespace SpaghettiManager.App.ViewModels;

public partial class CatalogSectionViewModel : ObservableObject, IQueryAttributable
{
    private readonly SpaghettiDatabase database;
    private readonly List<object> sourceItems = new();
    private string? sectionKey;

    [ObservableProperty]
    private string sectionTitle = string.Empty;

    [ObservableProperty]
    private string sectionSummary = string.Empty;

    [ObservableProperty]
    private string emptyStateMessage = "No items available yet.";

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private bool supportsSearch;

    [ObservableProperty]
    private string searchQuery = string.Empty;

    public ObservableCollection<object> Items { get; } = new();

    public CatalogSectionViewModel(SpaghettiDatabase database)
    {
        this.database = database;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("key", out var value))
        {
            sectionKey = value?.ToString()?.ToLowerInvariant();
            _ = LoadSectionAsync();
        }
    }

    partial void OnSearchQueryChanged(string value)
    {
        ApplyFilter();
    }

    private async Task LoadSectionAsync()
    {
        if (string.IsNullOrWhiteSpace(sectionKey))
        {
            return;
        }

        IsLoading = true;
        try
        {
            SupportsSearch = sectionKey is "manufacturers" or "materials" or "spools";
            SearchQuery = string.Empty;
            Items.Clear();
            sourceItems.Clear();

            switch (sectionKey)
            {
                case "manufacturers":
                    SectionTitle = "Manufacturers";
                    await LoadManufacturersAsync();
                    break;
                case "materials":
                    SectionTitle = "Materials";
                    await LoadMaterialsAsync();
                    break;
                case "spools":
                    SectionTitle = "Spools / carriers";
                    await LoadCarriersAsync();
                    break;
                case "additives":
                    SectionTitle = "Additives";
                    SectionSummary = "No additive blends tracked yet.";
                    EmptyStateMessage = "Additive profiles will appear here once added.";
                    break;
                default:
                    SectionTitle = sectionKey;
                    SectionSummary = string.Empty;
                    EmptyStateMessage = "Nothing to show yet.";
                    break;
            }

            ApplyFilter();
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadManufacturersAsync()
    {
        var manufacturers = await database.GetManufacturersAsync();
        var ordered = manufacturers
            .OrderBy(item => item.Name)
            .ToList();

        SectionSummary = $"{ordered.Count} manufacturers";
        EmptyStateMessage = "No manufacturers found in the catalog.";
        sourceItems.AddRange(ordered);
    }

    private async Task LoadMaterialsAsync()
    {
        var materials = await database.GetMaterialsAsync();
        var ordered = materials
            .OrderBy(item => item.Manufacturer)
            .ThenBy(item => item.Name)
            .ToList();

        var familyCount = ordered.Select(item => item.Family).Distinct().Count();
        var manufacturerCount = ordered
            .Select(item => item.Manufacturer)
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Count();

        SectionSummary = $"{ordered.Count} materials • {familyCount} families • {manufacturerCount} manufacturers";
        EmptyStateMessage = "No materials found in the catalog.";
        sourceItems.AddRange(ordered);
    }

    private async Task LoadCarriersAsync()
    {
        var carriers = await database.GetCarriersAsync();
        var ordered = carriers
            .OrderBy(item => item.Manufacturer)
            .ThenBy(item => item.SpoolType)
            .ToList();

        SectionSummary = $"{ordered.Count} carriers";
        EmptyStateMessage = "No carriers have been added yet.";
        sourceItems.AddRange(ordered);
    }

    private void ApplyFilter()
    {
        Items.Clear();
        IEnumerable<object> filtered = sourceItems;

        if (!string.IsNullOrWhiteSpace(SearchQuery))
        {
            var query = SearchQuery.Trim();
            filtered = sourceItems.Where(item => MatchesSearch(item, query));
        }

        foreach (var item in filtered)
        {
            Items.Add(item);
        }
    }

    private static bool MatchesSearch(object item, string query)
    {
        return item switch
        {
            Manufacturer manufacturer =>
                Contains(manufacturer.Name, query)
                || Contains(manufacturer.Country, query)
                || Contains(manufacturer.Website, query),
            Material material =>
                Contains(material.Name, query)
                || Contains(material.Manufacturer, query)
                || Contains(material.Family.ToString(), query)
                || Contains(material.AdditiveMaterial.ToString(), query)
                || Contains(material.Color, query),
            Carrier carrier =>
                Contains(carrier.Manufacturer, query)
                || Contains(carrier.SpoolType.ToString(), query),
            _ => false
        };
    }

    private static bool Contains(string? value, string query)
    {
        return !string.IsNullOrWhiteSpace(value)
            && value.Contains(query, StringComparison.OrdinalIgnoreCase);
    }
}
