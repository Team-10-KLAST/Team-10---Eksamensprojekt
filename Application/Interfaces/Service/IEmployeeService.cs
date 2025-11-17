using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AssetManager.Model;

namespace Application.Interfaces
{
    public interface IEmployeeService : IRepository
    {
        /*Employee GetAllEmployee(int employeeId);
        void AddEmployee(Employee employee);
        void UpdateEmployee(Employee employee);
        void DeleteEmployee(int employeeId);*/

        IEnumerable<Employee> GetAllEmployees(); //--retrieves all employees
        Employee GetEmployeeByEmployeeId(int employeeId); //--finds employee by unique ID
        IEnumerable<string> GetEmployeeByLastName(string lastName); //--finds employees by last name
        Employee GetEmployeeByEmail(string email); //--finds employee by email
        IEnumerable<Employee> GetEmployeesByDepartment(Department department); //--finds employees by department
        IEnumerable<Employee> GetEmployeesByRole(Role role); //--finds employees by role
    }
}
