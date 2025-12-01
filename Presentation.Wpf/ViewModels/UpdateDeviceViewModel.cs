using Application.Interfaces;
using Application.Models.DisplayModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Presentation.Wpf.Commands;
using System.Buffers;

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
                }
            }
        }

        // Properties for binding, fx:
        public string Type => SelectedDevice?.Type ?? string.Empty;
        public string OS => SelectedDevice?.OS ?? string.Empty;
        public List<string> StatusHistory => SelectedDevice?.StatusHistory ?? new List<string>();
        public string StatusHistoryString => string.Join(Environment.NewLine, StatusHistory);


        public string Owner
        {
            get => SelectedDevice.OwnerFullName;
            set
            {
                if (SelectedDevice.OwnerFullName != value)
                {
                    SelectedDevice.OwnerFullName = value;
                    OnPropertyChanged();
                }
            }
        }
        public string Location
        {
            get => SelectedDevice.Location;
            set
            {
                if (SelectedDevice.Location != value)
                {
                    SelectedDevice.Location = value;
                    OnPropertyChanged();
                }
            }
        }
        public string Status
        {
            get => SelectedDevice.Status;
            set
            {
                if (SelectedDevice.Status != value)
                {
                    SelectedDevice.Status = value;
                    OnPropertyChanged();
                }
            }
        }

        public DateTime EventDate
        {
            get => SelectedDevice.EventDate;
            set
            {
                if (SelectedDevice.EventDate != value)
                {
                    SelectedDevice.EventDate = value;
                    OnPropertyChanged();
                }
            }
        }

        public string RegistrationDate
        {
            get => SelectedDevice.RegistrationDate.ToString("yyyy-MM-dd");
            set
            {
                if (DateTime.TryParse(value, out var date))
                {
                    SelectedDevice.RegistrationDate = date;
                    OnPropertyChanged();
                }
            }
        }

        public DateTime ExpirationDate
        {
            get => SelectedDevice.ExpirationDate;
            set
            {
                if (SelectedDevice.ExpirationDate != value)
                {
                    SelectedDevice.ExpirationDate = value;
                    OnPropertyChanged();
                }
            }
        }

        public decimal Price
        {
            get => SelectedDevice.Price;
            set
            {
                if (SelectedDevice.Price != value)
                {
                    SelectedDevice.Price = value;
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

        private void UpdateDevice()
        {
            _deviceService.UpdateDevice(SelectedDevice); // Saves changes

            //Gets updated status history
            var updatedHistory = _deviceService.GetStatusHistory(SelectedDevice.DeviceID);
            SelectedDevice.StatusHistory = updatedHistory;
            OnPropertyChanged(nameof(StatusHistory));

            CloseOverlay();
        }

        private void Cancel()
        {
            CloseOverlay();
        }
    }
}
