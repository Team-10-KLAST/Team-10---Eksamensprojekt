using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Application.Interfaces;
using Application.Interfaces.Service;
using Application.Models;
using Application.Models.DisplayModels;
using Presentation.Wpf.Commands;

namespace Presentation.Wpf.ViewModels
{
    // ViewModel for the dashboard page showing requests and device status
    public class DashboardViewModel : OverlayHostViewModel
    {
        // Services
        private readonly IDashboardService _dashboardService;
        private readonly IRequestService _requestService;
        private readonly IDeviceService _deviceService;
        private readonly IEmployeeService _employeeService;
        private readonly IDeviceDescriptionService _deviceDescriptionService;

        // Collection of status cards for the dashboard
        public ObservableCollection<StatusCardViewModel> StatusCards { get; } = new();

        // Observable collections for each dashboard section
        public ObservableCollection<RequestDashboardDisplayModel> PendingRequests { get; } = new();
        public ObservableCollection<DeviceDashboardDisplayModel> PlannedDevices { get; } = new();
        public ObservableCollection<DeviceDashboardDisplayModel> OrderedDevices { get; } = new();
        public ObservableCollection<DeviceDashboardDisplayModel> ReceivedDevices { get; } = new();
        public ObservableCollection<DeviceDashboardDisplayModel> TerminatedBorrowerDevices { get; } = new();

        // Count properties for each collection
        public int PendingRequestsCount => PendingRequests.Count;
        public int PlannedDevicesCount => PlannedDevices.Count;
        public int OrderedDevicesCount => OrderedDevices.Count;
        public int ReceivedDevicesCount => ReceivedDevices.Count;
        public int TerminatedBorrowerDevicesCount => TerminatedBorrowerDevices.Count;

        // Error message property for displaying load errors
        private string _errorMessage = string.Empty;
        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        // Commands
        public ICommand OpenRegisterDeviceCommand { get; }
        public ICommand OpenRegisterEmployeeCommand { get; }
        public ICommand OpenProcessRequestCommand { get; }
        public ICommand OpenUpdateDeviceCommand { get; }
        public ICommand RefreshDashboardCommand { get; }

        // Constructor
        public DashboardViewModel(
            IDashboardService dashboardService,
            IRequestService requestService,
            IDeviceService deviceService,
            IEmployeeService employeeService,
            IDeviceDescriptionService deviceDescriptionService)
        {
            _dashboardService = dashboardService;
            _requestService = requestService;
            _deviceService = deviceService;
            _employeeService = employeeService;
            _deviceDescriptionService = deviceDescriptionService;

            OpenRegisterDeviceCommand = new RelayCommand(OpenRegisterDeviceOverlay);
            OpenRegisterEmployeeCommand = new RelayCommand(OpenRegisterEmployeeOverlay);
            OpenProcessRequestCommand = new RelayCommand<RequestDashboardDisplayModel>(OpenProcessRequestOverlay);
            OpenUpdateDeviceCommand = new RelayCommand<DeviceDashboardDisplayModel>(OpenUpdateDeviceOverlay);
            RefreshDashboardCommand = new RelayCommand(LoadDashboardData);

            LoadDashboardData();
        }

        // Loads data for the dashboard and handles errors
        private void LoadDashboardData()
        {
            try
            {
                LoadCollections();
                BuildStatusCards();
                ErrorMessage = string.Empty;
            }
            catch (Exception)
            {
                ErrorMessage = "Unable to load dashboard data. Please try again later.";
            }
        }

