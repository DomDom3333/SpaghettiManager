using SpaghettiManager.App.Services;
using SpaghettiManager.Model;
using SpaghettiManager.Model.Records;

namespace SpaghettiManager.App.ViewModels;

public partial class CatalogSectionViewModel : ObservableObject, IQueryAttributable
{
    private readonly SpaghettiDatabase database;
    private readonly List<object> sourceItems = new();
    private string? sectionKey;
    
    // Pagination for materials
    private const int PageSize = 50;
    private int currentOffset;
    private int totalMaterialCount;
    private bool isLoadingMore;
    private bool hasMoreItems;

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

    [ObservableProperty]
    private bool canLoadMore;

    public ObservableCollection<object> Items { get; } = new();

    public CatalogSectionViewModel(SpaghettiDatabase database)
    {
        this.database = database;
    }

    [RelayCommand]
    private async Task LoadMoreAsync()
    {
        if (sectionKey != "materials" || isLoadingMore || !hasMoreItems || !string.IsNullOrWhiteSpace(SearchQuery))
        {
            return;
        }

        await LoadMoreMaterialsAsync();
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
        // Reset pagination state
        currentOffset = 0;
        totalMaterialCount = 0;
        hasMoreItems = false;
        CanLoadMore = false;

        // Get summary info without loading all materials
        var summary = await database.GetMaterialsSummaryAsync();
        totalMaterialCount = summary.TotalCount;
        
        SectionSummary = $"{summary.TotalCount} materials • {summary.FamilyCount} families • {summary.ManufacturerCount} manufacturers";
        EmptyStateMessage = "No materials found in the catalog.";

        // Load first page (already ordered by Manufacturer, Name in the database query)
        var materials = await database.GetMaterialsPagedAsync(0, PageSize);
        currentOffset = materials.Count;
        hasMoreItems = currentOffset < totalMaterialCount;
        CanLoadMore = hasMoreItems;

        sourceItems.AddRange(materials);
    }

    private async Task LoadMoreMaterialsAsync()
    {
        if (isLoadingMore || !hasMoreItems)
        {
            return;
        }

        isLoadingMore = true;
        try
        {
            var materials = await database.GetMaterialsPagedAsync(currentOffset, PageSize);
            
            currentOffset += materials.Count;
            hasMoreItems = currentOffset < totalMaterialCount;
            CanLoadMore = hasMoreItems;

            foreach (var material in materials)
            {
                sourceItems.Add(material);
                Items.Add(material);
            }
        }
        finally
        {
            isLoadingMore = false;
        }
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
            
            // For materials with pagination, search needs special handling
            if (sectionKey == "materials" && hasMoreItems)
            {
                // Trigger async search that loads from database
                _ = SearchMaterialsAsync(query);
                return;
            }
            
            filtered = sourceItems.Where(item => MatchesSearch(item, query));
        }
        else
        {
            // When clearing search for materials, reset to paginated view
            if (sectionKey == "materials")
            {
                CanLoadMore = hasMoreItems;
            }
        }

        foreach (var item in filtered)
        {
            Items.Add(item);
        }
    }

    private async Task SearchMaterialsAsync(string query)
    {
        IsLoading = true;
        try
        {
            // When searching, we need to search all materials in the database
            var allMaterials = await database.GetMaterialsAsync();
            var filtered = allMaterials
                .Where(m => MatchesSearch(m, query))
                .OrderBy(m => m.Manufacturer)
                .ThenBy(m => m.Name);

            Items.Clear();
            foreach (var material in filtered)
            {
                Items.Add(material);
            }
            
            // Disable load more during search
            CanLoadMore = false;
        }
        finally
        {
            IsLoading = false;
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
