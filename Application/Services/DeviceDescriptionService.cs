using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Interfaces.Service;

namespace Application.Services
{
    public class DeviceDescriptionService : IDeviceDescriptionService
    {
        //Data skal hentes fra repository, som ikke er implementeres endnu
        public IEnumerable<string> GetAllOSOptions()
        {
            return new List<string> { "Windows 11", "macOS", "Linux" };
        }

        public IEnumerable<string> GetAllDeviceTypeOptions()
        {
            return new List<string> { "Laptop", "Desktop", "Mobil" };
        }

        public IEnumerable<string> GetAllCountryOptions()
        {
            return new List<string> { "Denmark", "Sweden", "Norway", "Germany" };
        }
    }
}
