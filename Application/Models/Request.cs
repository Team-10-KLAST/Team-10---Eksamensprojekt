using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Models
{
    public enum RequestStatus
    {
        PENDING,
        CLOSED
    }
    public class Request
    {
        public int RequestID { get; set; }
        public DateOnly RequestDate { get; set; }
        public DateOnly NeededByDate { get; set; }
        public string Justification { get; set; } = "";
        public RequestStatus Status { get; set; } = Models.RequestStatus.PENDING;


        // Parameterless constructor. Needed for ADO.NET.
        public Request() { }

        // Constructor with parameters (excluding RequestID). --> Behøver vi denne, når vi har parameterless constructor? <--
        public Request(DateOnly requestDate, DateOnly neededByDate, string justification, RequestStatus status)
        {
            RequestDate = requestDate;
            NeededByDate = neededByDate;
            Justification = justification;
            Status = status;
        }
    }
}
