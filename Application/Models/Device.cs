using System;
using Application.Models;

namespace Application.Models
{
    public enum DeviceStatus
    {
        REGISTERED,
        CANCELLED,
        PLANNED,
        ORDERED,
        RECEIVED,
        INUSE,
        INSTOCK
    }

    public class Device
    {
        public int DeviceID { get; set; }
        public DeviceStatus Status { get; set; } = DeviceStatus.REGISTERED;
        public bool Wiped { get; set; } = false;
        public DateOnly PurchaseDate { get; set; }
        public DateOnly ExpectedEndDate { get; set; }
        public int DeviceDescriptionID { get; set; }
        public Device()
        {
        }

        // Constructor with parameters (excluding DeviceID).
        public Device(DeviceStatus status,
                      bool wiped,
                      DateOnly purchaseDate,
                      DateOnly expectedEndDate,
                      int deviceDescriptionID)
        {
            Status = status;
            Wiped = wiped;
            PurchaseDate = purchaseDate;
            ExpectedEndDate = expectedEndDate;
            DeviceDescriptionID = deviceDescriptionID;
        }
    }
}
