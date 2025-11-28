using Application.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.Repository
{
    public interface IDecisionRepository
    {
        void AddDecision (Decision decision);
        void UpdateDecision (int decision);
        void DeleteDecision (int decisionID);


    }
}
