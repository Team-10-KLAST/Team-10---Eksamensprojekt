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
        public DeviceStatus Status { get; set; } = Models.DeviceStatus.REGISTERED;
        public decimal Price { get; set; }
        public DateOnly PurchaseDate { get; set; }
        public DateOnly ExpectedEndDate { get; set; }
        public int DeviceDescriptionID { get; set; }


        // Parameterless constructor for ADO.NET.
        public Device()
        {
        }

        // Constructor with parameters (excluding DeviceID).
        public Device(DeviceStatus status, decimal price,
                      DateOnly purchaseDate, DateOnly expectedEndDate,
                      int deviceDescriptionID)
        {
            Status = status;
            Price = price;
            PurchaseDate = purchaseDate;
            ExpectedEndDate = expectedEndDate;
            DeviceDescriptionID = deviceDescriptionID;
        }
    }
}
