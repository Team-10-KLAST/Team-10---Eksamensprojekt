using Application.Models;
using Application.Models.DisplayModels;
using Presentation.Wpf.Commands;
using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace Presentation.Wpf.ViewModels
{
    public class UpdateDeviceViewModel : OverlayPanelViewModelBase
    {
        private DeviceDisplayModel _selectedDevice;
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
                    OnPropertyChanged(nameof(StatusHistory));
                    OnPropertyChanged(nameof(StatusHistoryString));
                    OnPropertyChanged(nameof(Owner));
                    OnPropertyChanged(nameof(RegistrationDate));
                    OnPropertyChanged(nameof(ExpirationDate));
                    OnPropertyChanged(nameof(SelectedStatus));
                    OnPropertyChanged(nameof(Wiped));
                }
            }
        }

        // Read-only convenience properties for bindings
        public string Type => SelectedDevice?.Type ?? string.Empty;
        public string OS => SelectedDevice?.OS ?? string.Empty;
        public List<string> StatusHistory => SelectedDevice?.StatusHistory ?? new List<string>();
        public string StatusHistoryString => string.Join(Environment.NewLine, StatusHistory);

        public Array StatusOptions => Enum.GetValues(typeof(DeviceStatus));

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

                    // Automatically set ExpirationDate = RegistrationDate + 3 years
                    if (value.HasValue)
                    {
                        SelectedDevice.ExpirationDate = value.Value.AddYears(3);
                        OnPropertyChanged(nameof(ExpirationDate));
                    }

                    OnPropertyChanged();
                }
            }
        }

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

        public ICommand UpdateDeviceCommand { get; }
        public ICommand CancelCommand { get; }

        public UpdateDeviceViewModel(DeviceDisplayModel device)
        {
            SelectedDevice = device ?? new DeviceDisplayModel();
            UpdateDeviceCommand = new RelayCommand(UpdateDevice);
            CancelCommand = new RelayCommand(Cancel);
        }

        // Temporary: just close the overlay for now
        private void UpdateDevice()
        {
            // Later you can call a service here to persist changes to the database.
            CloseOverlay();
        }

        private void Cancel()
        {
            CloseOverlay();
        }
    }
}
