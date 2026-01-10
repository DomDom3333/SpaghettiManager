using BarcodeScanning;

namespace SpaghettiManager.App.ViewModels;

public partial class MainPageViewModel : ObservableObject
{
    private readonly BaseServices _services;

    public MainPageViewModel(BaseServices services)
    {
        _services = services;
    }

    // Barcode text shown in the Entry
    [ObservableProperty]
    private string _barcode = string.Empty;

    // Controls scanning state
    [ObservableProperty]
    private bool _isScanning;

    // Bindable properties for the CameraView
    [ObservableProperty]
    private bool _isScannerVisible;

    [ObservableProperty]
    private bool _isScannerEnabled;

    [ObservableProperty]
    private bool _cameraEnabled;

    [ObservableProperty]
    private bool _aimMode;

    // Toggle scanning on/off
    [RelayCommand]
    private void ToggleScanning()
    {
        IsScanning = !IsScanning;
        IsScannerEnabled = IsScanning;
        CameraEnabled = IsScanning;
    }

    // Called from page OnAppearing
    public async Task AppearingAsync()
    {
        // Touch the injected service to avoid unused-field warnings (service used by other features)
        _ = _services;

        var status = await Permissions.CheckStatusAsync<Permissions.Camera>();
        if (status != PermissionStatus.Granted)
        {
            status = await Permissions.RequestAsync<Permissions.Camera>();
        }

        if (status == PermissionStatus.Granted)
        {
            IsScannerVisible = true;
            IsScannerEnabled = true;
            CameraEnabled = true;
            AimMode = false;
            IsScanning = true;
        }
        else
        {
            IsScannerVisible = false;
            IsScannerEnabled = false;
            CameraEnabled = false;
            AimMode = false;
            IsScanning = false;
        }
    }

    // Called from page OnDisappearing
    [RelayCommand]
    internal void Disappearing()
    {
        IsScannerEnabled = false;
        CameraEnabled = false;
        IsScanning = false;
    }

    // Receives barcode results from the CameraView event
    [RelayCommand]
    internal void DetectionFinished(IReadOnlySet<BarcodeResult> results)
    {
        if (results is { Count: > 0 })
        {
            // Use the first result for simplicity
            Barcode = results.FirstOrDefault()?.DisplayValue ?? string.Empty;
        }
    }
}