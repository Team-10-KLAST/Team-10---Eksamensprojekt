using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Models;
using Application.Models.DisplayModels;

namespace Application.Interfaces.Service
{
    public interface IRequestService
    {
        Request GetRequestByID(int requestID);
        IEnumerable<Request> GetAllRequests();
        void SubmitRequest(Request request);
        void ApproveRequest(int requestId, int approverID, string comment);
        void RejectRequest(int requestID, int approverID, string comment);
        ProcessRequestDisplayModel GetProcessRequestDisplayModel(int requestId);
    }
}
