using System;
using Application.Models;

namespace Application.Models
{

    public enum DeviceStatus
    {
        PLANNED,
        ORDERED,
        RECEIVED,
        INUSE
    }
    public class Device
    {
        public int DeviceID { get; set; }
        public string DeviceStatus { get; set; } = "";
        public DeviceStatus Status { get; set; } = Models.DeviceStatus.PLANNED;
        public decimal Price { get; set; }
        public DateOnly PurchaseDate { get; set; }
        public DateOnly ExpectedEndDate { get; set; }
        public int DeviceDescriptionID { get; set; }


        // Parameterless constructor for ADO.NET.
        public Device()
        {
        }

        // Constructor with parameters (excluding DeviceID).
        public Device(string deviceStatus, decimal price,
                      DateOnly purchaseDate, DateOnly expectedEndDate,
                      int deviceDescriptionID)
        {
            DeviceStatus = deviceStatus;
            Status = Models.DeviceStatus.PLANNED;
            Price = price;
            PurchaseDate = purchaseDate;
            ExpectedEndDate = expectedEndDate;
            DeviceDescriptionID = deviceDescriptionID;
        }
    }
}
