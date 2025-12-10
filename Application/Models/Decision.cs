using System;

namespace Application.Models
{
    public enum DecisionStatus
    {
        PENDING,
        APPROVED,
        REJECTED
    }

    public class Decision
    {
        public int DecisionID { get; set; }
        public DecisionStatus Status { get; set; } = DecisionStatus.PENDING;
        public DateOnly DecisionDate { get; set; }
        public string Comment { get; set; } = "";
        public int LoanID { get; set; }
    }
}
