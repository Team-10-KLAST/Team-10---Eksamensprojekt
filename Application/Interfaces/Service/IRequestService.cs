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
        Request? GetRequestByID(int requestID);
        IEnumerable<Request> GetAllRequests();
        void SubmitRequest(string email, string deviceType, string OS, 
            string country, string comment, DateOnly neededByDate);
        void ApproveRequest(int requestId, string approverEmail, string comment);
        void RejectRequest(int requestID, string approverEmail, string comment);
        ProcessRequestDisplayModel GetProcessRequestDisplayModel(int requestId);
        void AddRequest(Request request);
    }
}
