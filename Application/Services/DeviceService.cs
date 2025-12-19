using System;
using System.Collections.Generic;
using System.Linq; // Needed for FirstOrDefault
using Application.Interfaces.Repository;
using Application.Interfaces.Service;
using Application.Models;
using Application.Models.DisplayModels;

namespace Application.Services
{
    // Implements the device service interface to provide business logic for device management
    public class DeviceService : IDeviceService
    {
        // Used to load, create, update and delete devices in the database
        private readonly IRepository<Device> _deviceRepository;
        private readonly IDeviceDescriptionService _deviceDescriptionService;

        // Repositories used to resolve current owner (loan + employee)
        private readonly IRepository<Loan> _loanRepository;
        private readonly IRepository<Employee> _employeeRepository;

        private readonly IRepository<Request> _requestRepository;

        // Receives the repository instances from dependency injection
        public DeviceService(
            IRepository<Device> deviceRepository,
            IDeviceDescriptionService deviceDescriptionService,
            IRepository<Loan> loanRepository,
            IRepository<Employee> employeeRepository,
            IRepository<Request> requestRepository)
        {
            _deviceRepository = deviceRepository;
            _deviceDescriptionService = deviceDescriptionService;
            _loanRepository = loanRepository;
            _employeeRepository = employeeRepository;
            _requestRepository = requestRepository;
        }

        // Gets one device by its DeviceID, or null if it does not exist
        public Device? GetDeviceByID(int deviceID)
        {
            // Guards against invalid DeviceID values
            if (deviceID <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(deviceID), "DeviceID must be greater than zero.");
            }

            return _deviceRepository.GetByID(deviceID);
        }

        //Display a device with additional information like loan, borrower, neededbydate, 
        public DeviceDisplayModel? GetDeviceDisplayByID(int deviceID)
        {
            var device = GetDeviceByID(deviceID);
            if (device == null)
                return null;

            var description = _deviceDescriptionService.GetByID(device.DeviceDescriptionID)
                              ?? throw new InvalidOperationException("Could not find description for device.");
                        
            var loan = _loanRepository.GetAll()
                .Where(loan => loan.DeviceID == device.DeviceID)
                .OrderByDescending(l => l.LoanID)
                .FirstOrDefault();

            var employee = loan is null
                ? null
                : _employeeRepository.GetByID(loan.BorrowerID);

            var ownerFullName = employee is null
                ? string.Empty
                : $"{employee.FirstName} {employee.LastName}";

            Request? request = (loan?.RequestID is int requestId)
                ? _requestRepository.GetByID(requestId)
                : null;

            DateTime? neededByDate = request?.NeededByDate.ToDateTime(new TimeOnly(0, 0));

            DateTime? registrationDate = device.PurchaseDate?.ToDateTime(new TimeOnly(0, 0));
            DateTime? expirationDate = device.ExpectedEndDate?.ToDateTime(new TimeOnly(0, 0));

            return new DeviceDisplayModel
            {
                DeviceID = device.DeviceID,
                Type = description.DeviceType,
                OS = description.OperatingSystem,
                Location = description.Location,
                Status = device.Status.ToString(),
                RegistrationDate = registrationDate,
                ExpirationDate = expirationDate,
                NeededByDate = neededByDate,
                Wiped = device.IsWiped,
                OwnerFullName = ownerFullName
            };
        }

        // Gets all devices in the system
        public IEnumerable<Device> GetAllDevices()
        {
            return _deviceRepository.GetAll();
        }

        // Creates a new device in the system
        public void AddDevice(Device device)
        {
            if (device is null)
            {
                throw new ArgumentNullException(nameof(device));
            }

            device.Status = DeviceStatus.INSTOCK;

            _deviceRepository.Add(device);
        }

        // Updates an existing device
        public void UpdateDevice(Device device)
        {
            if (device is null)
                throw new ArgumentNullException(nameof(device));

            if (device.DeviceID <= 0)
                throw new ArgumentOutOfRangeException(nameof(device.DeviceID), "DeviceID must be greater than zero.");

            if (device.Status == DeviceStatus.RECEIVED && device.PurchaseDate == default)
            {
                device.PurchaseDate = DateOnly.FromDateTime(DateTime.Today);
                var expiry = CalculateDefaultExpiryDate(DateTime.Today);
                device.ExpectedEndDate = DateOnly.FromDateTime(expiry);
            }

            if (device.Status == DeviceStatus.INSTOCK && !device.IsWiped)
            {
                throw new InvalidOperationException(
                    "The device must be wiped before it can be set to 'In stock'.");
            }

            if (device.Status == DeviceStatus.INSTOCK)
            {
                var activeLoan = _loanRepository
                    .GetAll()
                    .FirstOrDefault(loan => loan.DeviceID == device.DeviceID &&
                                         loan.Status == LoanStatus.ACTIVE);

                if (activeLoan != null)
                {
                    activeLoan.Status = LoanStatus.INACTIVE;
                    activeLoan.EndDate = DateOnly.FromDateTime(DateTime.Now);

                    _loanRepository.Update(activeLoan);
                }
            }
            _deviceRepository.Update(device);
        }

