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

        // Gets all pending requests for the dashboard
        public IReadOnlyList<RequestDashboardDisplayModel> GetPendingRequests()
        {
            var allRequests = _requestService.GetAllRequests();
            var pendingRequests = allRequests
                .Where(r => r.Status == RequestStatus.PENDING)
                .OrderBy(r => r.NeededByDate)
                .ToList();

            var (allDevices, allLoans, allEmployees, allDescriptions) = LoadAll();

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
                    description = allDescriptions
                        .FirstOrDefault(dd => dd.DeviceDescriptionID == device.DeviceDescriptionID);
                }

                var deviceType = description?.DeviceType ?? "Device";
                var operatingSystem = description?.OperatingSystem ?? "Unknown";
                var headerText = $"REQ-{request.RequestID} · {deviceType} · {operatingSystem}";

                var employeeName = BuildEmployeeName(employee);
                var location = description?.Location ?? "Unknown";
                var dateText = request.NeededByDate.ToString("dd.MM.yyyy");
                //var subText = BuildSubText(employeeName, location, dateText);
                var subText = $"{employeeName} · {location} · Needed by: {dateText} ";
                result.Add(new RequestDashboardDisplayModel
                {
                    RequestID = request.RequestID,
                    HeaderText = headerText,
                    SubText = subText
                });
            }

            return result;
        }

        // Gets devices by their status
        public IReadOnlyList<DeviceDashboardDisplayModel> GetDevicesByStatus(DeviceStatus status)
        {
            var (allDevices, allLoans, allEmployees, allDeviceDescriptions) = LoadAll();

            var result = new List<DeviceDashboardDisplayModel>();

            foreach (var device in allDevices.Where(d => d.Status == status))
            {
                var loan = allLoans.FirstOrDefault(l => l.DeviceID == device.DeviceID);
                var employee = loan is null
                    ? null
                    : allEmployees.FirstOrDefault(e => e.EmployeeID == loan.BorrowerID);

                var description = allDeviceDescriptions
                    .FirstOrDefault(dd => dd.DeviceDescriptionID == device.DeviceDescriptionID);

                var dateText = device.PurchaseDate?.ToString("dd.MM.yyyy") ?? "No date";

                result.Add(MapDevice(device, employee, description, $"Registered: {dateText}"));
            }
            return result;
        }

        // Gets devices currently loaned to employees who have been terminated
        public IReadOnlyList<DeviceDashboardDisplayModel> GetDevicesWithTerminatedBorrowers()
        {
            var (allDevices, allLoans, allEmployees, allDeviceDescriptions) = LoadAll();

            var result = new List<DeviceDashboardDisplayModel>();

            var terminatedBorrowerLoans = new List<(Loan loan, Employee employee)>();

            foreach (var loan in allLoans)
            {
                var employee = allEmployees.FirstOrDefault(e => e.EmployeeID == loan.BorrowerID);
                if (employee is null)
                    continue;

                if (loan.Status != LoanStatus.ACTIVE)
                    continue;

                if (employee.TerminationDate != null)
                {
                    terminatedBorrowerLoans.Add((loan, employee));
                }
            }

            terminatedBorrowerLoans = terminatedBorrowerLoans
                .OrderBy(t => t.employee.TerminationDate)
                .ToList();

            foreach (var item in terminatedBorrowerLoans)
            {
                var loan = item.loan;
                var employee = item.employee;

                var device = allDevices.FirstOrDefault(d => d.DeviceID == loan.DeviceID);
                if (device is null)
                    continue;

                var description = allDeviceDescriptions
                    .FirstOrDefault(dd => dd.DeviceDescriptionID == device.DeviceDescriptionID);

                var terminationDateText = employee.TerminationDate?.ToString("dd.MM.yyyy") ?? "No termination date";

                result.Add(MapDevice(device, employee, description, $"Terminated: {terminationDateText}"));
            }
            return result;
        }

        // Helper method to load all necessary data
        private (List<Device> devices, List<Loan> loans, List<Employee> employees, List<DeviceDescription> descriptions) LoadAll()
        {
            return (
                _deviceService.GetAllDevices().ToList(),
                _loanRepository.GetAll().ToList(),
                _employeeRepository.GetAll().ToList(),
                _deviceDescriptionRepository.GetAll().ToList()
            );
        }

        // Builds the header text for a device
        private string BuildDeviceHeader(Device device, DeviceDescription? desc)
        {
            var type = desc?.DeviceType ?? "Device";
            var os = desc?.OperatingSystem ?? "Unknown";
            return $"DEV-{device.DeviceID} · {type} · {os}";
        }

        // Builds the subtext line
        private string BuildSubText(string employeeName, string location, string dateText)
        {
            return $"{employeeName} · {location} · {dateText}";
        }

        // Builds employee name consistently
        private string BuildEmployeeName(Employee? employee)
        {
            return employee is null
                ? "Unknown"
                : $"{employee.FirstName} {employee.LastName}";
        }

        // Maps a device + employee + description into a DeviceDashboardDisplayModel
        private DeviceDashboardDisplayModel MapDevice(
            Device device,
            Employee? employee,
            DeviceDescription? description,
            string dateText)
        {
            var headerText = BuildDeviceHeader(device, description);
            var employeeName = BuildEmployeeName(employee);
            var location = description?.Location ?? "Unknown";
            var subText = BuildSubText(employeeName, location, dateText);

            return new DeviceDashboardDisplayModel
            {
                DeviceID = device.DeviceID,
                HeaderText = headerText,
                SubText = subText
            };
        }
    }
}
