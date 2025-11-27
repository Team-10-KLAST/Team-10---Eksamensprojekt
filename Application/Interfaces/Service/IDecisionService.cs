using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Models;

namespace Application.Interfaces.Service;

public interface IDecisionService
{
    //Decision GetDecisionByID(int decisionID);
    void ApproveLoan(int loanID, string comment);
    void RejectLoan(int loanID, string comment);
}
