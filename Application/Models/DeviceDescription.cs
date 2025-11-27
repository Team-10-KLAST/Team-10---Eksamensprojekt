using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Models
{
    public class DeviceDescription
    {
        public int DeviceDescriptionID { get; set; }
        public string DeviceType { get; set; } = "";
        public string OperatingSystem { get; set; } = "";
        public string Location { get; set; } = "";

        public DeviceDescription()
        {
        }

        // Constructor with parameters (excluding DeviceDescriptionID).
        public DeviceDescription(string deviceType, string operatingSystem, string location)
        {
            DeviceType = deviceType;
            OperatingSystem = operatingSystem;
            Location = location;
        }
    }
}
