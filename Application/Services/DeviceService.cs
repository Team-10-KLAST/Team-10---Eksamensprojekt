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

        public DeviceDisplayModel? GetDeviceDisplayByID(int deviceID)
        {
            var device = GetDeviceByID(deviceID);
            if (device == null)
                return null;

            var description = _deviceDescriptionService.GetByID(device.DeviceDescriptionID)
                              ?? throw new InvalidOperationException("Could not find description for device.");

            // Find loan for this device
            var loan = _loanRepository.GetAll()
                .FirstOrDefault(l => l.DeviceID == device.DeviceID);

            // Find borrower
            var employee = loan is null
                ? null
                : _employeeRepository.GetByID(loan.BorrowerID);

            var ownerFullName = employee is null
                ? string.Empty
                : $"{employee.FirstName} {employee.LastName}";

            // Find original request to get NeededByDate
            Request? request = loan is null
                ? null
                : _requestRepository.GetByID(loan.RequestID);

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
                OwnerFullName = ownerFullName,
                StatusHistory = new List<string>()
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

            _deviceRepository.Add(device);
        }

        // Updates an existing device
        public void UpdateDevice(Device device)
        {
            if (device is null)
                throw new ArgumentNullException(nameof(device));

            if (device.DeviceID <= 0)
                throw new ArgumentOutOfRangeException(nameof(device.DeviceID), "DeviceID must be greater than zero.");

            // Business rule: device must be wiped before it can be set to 'In stock'
            if (device.Status == DeviceStatus.INSTOCK && !device.IsWiped)
            {
                throw new InvalidOperationException(
                    "The device must be wiped before it can be set to 'In stock'.");
            }

            _deviceRepository.Update(device);
        }

        public void UpdateDevice(DeviceDisplayModel updatedDevice)
        {
            if (updatedDevice is null)
                throw new ArgumentNullException(nameof(updatedDevice));

            // Load the real Device from the repository
            var device = _deviceRepository.GetByID(updatedDevice.DeviceID);
            if (device is null)
                throw new KeyNotFoundException($"Device with ID {updatedDevice.DeviceID} not found.");

            // Map mutable fields from display model back to entity

            // Status
            if (!Enum.TryParse<DeviceStatus>(updatedDevice.Status, out var status))
                throw new ArgumentException("Invalid status value on updatedDevice.", nameof(updatedDevice));

            device.Status = status;

            // Wiped flag
            device.IsWiped = updatedDevice.Wiped;

            // Dates 
            device.PurchaseDate = updatedDevice.RegistrationDate.HasValue
                ? DateOnly.FromDateTime(updatedDevice.RegistrationDate.Value)
                : null;

            device.ExpectedEndDate = updatedDevice.ExpirationDate.HasValue
                ? DateOnly.FromDateTime(updatedDevice.ExpirationDate.Value)
                : null;

            // Reuse existing validation and persistence logic
            UpdateDevice(device);
        }


        // Deletes a device by its DeviceID
        public void DeleteDevice(int deviceID)
        {
            if (deviceID <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(deviceID), "DeviceID must be greater than zero.");
            }

            _deviceRepository.Delete(deviceID);
        }

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
    }
}
