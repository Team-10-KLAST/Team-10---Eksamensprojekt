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
        void DeleteDevice(int deviceID);
        int CreateVirtualDeviceID(string DeviceType, string OS, string country);
        DeviceDisplayModel? GetDeviceDisplayByID(int deviceID);
        IEnumerable<Device> GetAvailableDevicesByType(string deviceType);
        IEnumerable<DeviceDisplayModel> GetAvailableDeviceDisplayModels(string? deviceType);
    }
}
