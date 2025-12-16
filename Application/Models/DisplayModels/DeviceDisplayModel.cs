using System;
using System.Collections.Generic;

namespace Application.Models.DisplayModels
{
    public class DeviceDisplayModel
    {
        public int DeviceID { get; set; }
        public string Type { get; set; } = string.Empty;
        public string OS { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;

        public DateTime? RegistrationDate { get; set; }
        public DateTime? ExpirationDate { get; set; }

        public DateTime? NeededByDate { get; set; }

        public bool Wiped { get; set; }
        public string OwnerFullName { get; set; } = string.Empty;
        public string OwnerEmail { get; set; } = string.Empty;
    }
}