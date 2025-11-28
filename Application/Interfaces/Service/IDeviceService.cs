using System.Collections.Generic;
using Application.Models;

namespace Application.Interfaces.Service
{
    public interface IDeviceService
    {
        Device? GetDeviceByID(int deviceID);
        IEnumerable<Device> GetAllDevices();
        void AddDevice(Device device);
        void UpdateDevice(Device device);
        void DeleteDevice(int deviceID);
    }
}
