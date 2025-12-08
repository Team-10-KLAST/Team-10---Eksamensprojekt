using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Application.Interfaces;
using Application.Interfaces.Repository;
using Application.Interfaces.Service;
using Application.Models;
using Application.Models.DisplayModels;
using Presentation.Wpf.Commands;

namespace Presentation.Wpf.ViewModels
{
    // ViewModel for the dashboard page showing requests and device status
    public class DashboardViewModel : OverlayHostViewModel
    {
        private readonly IRequestService _requestService;
        private readonly IDeviceService _deviceService;

        // Repositories used to load additional data needed for display strings
        private readonly IRepository<Loan> _loanRepository;
        private readonly IRepository<Employee> _employeeRepository;
        private readonly IRepository<DeviceDescription> _deviceDescriptionRepository;

        // Collection of Requests with status PENDING
        public ObservableCollection<RequestDashboardDisplayModel> PendingRequests { get; } = new();

        // Collection of Devices with status PLANNED
        public ObservableCollection<DeviceDashboardDisplayModel> PlannedDevices { get; } = new();

        // Collection of Devices with status ORDERED
        public ObservableCollection<DeviceDashboardDisplayModel> OrderedDevices { get; } = new();

        // Collection of Devices with status RECEIVED
        public ObservableCollection<DeviceDashboardDisplayModel> ReceivedDevices { get; } = new();

        // Count of pending requests for displaying "x items"
        public int PendingRequestsCount => PendingRequests.Count;

        // Count of planned devices for displaying "x items"
        public int PlannedDevicesCount => PlannedDevices.Count;

        // Count of ordered devices for displaying "x items"
        public int OrderedDevicesCount => OrderedDevices.Count;

        // Count of received devices for displaying "x items"
        public int ReceivedDevicesCount => ReceivedDevices.Count;

        // Error message shown when loading the dashboard fails
        private string _errorMessage = string.Empty;
        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        // Commands for quick actions
        public ICommand OpenRegisterDeviceCommand { get; }
        public ICommand OpenRegisterEmployeeCommand { get; }

        // Commands for opening overlays for individual items
        public ICommand OpenProcessRequestCommand { get; }
        public ICommand OpenUpdateDeviceCommand { get; }

        // Constructor
        public DashboardViewModel(
            IRequestService requestService,
            IDeviceService deviceService,
            IRepository<Loan> loanRepository,
            IRepository<Employee> employeeRepository,
            IRepository<DeviceDescription> deviceDescriptionRepository)
        {
            _requestService = requestService;
            _deviceService = deviceService;
            _loanRepository = loanRepository;
            _employeeRepository = employeeRepository;
            _deviceDescriptionRepository = deviceDescriptionRepository;

            OpenRegisterDeviceCommand = new RelayCommand(OpenRegisterDeviceOverlay);
            OpenRegisterEmployeeCommand = new RelayCommand(OpenRegisterEmployeeOverlay);

            OpenProcessRequestCommand = new RelayCommand<RequestDashboardDisplayModel>(OpenProcessRequestOverlay);
            OpenUpdateDeviceCommand = new RelayCommand<DeviceDashboardDisplayModel>(OpenUpdateDeviceOverlay);

            LoadDashboardData();
        }

