using SpaghettiManager.App.Services;
using SpaghettiManager.Model;
using SpaghettiManager.Model.Records;

namespace SpaghettiManager.App.ViewModels;

public partial class CatalogSectionViewModel : ObservableObject, IQueryAttributable
{
    private readonly SpaghettiDatabase database;
    private string? sectionKey;

    // Pagination for materials and manufacturers
    private const int PageSize = 50;
    private bool hasMoreItems;
    private bool isSearchMode;
    private bool suppressSearchChange;
    private string? activeSearchQuery;
    private CancellationTokenSource? searchDebounceCts;
    private CancellationTokenSource? streamCts;
    private IAsyncEnumerator<object>? currentEnumerator;
    private int loadedCount;

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

    [ObservableProperty]
    private bool isLoadingMore;

    public ObservableCollection<object> Items { get; } = new();

    public CatalogSectionViewModel(SpaghettiDatabase database)
    {
        this.database = database;
    }

    [RelayCommand]
    private async Task LoadMoreAsync()
    {
        if (currentEnumerator is null || IsLoadingMore || !hasMoreItems)
        {
            return;
        }

        if (isSearchMode && !string.IsNullOrWhiteSpace(activeSearchQuery))
        {
            await LoadMoreSearchResultsAsync();
            return;
        }

        await FetchNextNAsync(PageSize);
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
            isSearchMode = false;
            activeSearchQuery = null;
            ResetStreamState();

            switch (sectionKey)
            {
                case "manufacturers":
                    SectionTitle = "Manufacturers";
                    await StartNewStreamAsync();
                    break;
                case "materials":
                    SectionTitle = "Materials";
                    await StartNewStreamAsync();
                    break;
                case "spools":
                    SectionTitle = "Spools / carriers";
                    await StartNewStreamAsync();
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
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task StartNewStreamAsync()
    {
        // Cancel previous stream
        streamCts?.Cancel();
        streamCts?.Dispose();
        currentEnumerator?.DisposeAsync();
        streamCts = new CancellationTokenSource();
        loadedCount = 0;
        SectionSummary = "Loading...";
        EmptyStateMessage = "No items available yet.";

        currentEnumerator = CreateStreamEnumerator(SearchQuery, streamCts.Token);
        await FetchNextNAsync(PageSize);
    }

    private IAsyncEnumerator<object> CreateStreamEnumerator(string? query, CancellationToken token)
    {
        return sectionKey switch
        {
            "materials" => database.StreamMaterialsAsync(query, PageSize, token).Select(x => (object)x).GetAsyncEnumerator(token),
            "manufacturers" => database.StreamManufacturersAsync(query, PageSize, token).Select(x => (object)x).GetAsyncEnumerator(token),
            "spools" => database.StreamCarriersAsync(query, PageSize, token).Select(x => (object)x).GetAsyncEnumerator(token),
            _ => AsyncEnumerable.Empty<object>().GetAsyncEnumerator(token)
        };
    }

    private async Task FetchNextNAsync(int n)
    {
        if (currentEnumerator is null)
        {
            hasMoreItems = false;
            CanLoadMore = false;
            return;
        }

        IsLoadingMore = true;
        try
        {
            var added = 0;
            while (added < n)
            {
                if (!await currentEnumerator.MoveNextAsync())
                {
                    hasMoreItems = false;
                    CanLoadMore = false;
                    break;
                }
                Items.Add(currentEnumerator.Current);
                added++;
                loadedCount++;
            }

            if (added > 0)
            {
                hasMoreItems = true; // assume more exists until enumerator ends
                CanLoadMore = true;
                SectionSummary = $"{loadedCount} items";
            }
            else if (loadedCount == 0)
            {
                EmptyStateMessage = "No items found.";
                SectionSummary = "0 items";
            }
        }
        finally
        {
            IsLoadingMore = false;
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
            ResetStreamState();
            suppressSearchChange = true; // prevent immediate re-trigger
            SearchQuery = query;
            suppressSearchChange = false;
            await StartNewStreamAsync();
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

        IsLoadingMore = true;
        try
        {
            await FetchNextNAsync(PageSize);
        }
        finally
        {
            IsLoadingMore = false;
        }
    }

    private void ResetStreamState()
    {
        loadedCount = 0;
        hasMoreItems = false;
        CanLoadMore = false;
        streamCts?.Cancel();
        streamCts?.Dispose();
        currentEnumerator?.DisposeAsync();
        currentEnumerator = null;
    }
}
