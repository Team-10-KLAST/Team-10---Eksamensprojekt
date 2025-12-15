using System;

namespace Application.Models
{
    public enum LoanStatus
    {
        INACTIVE,
        ACTIVE
    }

    public class Loan
    {
        public int LoanID { get; set; }
        public LoanStatus Status { get; set; } = LoanStatus.INACTIVE;
        public DateOnly? StartDate { get; set; }
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
        public Loan(LoanStatus loanStatus, DateOnly startDate, DateOnly? endDate,
                    int requestID, int borrowerID, int approverID, int deviceID)
        {
            Status = loanStatus;
            StartDate = startDate;
            EndDate = endDate;
            RequestID = requestID;
            BorrowerID = borrowerID;
            ApproverID = approverID;
            DeviceID = deviceID;
        }
    }
}
