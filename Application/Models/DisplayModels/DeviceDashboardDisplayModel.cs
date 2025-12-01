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
        public DeviceStatus Status { get; set; }
        public decimal Price { get; set; }
        public DateOnly PurchaseDate { get; set; }
        public DateOnly ExpectedEndDate { get; set; }
        // Text to show in the first line of the card
        public string HeaderText { get; set; } = string.Empty;
        // Text to show in the second line of the card
        public string SubText { get; set; } = string.Empty;
    }
}
