using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Interfaces.Repository;
using Application.Interfaces.Service;
using Application.Models;

namespace Application.Services;

public class DecisionService : IDecisionService
{


    public DecisionService()
    { }

    /*public Decision GetDecisionByID(int id)
    { } */

    public void ApproveLoan(int id, string comment)
    { }

    public void RejectLoan(int id, string comment)
    { }

}