        //UpdateDevice with information from DeviceDisplayModel
        public void UpdateDevice(DeviceDisplayModel updatedDevice)
        {
            if (updatedDevice is null)
                throw new ArgumentNullException(nameof(updatedDevice));

            var device = _deviceRepository.GetByID(updatedDevice.DeviceID);
            if (device is null)
                throw new KeyNotFoundException($"Device with ID {updatedDevice.DeviceID} not found.");

            if (!Enum.TryParse<DeviceStatus>(updatedDevice.Status, out var status))
                throw new ArgumentException("Invalid status value on updatedDevice.", nameof(updatedDevice));

            device.Status = status;

            device.IsWiped = updatedDevice.Wiped;

            device.PurchaseDate = updatedDevice.RegistrationDate.HasValue
                ? DateOnly.FromDateTime(updatedDevice.RegistrationDate.Value)
                : null;

            device.ExpectedEndDate = updatedDevice.ExpirationDate.HasValue
                ? DateOnly.FromDateTime(updatedDevice.ExpirationDate.Value)
                : null;

            UpdateDevice(device);
        }

        //Create a virtual device for new request
        public int CreateVirtualDeviceID(string DeviceType, string OS, string country)
        {
            Device device = new Device
            {
                DeviceDescriptionID = _deviceDescriptionService.GetDeviceDescriptionID(DeviceType, OS, country),
                Status = DeviceStatus.REGISTERED,
                IsWiped = false
            };
            AddDevice(device);
            return device.DeviceID;
        }
        

        // Gets all available device display models, optionally filtered by device type
        public IEnumerable<DeviceDisplayModel> GetAvailableDeviceDisplayModels(string? deviceType)
        {
            var devices = _deviceRepository
                .GetAll()
                .Where(d => d.Status == DeviceStatus.INSTOCK);

            if (!string.IsNullOrWhiteSpace(deviceType))
            {
                devices = devices.Where(d =>
                {
                    var desc = _deviceDescriptionService.GetByID(d.DeviceDescriptionID);
                    return desc != null &&
                           desc.DeviceType.Equals(deviceType, StringComparison.OrdinalIgnoreCase);
                });
            }

            var descriptions = _deviceDescriptionService
                .GetAllDescriptions()
                .ToDictionary(d => d.DeviceDescriptionID);

            return devices.Select(device =>
            {
                var desc = descriptions[device.DeviceDescriptionID];
                return MapToDisplayModel(device, desc);
            });
        }

        // Gets all device display models for the deviceviewmodel
        public IEnumerable<DeviceDisplayModel> GetAllDeviceDisplayModels()
        {
            var devices = _deviceRepository.GetAll();

            foreach (var device in devices)
            {
                var description = _deviceDescriptionService.GetByID(device.DeviceDescriptionID);
                if (description == null)
                    continue;

                var loan = _loanRepository.GetAll()
                    .Where(loan => loan.DeviceID == device.DeviceID)
                    .OrderByDescending(loan => loan.LoanID)
                    .FirstOrDefault();

                var employee = loan != null
                    ? _employeeRepository.GetByID(loan.BorrowerID)
                    : null;

                string ownerEmail =
                    loan != null
                    && device.Status != DeviceStatus.INSTOCK
                    && device.Status != DeviceStatus.CANCELLED
                        ? employee?.Email ?? string.Empty
                        : string.Empty;

                yield return new DeviceDisplayModel
                {
                    DeviceID = device.DeviceID,
                    Type = description.DeviceType,
                    OS = description.OperatingSystem,
                    Location = description.Location,
                    Status = device.Status.ToString(),
                    RegistrationDate = device.PurchaseDate?.ToDateTime(TimeOnly.MinValue),
                    ExpirationDate = device.ExpectedEndDate?.ToDateTime(TimeOnly.MinValue),
                    OwnerEmail = ownerEmail
                };
            }
        }

        // Helper method to map Device and DeviceDescription to DeviceDisplayModel
        private DeviceDisplayModel MapToDisplayModel(Device device, DeviceDescription desc)
        {
            return new DeviceDisplayModel
            {
                DeviceID = device.DeviceID,
                Type = desc.DeviceType,
                OS = desc.OperatingSystem,
                Location = desc.Location,
                Status = device.Status.ToString(),
                RegistrationDate = device.PurchaseDate?.ToDateTime(TimeOnly.MinValue),
                ExpirationDate = device.ExpectedEndDate?.ToDateTime(TimeOnly.MinValue)
            };
        }

        public DateTime CalculateDefaultExpiryDate(DateTime registrationDate)
        => registrationDate.AddYears(3);
    }
}
