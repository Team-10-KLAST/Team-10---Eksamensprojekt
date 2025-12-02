using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Models.DisplayModels
{
    // Display model for requests on the dashboard
    public class RequestDashboardDisplayModel
    {
        public int RequestID { get; set; }
        public DateOnly RequestDate { get; set; }
        public string Justification { get; set; } = string.Empty;
        public RequestStatus Status { get; set; }
        // Text to show in the first line of the card
        public string HeaderText { get; set; } = string.Empty;
        // Text to show in the second line of the card
        public string SubText { get; set; } = string.Empty;
    }
}
