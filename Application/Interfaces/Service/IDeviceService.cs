using System.Collections.Generic;
using Application.Models;
using Application.Models.DisplayModels;

namespace Application.Interfaces.Service
{
    public interface IDeviceService
    {
        Device? GetDeviceByID(int deviceID);
        IEnumerable<Device> GetAllDevices();
        void AddDevice(Device device);
        void UpdateDevice(Device device);
        void UpdateDevice(DeviceDisplayModel updatedDevice);        
        int CreateVirtualDeviceID(string DeviceType, string OS, string country);
        DeviceDisplayModel? GetDeviceDisplayByID(int deviceID);        
        IEnumerable<DeviceDisplayModel> GetAvailableDeviceDisplayModels(string? deviceType);
        IEnumerable<DeviceDisplayModel> GetAllDeviceDisplayModels();
        DateTime CalculateDefaultExpiryDate(DateTime registrationDate);
    }
}
