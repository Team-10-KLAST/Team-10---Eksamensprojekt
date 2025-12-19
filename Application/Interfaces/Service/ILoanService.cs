using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Models;

namespace Application.Interfaces.Service;

public interface ILoanService
{    
    void CreateLoan(int requestID, int borrowerID, int deviceID);
    void AddLoan (Loan loan);    
    void AssignDeviceToEmployee(int deviceID, int employeeID, int approverID);
}
