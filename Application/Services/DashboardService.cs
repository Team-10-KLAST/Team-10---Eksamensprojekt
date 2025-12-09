using System.Collections.Generic;
using System.Linq;
using Application.Interfaces;
using Application.Interfaces.Repository;
using Application.Interfaces.Service;
using Application.Models;
using Application.Models.DisplayModels;

namespace Application.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly IRequestService _requestService;
        private readonly IDeviceService _deviceService;
        private readonly IRepository<Loan> _loanRepository;
        private readonly IRepository<Employee> _employeeRepository;
        private readonly IRepository<DeviceDescription> _deviceDescriptionRepository;

        public DashboardService(
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
        }

        public IReadOnlyList<RequestDashboardDisplayModel> GetPendingRequests()
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

            var result = new List<RequestDashboardDisplayModel>();

            foreach (var request in pendingRequests)
            {
                var loan = allLoans.FirstOrDefault(l => l.RequestID == request.RequestID);
                if (loan is null)
                    continue;

                var employee = allEmployees.FirstOrDefault(e => e.EmployeeID == loan.BorrowerID);

                var device = allDevices.FirstOrDefault(d => d.DeviceID == loan.DeviceID);
                DeviceDescription? description = null;
                if (device is not null)
                {
                    description = allDeviceDescriptions
                        .FirstOrDefault(dd => dd.DeviceDescriptionID == device.DeviceDescriptionID);
                }

                var deviceType = description?.DeviceType ?? "Device";
                var operatingSystem = description?.OperatingSystem ?? "Unknown";
                var headerText = $"REQ-{request.RequestID} · {deviceType} · {operatingSystem}";

                var employeeName = employee is null
                    ? "Unknown"
                    : $"{employee.FirstName} {employee.LastName}";

                var location = description?.Location ?? "Unknown";
                var dateText = request.NeededByDate.ToString("dd.MM.yyyy");
                var subText = $"{employeeName} · {location} · {dateText}";

                result.Add(new RequestDashboardDisplayModel
                {
                    RequestID = request.RequestID,
                    HeaderText = headerText,
                    SubText = subText
                });
            }

            return result;
        }


        public IReadOnlyList<DeviceDashboardDisplayModel> GetDevicesByStatus(DeviceStatus status)
        {
            var allDevices = _deviceService.GetAllDevices().ToList();
            var allLoans = _loanRepository.GetAll().ToList();
            var allEmployees = _employeeRepository.GetAll().ToList();
            var allDeviceDescriptions = _deviceDescriptionRepository.GetAll().ToList();

            var result = new List<DeviceDashboardDisplayModel>();

            foreach (var device in allDevices.Where(d => d.Status == status))
            {
                var loan = allLoans.FirstOrDefault(l => l.DeviceID == device.DeviceID);
                var employee = loan is null
                    ? null
                    : allEmployees.FirstOrDefault(e => e.EmployeeID == loan.BorrowerID);

                var description = allDeviceDescriptions
                    .FirstOrDefault(dd => dd.DeviceDescriptionID == device.DeviceDescriptionID);

                var deviceType = description?.DeviceType ?? "Device";
                var operatingSystem = description?.OperatingSystem ?? "Unknown";
                var headerText = $"DEV-{device.DeviceID} · {deviceType} · {operatingSystem}";

                var employeeName = employee is null
                    ? "Unknown"
                    : $"{employee.FirstName} {employee.LastName}";

                var location = description?.Location ?? "Unknown";
                var dateText = device.PurchaseDate.ToString("dd.MM.yyyy");
                var subText = $"{employeeName} · {location} · {dateText}";

                result.Add(new DeviceDashboardDisplayModel
                {
                    DeviceID = device.DeviceID,
                    HeaderText = headerText,
                    SubText = subText
                });
            }

            return result;
        }

    }
}
