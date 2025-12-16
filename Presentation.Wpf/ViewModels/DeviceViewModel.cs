using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Numerics;
using System.Windows.Data;
using System.Windows.Input;
using Application.Interfaces;
using Application.Interfaces.Service;
using Application.Models;
using Application.Models.DisplayModels;
using Application.Services;
using Presentation.Wpf.Commands;
using Presentation.Wpf.Views;

namespace Presentation.Wpf.ViewModels
{
    // ViewModel for the device view
    public class DeviceViewModel : OverlayHostViewModel
    {
        // Service used to retrieve devices from the database
        private readonly IDeviceService _deviceService;
        private readonly IDeviceDescriptionService _deviceDescriptionService;

        // Collection of all devices (unfiltered)
        public ObservableCollection<DeviceDisplayModel> Devices { get; } = new();

        // Collectionview for filtered devices
        public ICollectionView DevicesView { get; }

        // Device types for filtering
        public ObservableCollection<string> DeviceTypes { get; } = new();

        // Device status options for filtering
        public ObservableCollection<string> DeviceStatusOptions { get; } = new();

        // Search text
        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                    DevicesView.Refresh();
            }
        }

        // Selected device type
        private const string AllDeviceTypes = "Device Type";
        private string _selectedDeviceType = AllDeviceTypes;
        public string SelectedDeviceType
        {
            get => _selectedDeviceType;
            set
            {
                if (SetProperty(ref _selectedDeviceType, value))
                    DevicesView.Refresh();
            }
        }

        // Selected status
        private const string AllStatuses = "Status";
        private string _selectedStatus = AllStatuses;
        public string SelectedStatus
        {
            get => _selectedStatus;
            set
            {
                if (SetProperty(ref _selectedStatus, value))
                    DevicesView.Refresh();
            }
        }

        // Commands
        public ICommand RefreshCommand { get; }
        public ICommand OpenDeviceCommand { get; }
        public ICommand RegisterDeviceCommand { get; }

        // Constructor
        public DeviceViewModel(IDeviceService deviceService, IDeviceDescriptionService deviceDescriptionService)
        {
            _deviceService = deviceService;
            _deviceDescriptionService = deviceDescriptionService;

            DevicesView = CollectionViewSource.GetDefaultView(Devices);
            DevicesView.Filter = DeviceFilter;

            RefreshCommand = new RelayCommand(LoadDevices);
            OpenDeviceCommand = new RelayCommand<DeviceDisplayModel>(OpenDevice);
            RegisterDeviceCommand = new RelayCommand(OpenRegisterDeviceOverlay);

            LoadDevices();
        }

        // Loads devices from the service into the Devices collection
        private void LoadDevices()
        {
            Devices.Clear();
            foreach (var d in _deviceService.GetAllDeviceDisplayModels())
                Devices.Add(d);

            DeviceTypes.Clear();
            DeviceTypes.Add(AllDeviceTypes);
            foreach (var type in _deviceDescriptionService.GetAllDeviceTypeOptions())
                DeviceTypes.Add(type);

            DeviceStatusOptions.Clear();
            DeviceStatusOptions.Add(AllStatuses);
            foreach (var status in Enum.GetNames(typeof(DeviceStatus)))
                DeviceStatusOptions.Add(status);

            SelectedDeviceType = AllDeviceTypes;
            SelectedStatus = AllStatuses;

            DevicesView.Refresh();
        }

        // Public method to refresh the device list
        public void Refresh()
        {
            LoadDevices();
        }

        // Defines how the Devices collection is filtered based on SearchText & combobox
        private bool DeviceFilter(object obj)
        {
            if (obj is not DeviceDisplayModel row) return false;
            var comparison = StringComparison.OrdinalIgnoreCase;

            bool matchesSearch =
            string.IsNullOrWhiteSpace(SearchText)
            || row.DeviceID.ToString().Contains(SearchText, comparison)
            || row.Type.Contains(SearchText, comparison)
            || row.OS.Contains(SearchText, comparison)
            || row.Location.Contains(SearchText, comparison)
            || row.OwnerEmail.Contains(SearchText, comparison);

            bool matchesType =
                SelectedDeviceType == AllDeviceTypes
                || string.Equals(row.Type, SelectedDeviceType, comparison);

            bool matchesStatus =
                SelectedStatus == AllStatuses
                || string.Equals(row.Status, SelectedStatus, comparison);

            return matchesSearch && matchesType && matchesStatus;
        }

        // Opens the device update overlay for the selected device
        private void OpenDevice(DeviceDisplayModel? row)
        {
            if (row == null)
                return;

            var deviceDisplayModel = _deviceService.GetDeviceDisplayByID(row.DeviceID);
            if (deviceDisplayModel == null)
                return;

            ShowOverlay(new UpdateDeviceViewModel(deviceDisplayModel, _deviceService));
        }

        // Opens the register device overlay
        private void OpenRegisterDeviceOverlay()
        {
            ShowOverlay(new RegisterDeviceViewModel(_deviceDescriptionService, _deviceService));
        }
    }
}
