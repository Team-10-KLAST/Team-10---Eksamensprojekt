using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Application.Interfaces;
using Application.Interfaces.Repository;
using Application.Interfaces.Service;
using Application.Models;
using Application.Models.DisplayModels;

namespace Application.Services;

public class RequestService : IRequestService
{
    // Repositories for accessing data
    private readonly IRepository<Request> _requestRepository;
    private readonly IRepository<Employee> _employeeRepository;
    private readonly IRepository<Device> _deviceRepository;
    private readonly IRepository<DeviceDescription> _deviceDescriptionRepository;
    private readonly IRepository<Loan> _loanRepository;
    private readonly IRepository<Decision> _decisionRepository;

    private readonly IDeviceService _deviceService;
    private readonly ILoanService _loanService;
    private readonly IEmployeeService _employeeService;

    // Constructor injecting the required repositories
    public RequestService(IRepository<Request> requestRepository, IRepository<Employee> employeeRepository,
        IRepository<Device> deviceRepository, IRepository<DeviceDescription> deviceDescriptionRepository,
        IRepository<Loan> loanRepository, IRepository<Decision> decisionRepository,
        IDeviceService deviceService, ILoanService loanService, IEmployeeService employeeService)
    {
        _requestRepository = requestRepository;
        _employeeRepository = employeeRepository;
        _deviceRepository = deviceRepository;
        _deviceDescriptionRepository = deviceDescriptionRepository;
        _loanRepository = loanRepository;
        _decisionRepository = decisionRepository;
        _deviceService = deviceService;
        _loanService = loanService;
        _employeeService = employeeService;
    }

    // Retrieves a request by its ID
    public Request? GetRequestByID(int requestID)
    {
        return _requestRepository.GetByID(requestID);
    }

    // Retrieves all requests
    public IEnumerable<Request> GetAllRequests()
    {
        return _requestRepository.GetAll();
    }

    // Approves a request by updating the relevant entities and recording the decision
    public void ApproveRequest(int requestId, int approverId, string? comment = null)
    {
        var loan = ProcessBaseRequest(requestId, approverId);
        loan.Status = LoanStatus.ACTIVE;
        loan.StartDate = DateOnly.FromDateTime(DateTime.Today);
        _loanRepository.Update(loan);

        var device = _deviceRepository.GetByID(loan.DeviceID)
                 ?? throw new InvalidOperationException("Device not found");
        device.Status = DeviceStatus.PLANNED;
        _deviceRepository.Update(device);

        RecordDecision(loan.LoanID, DecisionStatus.APPROVED, comment);
    }

    // Rejects a request by updating the relevant entities and recording the decision
    public void RejectRequest(int requestId, int approverId, string comment)
    {
        var loan = ProcessBaseRequest(requestId, approverId);

        var device = _deviceRepository.GetByID(loan.DeviceID)
                 ?? throw new InvalidOperationException("Device not found");
        device.Status = DeviceStatus.CANCELLED;
        _deviceRepository.Update(device);

        RecordDecision(loan.LoanID, DecisionStatus.REJECTED, comment);
    }

    // Helper method to record a decision
    private void RecordDecision(int loanId, DecisionStatus status, string? comment)
    {
        var decision = new Decision
        {
            LoanID = loanId,
            Status = status,
            DecisionDate = DateOnly.FromDateTime(DateTime.Now),
            Comment = string.IsNullOrWhiteSpace(comment) ? string.Empty : comment
        };
        _decisionRepository.Add(decision);
    }

    // Common processing for both approval and rejection of requests. Update loan with ApproverID and change request.Status to CLOSED.
    private Loan ProcessBaseRequest(int requestId, int approverId)
    {
        var loan = _loanRepository.GetAll().FirstOrDefault(l => l.RequestID == requestId);
        if (loan == null) throw new InvalidOperationException("Loan not found");

        loan.ApproverID = approverId;
        _loanRepository.Update(loan);

        var request = _requestRepository.GetByID(requestId)
              ?? throw new InvalidOperationException("Request not found");
        request.Status = RequestStatus.CLOSED;
        _requestRepository.Update(request);

        return loan;
    }

    // Fetches and constructs a ProcessRequestDisplayModel for the given request ID
    public ProcessRequestDisplayModel GetProcessRequestDisplayModel(int requestId)
    {
        var loan = _loanRepository.GetAll().FirstOrDefault(l => l.RequestID == requestId)
               ?? throw new InvalidOperationException("Loan not found");

        var borrower = _employeeRepository.GetByID(loan.BorrowerID)
                      ?? throw new InvalidOperationException("Borrower not found");

        var device = _deviceRepository.GetByID(loan.DeviceID)
                     ?? throw new InvalidOperationException("Device not found");

        var description = _deviceDescriptionRepository.GetByID(device.DeviceDescriptionID)
                          ?? throw new InvalidOperationException("Device description not found");

        var request = _requestRepository.GetByID(requestId)
                      ?? throw new InvalidOperationException("Request not found");

        var neededByDate = _requestRepository.GetByID(requestId)?.NeededByDate
                           ?? throw new InvalidOperationException("NeededByDate not found");

        return new ProcessRequestDisplayModel
        {
            TentativeAssigneeEmail = borrower.Email,
            DeviceType = description.DeviceType,
            OperatingSystem = description.OperatingSystem,
            Location = description.Location,
            Justification = request.Justification,
            NeededByDate = request.NeededByDate.ToDateTime(TimeOnly.MinValue)
        };
    }

    // pass on the information from AddRequest form
    public void SubmitRequest(string email, string deviceType, string OS, string country, string justification, DateOnly neededByDate)
    {
        Request _request = new Request
        {
            Justification = justification,
            RequestDate = DateOnly.FromDateTime(DateTime.Now),
            NeededByDate = neededByDate,
            Status = RequestStatus.PENDING,
        };
        AddRequest(_request);
        Employee? employee = null;
        try
        {
            employee = _employeeService.GetEmployeeByEmail(email);
        }
        catch (ArgumentException ex)
        {
            throw new ArgumentException($"Invalid email format: {email}", ex);
        }
        
        if (employee == null)
        {
            throw new InvalidOperationException($"Employee with email {email} not found");
        }
        
        int _borrowerID = employee.EmployeeID;
        int _deviceID = _deviceService.CreateVirtualDeviceID(deviceType, OS, country);

        _loanService.CreateLoan(_request.RequestID, _borrowerID, _deviceID);
    }

    public void AddRequest(Request request)
    {
        if (string.IsNullOrWhiteSpace(request.Justification))
        {
            throw new ArgumentException("Comment field cannot be empty.");
        }
        _requestRepository.Add(request);
    }
}
