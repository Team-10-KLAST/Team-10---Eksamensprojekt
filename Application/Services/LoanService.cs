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
    private readonly IRepository<Loan> _loanRepository;

    public LoanService(IRepository<Loan> loanRepository)
    {
        _loanRepository = loanRepository;
    }


    // Loan GetLoanByID(int loanID) { }
    // IEnumerable<Loan> GetAllLoans() { }
    public void CreateLoan(int requestID, int borrowerID, int deviceID)
    {
        var loan = new Loan
        {
            RequestID = requestID,
            BorrowerID = borrowerID,
            DeviceID = deviceID,
            LoanStatus = "new"
        };
        AddLoan(loan);
    }
    public void CloseLoan(int loanID)
    {

    }
    public void UpdateLoanStatus(int loanID, string newStatus)
    {

    }

    public void AddLoan(Loan loan)
    {
        if (loan.BorrowerID==null) { throw new ArgumentException("BorrowerID cannot be empty"); }
        if (loan.DeviceID == null) { throw new ArgumentException("DeviceID cannot be empty"); }        
        _loanRepository.Add(loan);
    }
}
