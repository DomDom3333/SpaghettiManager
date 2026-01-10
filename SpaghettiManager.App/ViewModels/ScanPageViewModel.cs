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
    private void SimulateScan()
    {
        Barcode = "0123456789012";
    }

    [RelayCommand]
    private Task ReviewAsync()
    {
        var barcodeValue = string.IsNullOrWhiteSpace(Barcode) ? "unknown" : Barcode;
        return Shell.Current.GoToAsync($"scan/review?barcode={barcodeValue}");
    }
}
