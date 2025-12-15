using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Models;

namespace Application.Interfaces.Service;

public interface ILoanService
{
   // Loan GetLoanByID(int loanID);
   // IEnumerable<Loan> GetAllLoans();
    void CreateLoan(int requestID, int borrowerID, int deviceID);
    void CloseLoan(int loanID);
    void UpdateLoanStatus(int loanID, LoanStatus newStatus);
    void AddLoan (Loan loan);
}
