using System;

namespace Application.Models
{
    public enum DecisionStatus
    {
        PENDING = 0,
        APPROVED = 1,
        REJECTED = 2
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
