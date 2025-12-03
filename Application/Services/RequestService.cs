using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

    // Constructor injecting the required repositories
    public RequestService(IRepository<Request> requestRepository, IRepository<Employee> employeeRepository, 
        IRepository<Device> deviceRepository, IRepository<DeviceDescription> deviceDescriptionRepository, 
        IRepository<Loan> loanRepository, IRepository<Decision> decisionRepository)
    {
        _requestRepository = requestRepository;
        _employeeRepository = employeeRepository;
        _deviceRepository = deviceRepository;
        _deviceDescriptionRepository = deviceDescriptionRepository;
        _loanRepository = loanRepository;
        _decisionRepository = decisionRepository;
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

    public void SubmitRequest(Request request)
    { }

    // Approves a request by updating the relevant entities and recording the decision
    public void ApproveRequest(int requestId, int approverId, string? comment = null)
    {
        var loan = ProcessBaseRequest(requestId, approverId);

        var device = _deviceRepository.GetByID(loan.DeviceID)
                 ?? throw new InvalidOperationException("Device not found");
        device.Status = DeviceStatus.PLANNED;
        _deviceRepository.Update(device);

        var decision = new Decision
        {
            LoanID = loan.LoanID,
            DecisionStatus = DecisionStatus.APPROVED,
            DecisionDate = DateOnly.FromDateTime(DateTime.Now),
            Comments = string.IsNullOrWhiteSpace(comment) ? string.Empty : comment
        };
        _decisionRepository.Add(decision);
    }

    // Rejects a request by updating the relevant entities and recording the decision
    public void RejectRequest(int requestId, int approverId, string comment)
    {
        var loan = ProcessBaseRequest(requestId, approverId);

        var device = _deviceRepository.GetByID(loan.DeviceID)
                 ?? throw new InvalidOperationException("Device not found");
        device.Status = DeviceStatus.CANCELLED;
        _deviceRepository.Update(device);

        var decision = new Decision
        {
            LoanID = loan.LoanID,
            DecisionStatus = DecisionStatus.REJECTED,
            DecisionDate = DateOnly.FromDateTime(DateTime.Now),
            Comments = string.IsNullOrWhiteSpace(comment) ? string.Empty : comment
        };
        _decisionRepository.Add(decision);
    }

    // Common processing for both approval and rejection of requests
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

        return new ProcessRequestDisplayModel
        {
            TentativeAssigneeEmail = borrower.Email,
            DeviceType = description.DeviceType,
            OperatingSystem = description.OperatingSystem,
            Location = description.Location,
        };
    }
}