        // Loads data into the observable collections
        private void LoadCollections()
        {
            PendingRequests.Clear();
            PlannedDevices.Clear();
            OrderedDevices.Clear();
            ReceivedDevices.Clear();
            TerminatedBorrowerDevices.Clear();

            foreach (var r in _dashboardService.GetPendingRequests())
                PendingRequests.Add(r);

            foreach (var d in _dashboardService.GetDevicesByStatus(DeviceStatus.PLANNED))
                PlannedDevices.Add(d);

            foreach (var d in _dashboardService.GetDevicesByStatus(DeviceStatus.ORDERED))
                OrderedDevices.Add(d);

            foreach (var d in _dashboardService.GetDevicesByStatus(DeviceStatus.RECEIVED))
                ReceivedDevices.Add(d);

            foreach (var d in _dashboardService.GetDevicesWithTerminatedBorrowers())
                TerminatedBorrowerDevices.Add(d);

            OnPropertyChanged(nameof(PendingRequestsCount));
            OnPropertyChanged(nameof(PlannedDevicesCount));
            OnPropertyChanged(nameof(OrderedDevicesCount));
            OnPropertyChanged(nameof(ReceivedDevicesCount));
            OnPropertyChanged(nameof(TerminatedBorrowerDevicesCount));
        }

        // Helper method to create and add a status card
        private void CreateCard(string title, string icon, IEnumerable items, int count, ICommand command)
        {
            StatusCards.Add(new StatusCardViewModel
            {
                Title = title,
                Icon = icon,
                Items = items,
                Count = count,
                Command = command
            });
        }

        // Builds the status cards for the dashboard
        private void BuildStatusCards()
        {
            StatusCards.Clear();

            CreateCard("Pending requests", "🖥", PendingRequests, PendingRequestsCount, OpenProcessRequestCommand);
            CreateCard("Planned", "📅", PlannedDevices, PlannedDevicesCount, OpenUpdateDeviceCommand);
            CreateCard("Ordered", "📦", OrderedDevices, OrderedDevicesCount, OpenUpdateDeviceCommand);
            CreateCard("Ready to assign", "✔", ReceivedDevices, ReceivedDevicesCount, OpenUpdateDeviceCommand);
            CreateCard("Terminated borrowers", "⚠", TerminatedBorrowerDevices, TerminatedBorrowerDevicesCount, OpenUpdateDeviceCommand);
        }

        // Shows an overlay and reloads dashboard data when the overlay is closed
        private void ShowOverlayAndReload(OverlayPanelViewModelBase overlayViewModel)
        {
            overlayViewModel.RequestClose += (sender, eventArgs) =>
            {
                CurrentOverlay = null;
                LoadDashboardData();
            };
            ShowOverlay(overlayViewModel);
        }

        // Opens the register device overlay
        private void OpenRegisterDeviceOverlay()
        {
            var overlay = new RegisterDeviceViewModel(
                _deviceDescriptionService,
                _deviceService);

            ShowOverlayAndReload(overlay);
        }

        // Opens the register employee overlay
        private void OpenRegisterEmployeeOverlay()
        {
            var overlay = new AddEmployeeViewModel(_employeeService);
            ShowOverlayAndReload(overlay);
        }

        // Opens the process request overlay
        private void OpenProcessRequestOverlay(RequestDashboardDisplayModel requestDisplayModel)
        {
            if (requestDisplayModel is null)
                return;

            var overlay = new ProcessRequestViewModel(
                _requestService,
                _employeeService,
                requestDisplayModel.RequestID);

            ShowOverlayAndReload(overlay);
        }

        // Opens the update device overlay
        private void OpenUpdateDeviceOverlay(DeviceDashboardDisplayModel deviceDisplayModel)
        {
            if (deviceDisplayModel is null)
                return;

            var deviceDisplay = _deviceService.GetDeviceDisplayByID(deviceDisplayModel.DeviceID);

            if (deviceDisplay == null)
            {
                ErrorMessage = "Could not load device from database.";
                return;
            }
            var overlay = new UpdateDeviceViewModel(deviceDisplay, _deviceService);
            ShowOverlayAndReload(overlay);
        }

        // Refreshes the dashboard data. Used in navigation.
        public void Refresh()
        {
            LoadDashboardData();
        }
    }
}
