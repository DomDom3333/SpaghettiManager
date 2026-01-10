using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace SpaghettiManager.App.ViewModels;

public partial class ScanReviewViewModel : ObservableObject, IQueryAttributable
{
    public class EditableField
    {
        public string Label { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public Color StateColor { get; set; } = Colors.Transparent;
    }

    [ObservableProperty]
    private string barcode = string.Empty;

    [ObservableProperty]
    private string summaryTitle = "Unknown filament";

    [ObservableProperty]
    private string summarySubtitle = "Review and complete details";

    [ObservableProperty]
    private bool saveEanMapping;

    public ObservableCollection<EditableField> Fields { get; } = new();

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("barcode", out var barcodeValue))
        {
            Barcode = barcodeValue?.ToString() ?? string.Empty;
        }

        LoadSample();
    }

    [RelayCommand]
    private Task AddToInventoryAsync()
    {
        return Shell.Current.GoToAsync("//inventory");
    }

    [RelayCommand]
    private Task EditCatalogAsync()
    {
        return Shell.Current.GoToAsync("//catalog/ean-mappings");
    }

    private void LoadSample()
    {
        SummaryTitle = "Overture PLA";
        SummarySubtitle = "Fire Engine Red • 1.75 mm";

        Fields.Clear();
        Fields.Add(new EditableField
        {
            Label = "Manufacturer",
            Value = "Overture",
            State = "Auto-filled",
            StateColor = Color.FromArgb("#D0F0C0")
        });
        Fields.Add(new EditableField
        {
            Label = "Product line",
            Value = "PLA Professional",
            State = "Assumed",
            StateColor = Color.FromArgb("#FFF4C2")
        });
        Fields.Add(new EditableField
        {
            Label = "Batch / lot",
            Value = string.Empty,
            State = "Required",
            StateColor = Color.FromArgb("#FFD6D6")
        });
        Fields.Add(new EditableField
        {
            Label = "Spool / carrier",
            Value = "Overture plastic spool",
            State = "Auto-filled",
            StateColor = Color.FromArgb("#D0F0C0")
        });
        Fields.Add(new EditableField
        {
            Label = "Initial weight (g)",
            Value = "1000",
            State = "Auto-filled",
            StateColor = Color.FromArgb("#D0F0C0")
        });
    }
}
