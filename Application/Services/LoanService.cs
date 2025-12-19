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

    // Creates a new loan based on a request
    public void CreateLoan(int requestID, int borrowerID, int deviceID)
    {
        if (requestID <= 0)
            throw new ArgumentException("RequestID cannot be empty");

        if (borrowerID <= 0)
            throw new ArgumentException("BorrowerID cannot be empty");

        if (deviceID <= 0)
            throw new ArgumentException("DeviceID cannot be empty");

        var loan = new Loan
        {
            RequestID = requestID,
            BorrowerID = borrowerID,
            ApproverID = null,
            DeviceID = deviceID,
            Status = LoanStatus.INACTIVE,
            StartDate = null,
            EndDate = null
        };
        AddLoan(loan);
    }        

    // Adds a new loan to the repository
    public void AddLoan(Loan loan)
    {
        if (loan.BorrowerID <= 0)
            throw new ArgumentException("BorrowerID cannot be empty");

        if (loan.DeviceID <= 0)
            throw new ArgumentException("DeviceID cannot be empty");

        _loanRepository.Add(loan);
    }    

    // Assigns a device to an employee without a request and updates the device status 
    public void AssignDeviceToEmployee(int deviceID, int employeeID, int approverID)
    {
        if (employeeID <= 0)
            throw new ArgumentException("EmployeeID cannot be empty");

        if (deviceID <= 0)
            throw new ArgumentException("DeviceID cannot be empty");

        if (approverID <= 0)
            throw new ArgumentException("ApproverID cannot be empty");

        var loan = new Loan
        {
            RequestID = null,         
            BorrowerID = employeeID,
            ApproverID = approverID,
            DeviceID = deviceID,
            Status = LoanStatus.ACTIVE,
            StartDate = DateOnly.FromDateTime(DateTime.Now),
            EndDate = null
        };

        _loanRepository.Add(loan);

        var device = _deviceService.GetDeviceByID(deviceID)
            ?? throw new InvalidOperationException("Device not found.");

        device.IsWiped = false;
        device.Status = DeviceStatus.INUSE;
        _deviceService.UpdateDevice(device);
    }
}