using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Models.DisplayModels
{
    public class DeviceDashboardDisplayModel
    {
        public int DeviceID { get; set; }
        public string HeaderText { get; set; } = string.Empty;
        // Text to show in the second line of the card
        public string SubText { get; set; } = string.Empty;
    }
}
