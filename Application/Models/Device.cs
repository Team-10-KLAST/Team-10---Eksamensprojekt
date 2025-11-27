using System;

namespace Application.Models
{
    public class Device
    {
        public int DeviceID { get; set; }
        public string DeviceStatus { get; set; } = "";
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
            Price = price;
            PurchaseDate = purchaseDate;
            ExpectedEndDate = expectedEndDate;
            DeviceDescriptionID = deviceDescriptionID;
        }
    }
}
