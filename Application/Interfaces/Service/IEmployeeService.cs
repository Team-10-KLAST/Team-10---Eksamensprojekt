using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Models;
using Application.Models.DisplayModels;

namespace Application.Interfaces
{
    public interface IEmployeeService
    {
        void AddEmployee(Employee employee); 
        IEnumerable<Employee> GetAllEmployees();
        Employee GetEmployeeByEmail(string email); 
        IEnumerable<EmployeeDisplayModel> GetEmployeeDisplayModels();
        IEnumerable<Department> GetAllDepartments();
        IEnumerable<Role> GetAllRoles();
        IEnumerable<string> GetAllEmployeeEmails();
        void TerminateEmployee(int employeeID, DateOnly terminationDate);        
        string ValidateApproverEmail(string email);
        string ValidateEmployeeEmail(string email);
    }

}
