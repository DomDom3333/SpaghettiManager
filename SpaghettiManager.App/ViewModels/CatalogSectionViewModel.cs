using SpaghettiManager.Model;
using SpaghettiManager.Model.Records;

namespace SpaghettiManager.App.ViewModels;

public partial class CatalogSectionViewModel : ObservableObject, IQueryAttributable
{
    [ObservableProperty]
    private string sectionTitle = string.Empty;

    public ObservableCollection<object> Items { get; } = new();

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("key", out var value))
        {
            SectionTitle = value?.ToString() ?? string.Empty;
        }
    }
}
