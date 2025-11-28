using System;
using System.Collections.Generic;
using Application.Interfaces.Repository;
using Application.Interfaces.Service;
using Application.Models;

namespace Application.Services
{
    // Implements the device service interface to provide business logic for device management
    public class DeviceService : IDeviceService
    {
        // Used to load, create, update and delete devices in the database
        private readonly IRepository<Device> _deviceRepository;

        // Receives the repository instance from dependency injection
        public DeviceService(IRepository<Device> deviceRepository)
        {
            _deviceRepository = deviceRepository;
        }

        // Gets one device by its DeviceID, or null if it does not exist
        public Device? GetDeviceByID(int deviceID)
        {
            // Guards against invalid DeviceID values
            if (deviceID <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(deviceID), "DeviceID must be greater than zero.");
            }

            return _deviceRepository.GetById(deviceID);
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
            {
                throw new ArgumentNullException(nameof(device));
            }

            if (device.DeviceID <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(device.DeviceID), "DeviceID must be greater than zero.");
            }

            _deviceRepository.Update(device);
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
    }
}