        // Loads requests and devices and splits them into the four dashboard lists
        private void LoadDashboardData()
        {
            try
            {
                PendingRequests.Clear();
                PlannedDevices.Clear();
                OrderedDevices.Clear();
                ReceivedDevices.Clear();

                LoadPendingRequests();
                LoadDevicesByStatus();

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

        // Loads all pending requests and builds formatted header and subtext strings
        private void LoadPendingRequests()
        {
            var allRequests = _requestService.GetAllRequests();
            var pendingRequests = allRequests
                .Where(r => r.Status == RequestStatus.PENDING)
                .OrderBy(r => r.NeededByDate)
                .ToList();
            var allLoans = _loanRepository.GetAll().ToList();
            var allEmployees = _employeeRepository.GetAll().ToList();
            var allDeviceDescriptions = _deviceDescriptionRepository.GetAll().ToList();
            var allDevices = _deviceService.GetAllDevices().ToList();

            foreach (var request in pendingRequests)
            {
                // Only show requests with status PENDING
                if (request.Status != RequestStatus.PENDING)
                    continue;

                // Find loan connected to this request
                var loan = allLoans.FirstOrDefault(l => l.RequestID == request.RequestID);
                if (loan is null)
                    continue;

                // Find employee (borrower)
                var employee = allEmployees.FirstOrDefault(e => e.EmployeeID == loan.BorrowerID);

                // Find device and description
                var device = allDevices.FirstOrDefault(d => d.DeviceID == loan.DeviceID);
                DeviceDescription? description = null;
                if (device is not null)
                {
                    description = allDeviceDescriptions
                        .FirstOrDefault(dd => dd.DeviceDescriptionID == device.DeviceDescriptionID);
                }

                // Build device-related part of the header
                var deviceType = description?.DeviceType ?? "Device";
                var operatingSystem = description?.OperatingSystem ?? "Unknown";

                var headerText = $"REQ-{request.RequestID} · {deviceType} · {operatingSystem}";

                // Build employee + location + date for the subtext
                var employeeName = employee is null
                    ? "Unknown"
                    : $"{employee.FirstName} {employee.LastName}";

                var location = description?.Location ?? "Unknown";

                var dateText = request.NeededByDate.ToString("dd.MM.yyyy");

                var subText = $"{employeeName} · {location} · {dateText}";

                var displayModel = new RequestDashboardDisplayModel
                {
                    RequestID = request.RequestID,
                    RequestDate = request.RequestDate,
                    Justification = request.Justification,
                    Status = request.Status,
                    HeaderText = headerText,
                    SubText = subText
                };

                PendingRequests.Add(displayModel);
            }
        }

        // Loads all devices and splits them into Planned, Ordered and Received lists
        private void LoadDevicesByStatus()
        {
            var allDevices = _deviceService.GetAllDevices().ToList();
            var allLoans = _loanRepository.GetAll().ToList();
            var allEmployees = _employeeRepository.GetAll().ToList();
            var allDeviceDescriptions = _deviceDescriptionRepository.GetAll().ToList();

            foreach (var device in allDevices)
            {
                // Find loan for this device (used to get borrower/employee)
                var loan = allLoans.FirstOrDefault(l => l.DeviceID == device.DeviceID);
                var employee = loan is null
                    ? null
                    : allEmployees.FirstOrDefault(e => e.EmployeeID == loan.BorrowerID);

                // Find description for type, OS and location
                var description = allDeviceDescriptions
                    .FirstOrDefault(dd => dd.DeviceDescriptionID == device.DeviceDescriptionID);

                // Build header: DEV-ID · Type · OS
                var deviceType = description?.DeviceType ?? "Device";
                var operatingSystem = description?.OperatingSystem ?? "Unknown";
                var headerText = $"DEV-{device.DeviceID} · {deviceType} · {operatingSystem}";

                // Build subtext: Employee · Location · Date
                var employeeName = employee is null
                    ? "Unknown"
                    : $"{employee.FirstName} {employee.LastName}";

                var location = description?.Location ?? "Unknown";
                var dateText = device.PurchaseDate.ToString("dd.MM.yyyy");

                var subText = $"{employeeName} · {location} · {dateText}";

                var displayModel = new DeviceDashboardDisplayModel
                {
                    DeviceID = device.DeviceID,
                    Status = device.Status,
                    Price = device.Price,
                    PurchaseDate = device.PurchaseDate,
                    ExpectedEndDate = device.ExpectedEndDate,
                    HeaderText = headerText,
                    SubText = subText
                };

                switch (device.Status)
                {
                    case DeviceStatus.PLANNED:
                        PlannedDevices.Add(displayModel);
                        break;
                    case DeviceStatus.ORDERED:
                        OrderedDevices.Add(displayModel);
                        break;
                    case DeviceStatus.RECEIVED:
                        ReceivedDevices.Add(displayModel);
                        break;
                }
            }
        }


        // Handles showing an overlay and reloading dashboard data when the overlay is closed
        private void ShowOverlayAndReload(OverlayPanelViewModelBase overlayViewModel)
        {
            overlayViewModel.RequestClose += (sender, eventArgs) =>
            {
                CurrentOverlay = null;
                LoadDashboardData();
            };
            ShowOverlay(overlayViewModel);
        }

        // Opens the Register Device overlay
        private void OpenRegisterDeviceOverlay()
        {
            // TODO: Replace with actual RegisterDeviceViewModel when available
            // ShowOverlayAndReload(new RegisterDeviceViewModel(...));
        }

        // Opens the Register Employee overlay
        private void OpenRegisterEmployeeOverlay()
        {
            // TODO: Replace with actual AddEmployeeViewModel or similar when available
            // ShowOverlayAndReload(new AddEmployeeViewModel(...));
        }

        // Opens the Process Request overlay for the selected pending request
        private void OpenProcessRequestOverlay(RequestDashboardDisplayModel requestDisplayModel)
        {
            if (requestDisplayModel is null)
                return;

            // TODO: Replace with actual ProcessRequestViewModel implementation
            // var overlay = new ProcessRequestViewModel(requestDisplayModel.RequestID, _requestService, _deviceService);
            // ShowOverlayAndReload(overlay);
        }

        // Opens the Update Device overlay for the selected device
        private void OpenUpdateDeviceOverlay(DeviceDashboardDisplayModel deviceDisplayModel)
        {
            if (deviceDisplayModel is null)
                return;

            // TODO: Replace with actual NewUpdateDeviceViewModel implementation
            // var overlay = new NewUpdateDeviceViewModel(deviceDisplayModel.DeviceID, _deviceService);
            // ShowOverlayAndReload(overlay);
        }
    }
}
