using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Interfaces;
using Application.Interfaces.Repository;
using Application.Interfaces.Service;
using Application.Models;

namespace Application.Services;

public class LoanService : ILoanService
{
    private readonly IRepository<Loan> _loanRepository;
    private readonly IDeviceService _deviceService;

    public LoanService(IRepository<Loan> loanRepository, IDeviceService deviceService)
    {
        _loanRepository = loanRepository;
        _deviceService = deviceService;
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
            Status = LoanStatus.INACTIVE,
            StartDate = null,
            EndDate = null
        };
        AddLoan(loan);
    }

    public void CloseLoan(int loanID)
    {

    }
    public void UpdateLoanStatus(int loanID, LoanStatus newStatus)
    {
        var loan = _loanRepository.GetByID(loanID)
            ?? throw new InvalidOperationException("Loan not found.");

        loan.Status = newStatus;
        _loanRepository.Update(loan);
    }

    public void AddLoan(Loan loan)
    {
        if (loan.BorrowerID <= 0) { throw new ArgumentException("BorrowerID cannot be empty"); }
        if (loan.DeviceID <= 0) { throw new ArgumentException("DeviceID cannot be empty"); }
        _loanRepository.Add(loan);

        if (loan.Status == LoanStatus.INACTIVE || loan.Status == LoanStatus.ACTIVE)
        {
            var device = _deviceService.GetDeviceByID(loan.DeviceID)
                ?? throw new InvalidOperationException("Device not found.");

            device.IsWiped = false;
            _deviceService.UpdateDevice(device);
        }

    }

    public IEnumerable<Loan> GetAllLoans()
    {
        return _loanRepository.GetAll();
    }

    public Loan GetMostRecentLoanByDeviceID(int deviceID)
    {
        var loan = GetAllLoans()
            .Where(d => d.DeviceID == deviceID)
            .OrderByDescending(d => d.LoanID)
            .FirstOrDefault();

        return loan;
    }
}