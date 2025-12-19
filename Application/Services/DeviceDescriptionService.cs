using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Interfaces.Repository;
using Application.Interfaces.Service;
using Application.Models;

namespace Application.Services
{
    public class DeviceDescriptionService : IDeviceDescriptionService
    {
        IRepository<DeviceDescription> _deviceDescriptionRepository;

        //Collection of all DeviceDescription from DB
        IEnumerable<DeviceDescription> _deviceDescriptions;

        public DeviceDescriptionService(IRepository<DeviceDescription> deviceDescriptionRepository)
        {
            _deviceDescriptionRepository = deviceDescriptionRepository;
            _deviceDescriptions = _deviceDescriptionRepository.GetAll();
        }

        //Extract the unique values of OS / Types / Countries
        public IEnumerable<string> GetAllOSOptions()
        {
            return _deviceDescriptions.Select(d => d.OperatingSystem).Distinct();
        }

        public IEnumerable<string> GetAllDeviceTypeOptions()
        {
            return _deviceDescriptions
                .Select(description => description.DeviceType)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(deviceType => deviceType)
                .ToList();
        }

        public IEnumerable<string> GetAllCountryOptions()
        {
            return _deviceDescriptions.Select(d => d.Location).Distinct();
        }

        //Get DeviceDescriptionID from input parameter
        public int GetDeviceDescriptionID(string DeviceType, string OS, string Country)
        {
            DeviceDescription matchingDeviceDescription =
                _deviceDescriptions.FirstOrDefault(d =>
                d.DeviceType.Equals(DeviceType, System.StringComparison.OrdinalIgnoreCase) &&
                d.OperatingSystem.Equals(OS, System.StringComparison.OrdinalIgnoreCase) &&
                d.Location.Equals(Country, System.StringComparison.OrdinalIgnoreCase)
                );
            if (matchingDeviceDescription == null)
            {
                throw new InvalidOperationException("No matching device description found");
            }
            return matchingDeviceDescription.DeviceDescriptionID;
        }

        public DeviceDescription? GetByID(int id)
        {
            return _deviceDescriptions
                .FirstOrDefault(d => d.DeviceDescriptionID == id);
        }

        public IEnumerable<DeviceDescription> GetAllDescriptions()
        {
            return _deviceDescriptions;
        }
    }
}
