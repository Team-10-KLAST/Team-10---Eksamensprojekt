using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Models;

namespace Application.Interfaces.Service
{
    public interface IDeviceDescriptionService
    {
        IEnumerable<string> GetAllOSOptions();
        IEnumerable<string> GetAllDeviceTypeOptions();
        IEnumerable<string> GetAllCountryOptions();
    }
}
