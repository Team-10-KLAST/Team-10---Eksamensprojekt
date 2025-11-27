using System;

namespace Application.Models
{
    public class Loan
    {
        public int LoanID { get; set; }
        public string LoanStatus { get; set; } = "";
        public DateOnly StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public int RequestID { get; set; }
        public int BorrowerID { get; set; }
        public int ApproverID { get; set; }
        public int DeviceID { get; set; }

        // Parameterless constructor for ADO.NET.
        public Loan()
        {
        }

        // Constructor with parameters (excluding LoanID).
        public Loan(string loanStatus, DateOnly startDate, DateOnly? endDate,
                    int requestID, int borrowerID, int approverID, int deviceID)
        {
            LoanStatus = loanStatus;
            StartDate = startDate;
            EndDate = endDate;
            RequestID = requestID;
            BorrowerID = borrowerID;
            ApproverID = approverID;
            DeviceID = deviceID;
        }
    }
}
