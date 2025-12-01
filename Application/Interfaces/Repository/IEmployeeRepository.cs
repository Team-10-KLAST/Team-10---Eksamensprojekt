using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Models;

namespace Application.Interfaces.Repository
{
    public interface IEmployeeRepository : IRepository<Employee>
    {
        // Get entity by email.
        Employee? GetByEmail(string email);
    }
}
