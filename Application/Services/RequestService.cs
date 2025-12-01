using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Models;
using Application.Interfaces;
using Application.Interfaces.Repository;
using Application.Interfaces.Service;

namespace Application.Services;

public class RequestService : IRequestService
{
    private readonly IRepository<Request> _requestRepository;

    public RequestService(IRepository<Request> requestRepository)
    {
        _requestRepository = requestRepository;
    }

    public Request? GetRequestByID(int requestID)
    {
        return _requestRepository.GetByID(requestID);
    }

    public IEnumerable<Request> GetAllRequests()
    {
        return _requestRepository.GetAll();
    }
    public void SubmitRequest(Request request)
    { }
    public void CancelRequest(int requestID)
    { }
    public void UpdateRequestStatus(int requestID, string newStatus)
    { }
}
