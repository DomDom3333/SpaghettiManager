using SpaghettiManager.App.Services;
using SpaghettiManager.Model;
using SpaghettiManager.Model.Records;

namespace SpaghettiManager.App.ViewModels;

public partial class CatalogSectionViewModel : ObservableObject, IQueryAttributable
{
    private readonly SpaghettiDatabase database;
    private readonly List<object> sourceItems = new();
    private string? sectionKey;

    // Pagination for materials and manufacturers
    private const int PageSize = 50;
    private int currentOffset;
    private int totalItemCount;
    private bool isLoadingMore;
    private bool hasMoreItems;
    private bool isSearchMode;
    private bool suppressSearchChange;
    private string? activeSearchQuery;
    private CancellationTokenSource? searchDebounceCts;

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
        if (!IsPagedSection() || isLoadingMore || !hasMoreItems)
        {
            return;
        }

        if (isSearchMode && !string.IsNullOrWhiteSpace(activeSearchQuery))
        {
            await LoadMoreSearchResultsAsync();
            return;
        }

        await LoadMorePagedItemsAsync();
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
        if (suppressSearchChange)
        {
            return;
        }

        searchDebounceCts?.Cancel();
        searchDebounceCts = new CancellationTokenSource();
        _ = ApplySearchAsync(value, searchDebounceCts.Token);
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
            suppressSearchChange = true;
            SearchQuery = string.Empty;
            suppressSearchChange = false;
            Items.Clear();
            sourceItems.Clear();
            isSearchMode = false;
            activeSearchQuery = null;
            ResetPaginationState();

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

            if (!IsPagedSection())
            {
                ApplyFilter();
            }
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadManufacturersAsync()
    {
        var totalCount = await database.GetManufacturersCountAsync();
        var ordered = await database.GetManufacturersPagedAsync(0, PageSize);

        SectionSummary = $"{totalCount} manufacturers";
        EmptyStateMessage = "No manufacturers found in the catalog.";
        SetPaginationState(totalCount, ordered.Count);
        foreach (var manufacturer in ordered)
        {
            Items.Add(manufacturer);
        }
    }

    private async Task LoadMaterialsAsync()
    {
        // Get summary info without loading all materials
        var summary = await database.GetMaterialsSummaryAsync();
        totalItemCount = summary.TotalCount;
        
        SectionSummary = $"{summary.TotalCount} materials • {summary.FamilyCount} families • {summary.ManufacturerCount} manufacturers";
        EmptyStateMessage = "No materials found in the catalog.";

        // Load first page (already ordered by Manufacturer, Name in the database query)
        var materials = await database.GetMaterialsPagedAsync(0, PageSize);
        SetPaginationState(totalItemCount, materials.Count);
        foreach (var material in materials)
        {
            Items.Add(material);
        }
    }

    private async Task LoadMorePagedItemsAsync()
    {
        isLoadingMore = true;
        try
        {
            IReadOnlyList<object> items = sectionKey switch
            {
                "materials" => (await database.GetMaterialsPagedAsync(currentOffset, PageSize)).Cast<object>().ToList(),
                "manufacturers" => (await database.GetManufacturersPagedAsync(currentOffset, PageSize)).Cast<object>().ToList(),
                _ => []
            };
            
            currentOffset += items.Count;
            hasMoreItems = currentOffset < totalItemCount;
            CanLoadMore = hasMoreItems;

            foreach (var item in items)
            {
                Items.Add(item);
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
        if (IsPagedSection())
        {
            return;
        }

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

    private async Task ApplySearchAsync(string query, CancellationToken token)
    {
        try
        {
            await Task.Delay(250, token);
        }
        catch (TaskCanceledException)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(sectionKey))
        {
            return;
        }

        if (!IsPagedSection())
        {
            ApplyFilter();
            return;
        }

        if (string.IsNullOrWhiteSpace(query))
        {
            await LoadSectionAsync();
            return;
        }

        await StartSearchAsync(query.Trim());
    }

    private async Task StartSearchAsync(string query)
    {
        IsLoading = true;
        try
        {
            activeSearchQuery = query;
            isSearchMode = true;
            Items.Clear();

            int searchTotal;
            IReadOnlyList<object> results = sectionKey switch
            {
                "materials" => (await database.SearchMaterialsPagedAsync(query, 0, PageSize)).Cast<object>().ToList(),
                "manufacturers" => (await database.SearchManufacturersPagedAsync(query, 0, PageSize)).Cast<object>().ToList(),
                _ => []
            };

            searchTotal = sectionKey switch
            {
                "materials" => await database.SearchMaterialsCountAsync(query),
                "manufacturers" => await database.SearchManufacturersCountAsync(query),
                _ => 0
            };

            SectionSummary = $"{searchTotal} results";
            EmptyStateMessage = $"No matches for \"{query}\".";

            SetPaginationState(searchTotal, results.Count);
            foreach (var item in results)
            {
                Items.Add(item);
            }
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadMoreSearchResultsAsync()
    {
        if (string.IsNullOrWhiteSpace(activeSearchQuery))
        {
            return;
        }

        isLoadingMore = true;
        try
        {
            IReadOnlyList<object> results = sectionKey switch
            {
                "materials" => (await database.SearchMaterialsPagedAsync(activeSearchQuery, currentOffset, PageSize)).Cast<object>().ToList(),
                "manufacturers" => (await database.SearchManufacturersPagedAsync(activeSearchQuery, currentOffset, PageSize)).Cast<object>().ToList(),
                _ => []
            };

            currentOffset += results.Count;
            hasMoreItems = currentOffset < totalItemCount;
            CanLoadMore = hasMoreItems;

            foreach (var item in results)
            {
                Items.Add(item);
            }
        }
        finally
        {
            isLoadingMore = false;
        }
    }

    private static bool MatchesSearch(object item, string query)
    {
        return item switch
        {
            Manufacturer manufacturer =>
                Contains(manufacturer.Name, query)
                || Contains(manufacturer.Country, query)
                || Contains(manufacturer.Website, query)
                || Contains(manufacturer.Aliases, query),
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

    private bool IsPagedSection()
    {
        return sectionKey is "materials" or "manufacturers";
    }

    private void ResetPaginationState()
    {
        currentOffset = 0;
        totalItemCount = 0;
        hasMoreItems = false;
        CanLoadMore = false;
    }

    private void SetPaginationState(int totalCount, int loadedCount)
    {
        totalItemCount = totalCount;
        currentOffset = loadedCount;
        hasMoreItems = currentOffset < totalItemCount;
        CanLoadMore = hasMoreItems;
    }
}
