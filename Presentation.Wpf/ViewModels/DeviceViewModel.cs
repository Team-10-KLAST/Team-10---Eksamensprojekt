using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Input;
using Application.Interfaces.Service;
using Application.Models;
using Presentation.Wpf.Commands;

namespace Presentation.Wpf.ViewModels
{
    public class DeviceViewModel : OverlayHostViewModel
    {
        // Service used to retrieve devices from the database
        private readonly IDeviceService _deviceService;
        private readonly IDeviceDescriptionService _deviceDescriptionService;

        // View-only representation of a device row for the Devices table
        public class DeviceRow
        {
            public int DeviceId { get; set; }

            public string Type { get; set; } = string.Empty;

            public string Os { get; set; } = string.Empty;

            public string Owner { get; set; } = string.Empty;

            public string Location { get; set; } = string.Empty;

            public string Status { get; set; } = string.Empty;

            public DateOnly? RegisteredDate { get; set; }

            public DateOnly? ExpiryDate { get; set; }
        }

        public ObservableCollection<DeviceRow> Devices { get; } = new();

        private DeviceRow? _selectedDevice;

        public DeviceRow? SelectedDevice
        {
            get => _selectedDevice;
            set
            {
                if (!Equals(_selectedDevice, value))
                {
                    _selectedDevice = value;
                    OnPropertyChanged(nameof(SelectedDevice));
                }
            }
        }

        private string _searchText = string.Empty;

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (_searchText != value)
                {
                    _searchText = value;
                    OnPropertyChanged(nameof(SearchText));

                    // Trigger refiltering when the search text changes
                    var view = CollectionViewSource.GetDefaultView(Devices);
                    view.Refresh();
                }
            }
        }

        // Reloads data from the service
        public ICommand RefreshCommand { get; }

        // Opens/highlights the selected device row
        public ICommand OpenDeviceCommand { get; }

        public ICommand RegisterDeviceCommand { get; }

        public DeviceViewModel(IDeviceService deviceService, IDeviceDescriptionService deviceDescriptionService)
        {
            _deviceService = deviceService ?? throw new ArgumentNullException(nameof(deviceService));

            var view = CollectionViewSource.GetDefaultView(Devices);
            view.Filter = DeviceFilter;

            RefreshCommand = new RelayCommand(LoadDevices);
            OpenDeviceCommand = new RelayCommand<DeviceRow>(OpenDevice);
            RegisterDeviceCommand = new RelayCommand(OpenRegisterDeviceOverlay);

            LoadDevices();
            _deviceDescriptionService = deviceDescriptionService;
        }

        private void LoadDevices()
        {
            Devices.Clear();

            foreach (Device device in _deviceService.GetAllDevices())
            {
                var row = new DeviceRow
                {
                    DeviceId = device.DeviceID,

                    // Placeholder values - replace with actual data retrieval logic
                    Type = string.Empty,
                    Os = string.Empty,
                    Owner = string.Empty,
                    Location = string.Empty,

                    Status = device.Status.ToString(),
                    RegisteredDate = device.PurchaseDate,
                    ExpiryDate = device.ExpectedEndDate
                };

                Devices.Add(row);
            }

            var view = CollectionViewSource.GetDefaultView(Devices);
            view.Refresh();
        }

        // Defines how the Devices collection is filtered based on SearchText
        private bool DeviceFilter(object obj)
        {
            if (obj is not DeviceRow row)
                return false;

            if (string.IsNullOrWhiteSpace(SearchText))
                return true;

            var search = SearchText.Trim();

            bool matchesId = row.DeviceId.ToString()
                .Contains(search, StringComparison.OrdinalIgnoreCase);

            bool matchesType = row.Type
                .Contains(search, StringComparison.OrdinalIgnoreCase);

            bool matchesOwner = row.Owner
                .Contains(search, StringComparison.OrdinalIgnoreCase);

            return matchesId || matchesType || matchesOwner;
        }

        private void OpenDevice(DeviceRow? row)
        {
            if (row == null)
                return;

            SelectedDevice = row;
        }

        private void OpenRegisterDeviceOverlay()
        {
            var overlayVm = new RegisterDeviceViewModel(_deviceDescriptionService, _deviceService);
            ShowOverlay(overlayVm);
        }
    }
}
