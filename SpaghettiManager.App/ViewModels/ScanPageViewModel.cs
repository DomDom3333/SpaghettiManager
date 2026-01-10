using System.Linq;
using BarcodeScanning;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace SpaghettiManager.App.ViewModels;

public partial class ScanPageViewModel : ObservableObject
{
    [ObservableProperty]
    private string barcode = string.Empty;

    [ObservableProperty]
    private bool isScanning;

    [RelayCommand]
    private void ToggleScanning()
    {
        IsScanning = !IsScanning;
    }

    [RelayCommand]
    private void HandleDetectionFinished(IReadOnlySet<BarcodeResult> results)
    {
        if (results is null || results.Count == 0)
        {
            return;
        }

        var result = results.FirstOrDefault();
        if (result is null)
        {
            return;
        }

        var value = string.IsNullOrWhiteSpace(result.RawValue)
            ? result.DisplayValue
            : result.RawValue;

        if (!string.IsNullOrWhiteSpace(value))
        {
            Barcode = value;
            IsScanning = false;
        }
    }

    [RelayCommand]
    private Task ReviewAsync()
    {
        var barcodeValue = string.IsNullOrWhiteSpace(Barcode) ? "unknown" : Barcode;
        return Shell.Current.GoToAsync($"///scan/review?barcode={barcodeValue}");
    }

    public void SetScanning(bool enabled)
    {
        IsScanning = enabled;
    }
}
