using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Models
{
    public class Request
    {
        public int RequestID { get; set; }
        public DateOnly RequestDate { get; set; }
        public string Justification { get; set; } = "";
        public string RequestStatus { get; set; } = "";


        // Parameterless constructor. Needed for ADO.NET.
        public Request() { }

        // Constructor with parameters (excluding RequestID).
        public Request(DateOnly requestDate, string justification, string requestStatus)
        {
            RequestDate = requestDate;
            Justification = justification;
            RequestStatus = requestStatus;
        }
    }
}
