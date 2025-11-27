using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Application.Interfaces;
using Application.Interfaces.Repository;
using Application.Models;
using Application.Models.DisplayModels;

namespace Application.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IRepository<Employee> _employeeRepository;
        private readonly IRepository<Department> _departmentRepository;
        private readonly IRepository<Role> _roleRepository;

        public EmployeeService(IRepository<Employee> employeeRepository, IRepository<Department> departmentRepository, IRepository<Role> roleRepository)
        {
            _employeeRepository = employeeRepository;
            _departmentRepository = departmentRepository;
            _roleRepository = roleRepository;
        }

        //Adds a new employee
        public void AddEmployee(Employee employee)
        {
            if (string.IsNullOrWhiteSpace(employee.FirstName))
            {
                throw new ArgumentException("Firstname field cannot be empty.");
            }
            
            if (string.IsNullOrWhiteSpace(employee.LastName))
            {
                throw new ArgumentException("Lastname field cannot be empty.");
            }
            
            if (string.IsNullOrWhiteSpace(employee.Email))
            {
                throw new ArgumentException("Email field cannot be empty.");
            }
            
            if (employee.DepartmentID == null)
            {
                throw new ArgumentException("Department field cannot be empty.");
            }
            
            if (employee.RoleID == null)
            {
                throw new ArgumentException("Role field cannot be empty.");
            }
            
            _employeeRepository.Add(employee);
        }

        //Deletes employee by ID
        public void DeleteEmployee(int employeeID)
        {
            _employeeRepository.Delete(employeeID);
        }

        //Gets all employees
        public IEnumerable<Employee> GetAllEmployees()
        {
            return _employeeRepository.GetAll();
        }

        //Gets employee by unique ID
        public Employee GetEmployeeByID(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException("Invalid employee ID.");
            }
            return _employeeRepository.GetByID(id);
        }

        // Gets all departments and roles
        public IEnumerable<Department> GetAllDepartments() => _departmentRepository.GetAll();
        public IEnumerable<Role> GetAllRoles() => _roleRepository.GetAll();

        // Gets employee display models with full details for EmployeeViewModel
        public IEnumerable<EmployeeDisplayModel> GetEmployeeDisplayModels()
        {
            var allEmployees = _employeeRepository.GetAll();
            var departments = _departmentRepository.GetAll().ToDictionary(department => department.DepartmentID, department => department.Name);
            var roles = _roleRepository.GetAll().ToDictionary(role => role.RoleID, role => role.Name);

            var employeeDisplayModels = new List<EmployeeDisplayModel>();

            foreach (var employee in allEmployees)
            {
                var employeeDisplayModel = new EmployeeDisplayModel
                {
                    EmployeeID = employee.EmployeeID,
                    FullName = $"{employee.FirstName} {employee.LastName}",
                    Email = employee.Email,
                    DepartmentName = departments.TryGetValue(employee.DepartmentID, out var departmentName) ? departmentName : "Unknown",
                    RoleName = roles.TryGetValue(employee.RoleID, out var roleName) ? roleName : "Unknown"
                };

                employeeDisplayModels.Add(employeeDisplayModel);
            }

            return employeeDisplayModels;
        }

        /*//Gets employees by department
        public IEnumerable<Employee> GetEmployeesByDepartment(Department department)
        {
            return _employeeRepository.GetByDepartment(department); //-- skal det laves til en filtreret liste i stedet metoden GetByDepartment?
        }*/

        //Updates existing employee details (all fields except EmployeeId)
        /*public void UpdateEmployee(Employee employee)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            //Checks for existing email in the database
            string checkEmailQuery = @"
                SELECT COUNT(*) FROM Employee 
                WHERE Email = @Email AND EmployeeId <> @EmployeeId;";

            using var checkEmailCommand = new SqlCommand(checkEmailQuery, connection);
            checkEmailCommand.Parameters.AddWithValue("@Email", employee.Email);
            checkEmailCommand.Parameters.AddWithValue("@EmployeeId", employee.EmployeeId);
            int emailExists = (int)checkEmailCommand.ExecuteScalar();
            if (emailExists > 0)
            {
                throw new InvalidOperationException("Email already exists for another employee.");
            }

            //Updates employee details
            string query = @"
                UPDATE Employee
                SET Role = @Role,
                    Department = @Department,
                    FirstName = @FirstName,
                    LastName = @LastName,
                    Email = @Email
                WHERE EmployeeId = @EmployeeId;";

            var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Role", employee.Role);
            command.Parameters.AddWithValue("@Department", employee.Department);
            command.Parameters.AddWithValue("@FirstName", employee.FirstName);
            command.Parameters.AddWithValue("@LastName", employee.LastName);
            command.Parameters.AddWithValue("@Email", employee.Email);
            command.Parameters.AddWithValue("@EmployeeId", employee.EmployeeId);

            command.ExecuteNonQuery();
        }*/
        //Gets employee by email
        /*public Employee GetEmployeeByEmail(string email)
        {
            Employee employee = null;
            string query = @"
                SELECT e.EmployeeId, e.Role, e.Department, e.FirstName, e.LastName, e.Email 
                FROM Employee e
                JOIN (SELECT EmployeeId, CONCAT(FirstName, ' ', LastName) AS FullName FROM Employee) f ON e.EmployeeId = f.EmployeeId
                WHERE e.Email = @Email;";

            using var connection = new SqlConnection(_connectionString);
            var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Email", email);

            connection.Open();
            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                employee = new Employee(
                    (int)reader["EmployeeId"],
                    (Role)reader["Role"],
                    (Department)reader["Department"],
                    (string)reader["FirstName"],
                    (string)reader["LastName"],
                    (string)reader["Email"]
                );
            }
            else
            {
                throw new InvalidOperationException("No employee found with the given email.");
            }
            return employee;
        }

        //Gets employees by last name
        public IEnumerable<Employee> GetByLastName(string lastName)
        {
            var employees = new List<Employee>();
            string query = @"
                SELECT e.EmployeeId, e.Role, e.Department, e.FirstName, e.LastName, e.Email 
                FROM Employee e
                JOIN (SELECT EmployeeId, CONCAT(FirstName, ' ', LastName) AS FullName FROM Employee) f ON e.EmployeeId = f.EmployeeId
                WHERE e.LastName = @LastName;";
            using var connection = new SqlConnection(_connectionString);
            var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@LastName", lastName);
            connection.Open();
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                employees.Add(new Employee(
                    (int)reader["EmployeeId"],
                    (Role)reader["Role"],
                    (Department)reader["Department"],
                    (string)reader["FirstName"],
                    (string)reader["LastName"],
                    (string)reader["Email"]
                ));
            }
            if (employees.Count == 0)
            {
                throw new InvalidOperationException("No employees found with the given lastname.");
            }
            return employees;
        }*/


        //Gets employees by role
        /*public IEnumerable<Employee> GetByRole(Role role)
        {
            var employees = new List<Employee>();
            string query = @"
                SELECT e.EmployeeId, e.Role, e.Department, e.FirstName, e.LastName, e.Email 
                FROM Employee e
                JOIN (SELECT EmployeeId, CONCAT(FirstName, ' ', LastName) AS FullName FROM Employee) f ON e.EmployeeId = f.EmployeeId
                WHERE e.Role = @Role;";
            using var connection = new SqlConnection(_connectionString);
            var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Role", role);
            connection.Open();
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                employees.Add(new Employee(
                    (int)reader["EmployeeId"],
                    (Role)reader["Role"],
                    (Department)reader["Department"],
                    (string)reader["FirstName"],
                    (string)reader["LastName"],
                    (string)reader["Email"]
                ));
            }
            if (employees.Count == 0)
            {
                throw new InvalidOperationException("No employees found in the given role.");
            }
            return employees;
        }*/
    }
}
