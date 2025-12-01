using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Models.DisplayModels;

// Represents a request with information for dashboard representation
public class RequestDisplayModel
{
    public int RequestID { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string DeviceType { get; set; } = string.Empty;
    public string OperatingSystem { get; set; } = string.Empty;
    public DateTime RequestDate { get; set; }
    public string RequestStatus { get; set; } = string.Empty;

    // Summary line for RequestID, type and operatingsystem
    public string RequestLine =>
        $"REQ-{RequestID} · {DeviceType} · {OperatingSystem}";

    // Summary line for requester, location and date. 
    public string RequesterLine =>
        $"{EmployeeName} · {Location} · {RequestDate:dd.MM.yyyy}";

}
