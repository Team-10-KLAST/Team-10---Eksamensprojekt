using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Models;

namespace Application.Interfaces.Service
{
    public interface IRequestService
    {
        Request GetRequestByID(int requestID);
        IEnumerable<Request> GetAllRequests();
        public void SubmitRequest(Request request);
        public void CancelRequest(int requestID);
        public void UpdateRequestStatus(int requestID, string newStatus);
    }
}
