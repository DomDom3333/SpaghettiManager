using Microsoft.Maui.Controls;
using SpaghettiManager.Model.Records;

namespace SpaghettiManager.App.Views;

public class CatalogItemTemplateSelector : DataTemplateSelector
{
    public DataTemplate ManufacturerTemplate { get; set; } = null!;
    public DataTemplate MaterialTemplate { get; set; } = null!;
    public DataTemplate CarrierTemplate { get; set; } = null!;

    protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
    {
        return item switch
        {
            Manufacturer => ManufacturerTemplate,
            Material => MaterialTemplate,
            Carrier => CarrierTemplate,
            _ => MaterialTemplate
        };
    }
}
