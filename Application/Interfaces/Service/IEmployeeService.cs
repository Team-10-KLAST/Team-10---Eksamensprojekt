using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Models;

namespace Application.Interfaces
{
    public interface IEmployeeService
    {
        void AddEmployee(Employee employee); //--adds a new employee
        void UpdateEmployee(Employee employee); //--updates an existing employee
        void DeleteEmployee(int employeeId); //--deletes an employee by unique ID
        IEnumerable<Employee> GetAllEmployees(); //--retrieves all employees
        Employee GetEmployeeById(int employeeId); //--finds employee by unique ID
        IEnumerable<Employee> GetEmployeesByDepartment(Department department); //--finds employees by department
        Employee GetEmployeeByEmail(string email); //--finds employee by email

        /*IEnumerable<Employee> GetAll(); //--retrieves all employees
        IEnumerable<string> GetEmployeeByLastName(string lastName); //--finds employees by last name
        Employee GetEmployeeByEmail(string email); //--finds employee by email
        IEnumerable<Employee> GetEmployeesByRole(Role role); //--finds employees by role*/
    }
}
