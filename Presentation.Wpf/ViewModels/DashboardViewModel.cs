using System;
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
        private readonly IDashboardService _dashboardService;
        private readonly IRequestService _requestService;
        private readonly IDeviceService _deviceService;
        private readonly IEmployeeService _employeeService;
        private readonly IDeviceDescriptionService _deviceDescriptionService;

        public ObservableCollection<RequestDashboardDisplayModel> PendingRequests { get; } = new();
        public ObservableCollection<DeviceDashboardDisplayModel> PlannedDevices { get; } = new();
        public ObservableCollection<DeviceDashboardDisplayModel> OrderedDevices { get; } = new();
        public ObservableCollection<DeviceDashboardDisplayModel> ReceivedDevices { get; } = new();

        public int PendingRequestsCount => PendingRequests.Count;
        public int PlannedDevicesCount => PlannedDevices.Count;
        public int OrderedDevicesCount => OrderedDevices.Count;
        public int ReceivedDevicesCount => ReceivedDevices.Count;

        private string _errorMessage = string.Empty;
        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public ICommand OpenRegisterDeviceCommand { get; }
        public ICommand OpenRegisterEmployeeCommand { get; }
        public ICommand OpenProcessRequestCommand { get; }
        public ICommand OpenUpdateDeviceCommand { get; }
        public ICommand RefreshDashboardCommand { get; }

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

        private void LoadDashboardData()
        {
            try
            {
                PendingRequests.Clear();
                PlannedDevices.Clear();
                OrderedDevices.Clear();
                ReceivedDevices.Clear();

                var pending = _dashboardService.GetPendingRequests();
                var planned = _dashboardService.GetDevicesByStatus(DeviceStatus.PLANNED);
                var ordered = _dashboardService.GetDevicesByStatus(DeviceStatus.ORDERED);
                var received = _dashboardService.GetDevicesByStatus(DeviceStatus.RECEIVED);

                foreach (var r in pending)
                    PendingRequests.Add(r);

                foreach (var d in planned)
                    PlannedDevices.Add(d);

                foreach (var d in ordered)
                    OrderedDevices.Add(d);

                foreach (var d in received)
                    ReceivedDevices.Add(d);

                OnPropertyChanged(nameof(PendingRequestsCount));
                OnPropertyChanged(nameof(PlannedDevicesCount));
                OnPropertyChanged(nameof(OrderedDevicesCount));
                OnPropertyChanged(nameof(ReceivedDevicesCount));

                ErrorMessage = string.Empty;
            }
            catch (Exception)
            {
                ErrorMessage = "Unable to load dashboard data. Please try again later.";
            }
        }

        private void ShowOverlayAndReload(OverlayPanelViewModelBase overlayViewModel)
        {
            overlayViewModel.RequestClose += (sender, eventArgs) =>
            {
                CurrentOverlay = null;
                LoadDashboardData();
            };
            ShowOverlay(overlayViewModel);
        }

       

        private void OpenRegisterDeviceOverlay()
        {
            var overlay = new RegisterDeviceViewModel(
                _deviceDescriptionService,
                _deviceService);

            ShowOverlayAndReload(overlay);
        }

        private void OpenRegisterEmployeeOverlay()
        {
            var overlay = new AddEmployeeViewModel(_employeeService);
            ShowOverlayAndReload(overlay);
        }

        

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



        private void OpenUpdateDeviceOverlay(DeviceDashboardDisplayModel deviceDisplayModel)
        {
            if (deviceDisplayModel is null)
                return;

            var deviceDisplay = _deviceService.GetDeviceDisplayByID(deviceDisplayModel.DeviceID);

            if (deviceDisplay == null)
            {
                ErrorMessage = "Kunne ikke hente device fra databasen.";
                return;
            }

            var overlay = new UpdateDeviceViewModel(deviceDisplay);
            ShowOverlayAndReload(overlay);
        }

    }
}
