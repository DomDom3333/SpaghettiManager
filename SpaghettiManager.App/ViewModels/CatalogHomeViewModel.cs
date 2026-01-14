namespace SpaghettiManager.App.ViewModels;

public partial class CatalogHomeViewModel : ObservableObject
{
    public class CatalogSectionItem
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? Route { get; set; }
    }

    public ObservableCollection<CatalogSectionItem> Sections { get; } = new();

    public CatalogHomeViewModel()
    {
        Sections.Add(new CatalogSectionItem
        {
            Title = "Manufacturers",
            Description = "Aliases, defaults, merge rules",
            Route = "///catalog/section?key=manufacturers"
        });
        Sections.Add(new CatalogSectionItem
        {
            Title = "EAN mappings",
            Description = "Barcode to product mapping",
            Route = "///catalog/ean-mappings"
        });
        Sections.Add(new CatalogSectionItem
        {
            Title = "Spools / carriers",
            Description = "Dimensions, tare, AMS compatibility",
            Route = "///catalog/section?key=spools"
        });
        Sections.Add(new CatalogSectionItem
        {
            Title = "Materials",
            Description = "Material profiles and defaults",
            Route = "///catalog/section?key=materials"
        });
        Sections.Add(new CatalogSectionItem
        {
            Title = "Additives",
            Description = "Optional blends and modifiers",
            Route = "///catalog/section?key=additives"
        });
    }

    [RelayCommand]
    private Task OpenSectionAsync(CatalogSectionItem section)
    {
        if (section?.Route is null)
        {
            return Task.CompletedTask;
        }

        return Shell.Current.GoToAsync(section.Route);
    }
}
