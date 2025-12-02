using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Models.DisplayModels
{
    public class ProcessRequestDisplayModel
    {
        public string TentativeAssigneeEmail { get; set; } = "";
        public string DeviceType { get; set; } = "";
        public string OperatingSystem { get; set; } = "";
        public string Location { get; set; } = "";
    }
}
