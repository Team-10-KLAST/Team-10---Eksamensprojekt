using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Models;
using AssetManager.Model;

namespace Application.Interfaces
{
    public interface IEmployeeService
    {

        Employee GetById(int employeeId); //--finds employee by unique ID
        IEnumerable<Employee> GetByDepartment(Department department); //--finds employees by department
        
        /*IEnumerable<Employee> GetAll(); //--retrieves all employees
        IEnumerable<string> GetEmployeeByLastName(string lastName); //--finds employees by last name
        Employee GetEmployeeByEmail(string email); //--finds employee by email
        IEnumerable<Employee> GetEmployeesByRole(Role role); //--finds employees by role*/
    }
}
