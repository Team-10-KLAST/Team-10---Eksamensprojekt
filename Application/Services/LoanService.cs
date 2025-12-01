using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Interfaces.Repository;
using Application.Interfaces.Service;
using Application.Models;

namespace Application.Services;

public class LoanService : ILoanService
{


    public LoanService()
    {

    }


    // Loan GetLoanByID(int loanID) { }
    // IEnumerable<Loan> GetAllLoans() { }
    public void CreateLoan(int requestID, int borrowerID, int deviceID)
    {

    }
    public void CloseLoan(int loanID)
    {

    }
    public void UpdateLoanStatus(int loanID, string newStatus)
    {

    }

    public void AddLoan(Loan loan)
    {
        
    }
    
}
