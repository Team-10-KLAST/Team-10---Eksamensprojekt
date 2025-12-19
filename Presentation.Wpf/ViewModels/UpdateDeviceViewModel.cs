using System;
using System.Windows;
using System.Windows.Input;
using Application.Interfaces.Service;
using Application.Models;
using Application.Models.DisplayModels;
using Presentation.Wpf.Commands;

namespace Presentation.Wpf.ViewModels
{
    // ViewModel for updating an existing device via an overlay panel
    // Responsible for UI state and delegating domain rules to the service layer
    public class UpdateDeviceViewModel : OverlayPanelViewModelBase
    {
        // Service used to validate and persist device updates
        private readonly IDeviceService _deviceService;

        // Currently selected device being edited
        private DeviceDisplayModel _selectedDevice;

        // Raised after a device has been successfully updated
        // Used by parent views to refresh device lists
        public event EventHandler? DeviceUpdated;

        // Selected device bound to the overlay
        public DeviceDisplayModel SelectedDevice
        {
            get => _selectedDevice;
            set
            {
                if (_selectedDevice != value)
                {
                    _selectedDevice = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(Type));
                    OnPropertyChanged(nameof(OS));
                    OnPropertyChanged(nameof(Location));
                    OnPropertyChanged(nameof(Owner));
                    OnPropertyChanged(nameof(RegistrationDate));
                    OnPropertyChanged(nameof(ExpirationDate));
                    OnPropertyChanged(nameof(SelectedStatus));
                    OnPropertyChanged(nameof(Wiped));
                }
            }
        }

        // Read-only convenience properties for display bindings
        public string Type => SelectedDevice?.Type ?? string.Empty;
        public string OS => SelectedDevice?.OS ?? string.Empty;

        // Available status values for status selection
        public Array StatusOptions => Enum.GetValues(typeof(DeviceStatus));

        // Device owner display name
        public string Owner
        {
            get => SelectedDevice?.OwnerFullName ?? string.Empty;
            set
            {
                if (SelectedDevice == null)
                    return;

                if (SelectedDevice.OwnerFullName != value)
                {
                    SelectedDevice.OwnerFullName = value;
                    OnPropertyChanged();
                }
            }
        }

        // Needed-by date displayed as read-only information
        public DateTime? NeededByDate
        {
            get => SelectedDevice?.NeededByDate;
        }

        // Device location
        public string Location
        {
            get => SelectedDevice?.Location ?? string.Empty;
            set
            {
                if (SelectedDevice == null)
                    return;

                if (SelectedDevice.Location != value)
                {
                    SelectedDevice.Location = value;
                    OnPropertyChanged();
                }
            }
        }

        // Selected device status mapped between UI enum and underlying string value
        public DeviceStatus SelectedStatus
        {
            get
            {
                if (SelectedDevice == null)
                    return DeviceStatus.REGISTERED;

                if (Enum.TryParse<DeviceStatus>(SelectedDevice.Status, out var statusEnum))
                    return statusEnum;

                return DeviceStatus.REGISTERED;
            }
            set
            {
                if (SelectedDevice == null)
                    return;

                var newValue = value.ToString();
                if (SelectedDevice.Status != newValue)
                {
                    SelectedDevice.Status = newValue;
                    OnPropertyChanged(nameof(SelectedStatus));
                }
            }
        }

        // Device registration date
        // Used as the basis for calculating a default expiration date
        public DateTime? RegistrationDate
        {
            get => SelectedDevice?.RegistrationDate;
            set
            {
                if (SelectedDevice == null)
                    return;

                if (SelectedDevice.RegistrationDate != value)
                {
                    SelectedDevice.RegistrationDate = value;

                    if (value.HasValue)
                    {
                        SelectedDevice.ExpirationDate =
                            _deviceService.CalculateDefaultExpiryDate(value.Value);

                        OnPropertyChanged(nameof(ExpirationDate));
                    }

                    OnPropertyChanged();
                }
            }
        }

        // Device expiration date
        public DateTime? ExpirationDate
        {
            get => SelectedDevice?.ExpirationDate;
            set
            {
                if (SelectedDevice == null)
                    return;

                if (SelectedDevice.ExpirationDate != value)
                {
                    SelectedDevice.ExpirationDate = value;
                    OnPropertyChanged();
                }
            }
        }

        // Indicates whether the device has been wiped
        public bool Wiped
        {
            get => SelectedDevice?.Wiped ?? false;
            set
            {
                if (SelectedDevice == null)
                    return;

                if (SelectedDevice.Wiped != value)
                {
                    SelectedDevice.Wiped = value;
                    OnPropertyChanged();
                }
            }
        }

        // Commands
        public ICommand UpdateDeviceCommand { get; }
        public ICommand CancelCommand { get; }

        // Constructor
        public UpdateDeviceViewModel(DeviceDisplayModel device, IDeviceService deviceService)
        {
            SelectedDevice = device;
            _deviceService = deviceService;

            UpdateDeviceCommand = new RelayCommand(UpdateDevice);
            CancelCommand = new RelayCommand(Cancel);
        }

        // Handles updating the device by delegating validation and persistence
        // to the service layer and notifying listeners on success
        private void UpdateDevice()
        {
            if (SelectedStatus == DeviceStatus.INSTOCK && !Wiped)
            {
                MessageBox.Show(
                    "The device must be wiped before it can be set to 'In stock'. " +
                    "Please mark the device as wiped first.",
                    "Cannot update device",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            try
            {
                _deviceService.UpdateDevice(SelectedDevice);
                DeviceUpdated?.Invoke(this, EventArgs.Empty);
                CloseOverlay();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Error during update",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        // Cancels the update operation and closes the overlay
        private void Cancel()
        {
            CloseOverlay();
        }
    }
}
