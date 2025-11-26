using Application.Interfaces;
using AssetManager.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Data.SqlClient;
using AssetManager.Model;
using Application.Models;

namespace Application.Services
{
    public class EmployeeService : IEmployeeService
    {
        /*private readonly string _connectionString;
        public EmployeeService(string connectionString)
        {
            _connectionString = connectionString;
        }----singleton??*/

        //Adds new employee
        public void AddEmployee(Employee employee)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            //Checks for existing email
            using var checkEmailCommand = new SqlCommand("SELECT COUNT(*) FROM Employee WHERE Email = @Email;", connection);
            checkEmailCommand.Parameters.AddWithValue("@Email", employee.Email);
            var emailExists = (int)checkEmailCommand.ExecuteScalar();
            if (emailExists > 0)
            {
                throw new InvalidOperationException("Email already exists.");
            }

            //Inserts new employee
            string query = @"
                INSERT INTO Employee (EmployeeId) 
                VALUES (@EmployeeId);
                SELECT SCOPE_IDENTITY();";
            var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@EmployeeId", employee.EmployeeId);

            employee.EmployeeId = Convert.ToInt32(command.ExecuteScalar());
        }

        //Updates existing employee details (all fields except EmployeeId)
        public void UpdateEmployee(Employee employee)
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
        }

        //Deletes employee by ID incl. checking for active devices befores deletion
        public void DeleteEmployee(int employeeId)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            //Checks for employee existence
            string findQuery = @"
                SELECT COUNT(*) FROM Employee 
                WHERE EmployeeId = @EmployeeId;";
            using var findCommand = new SqlCommand(findQuery, connection);
            findCommand.Parameters.AddWithValue("@EmployeeId", employeeId);
            int employeeExists = (int)findCommand.ExecuteScalar();
            if (employeeExists == 0)
            {
                throw new InvalidOperationException("No employee found by the given employee id.");
            }

            // Checks for active devices assigned to the employee
            string checkQuery = @"
                SELECT COUNT(*) FROM Device 
                WHERE AssignedEmployeeId = @EmployeeId AND IsActive = 1;"; //IsActive indicates active assignment from fra Device TABLE
            using var checkCommand = new SqlCommand(checkQuery, connection);
            checkCommand.Parameters.AddWithValue("@EmployeeId", employeeId);
            int activeDeviceCount = (int)checkCommand.ExecuteScalar();
            if (activeDeviceCount > 0)
            {
                throw new InvalidOperationException("Chosen employee has active devices. Deletion not possible.");
            }

            // Deletes the employee if no active devices are found
            string deleteQuery = @"
                DELETE FROM Employee
                WHERE EmployeeId = @EmployeeId;";

            using var deleteCommand = new SqlCommand(deleteQuery, connection);
            deleteCommand.Parameters.AddWithValue("@EmployeeId", employeeId);
            deleteCommand.ExecuteNonQuery();
        }

        //Gets all employees
        public IEnumerable<Employee> GetAllEmployees()
        {
            var employees = new List<Employee>();
            string query = @"
                SELECT e.EmployeeId, e.Role, e.Department, e.FirstName, e.LastName, e.Email 
                FROM Employee e
                JOIN (SELECT EmployeeId, CONCAT(FirstName, ' ', LastName) AS FullName FROM Employee) f ON e.EmployeeId = f.EmployeeId";

            using var connection = new SqlConnection(_connectionString);
            var command = new SqlCommand(query, connection);

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
            return employees;
        }

        //Gets employee by unique ID
        public Employee GetEmployeeById(int id)
        {
            Employee employee = null;
            string query = @"
                SELECT e.EmployeeId, e.Role, e.Department, e.FirstName, e.LastName, e.Email 
                FROM Employee e
                JOIN (SELECT EmployeeId, CONCAT(FirstName, ' ', LastName) AS FullName FROM Employee) f ON e.EmployeeId = f.EmployeeId
                WHERE e.EmployeeId = @EmployeeId;";

            using var connection = new SqlConnection(_connectionString);
            var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@EmployeeId", id);

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
                throw new InvalidOperationException("No employee found with the given employee id.");
            }
            return employee;
        }
        //Gets employee by email
        public Employee GetEmployeeByEmail(string email)
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
        }

        //Gets employees by department
        public IEnumerable<Employee> GetByDepartment(Department department)
        {
            var employees = new List<Employee>();
            string query = @"
                SELECT e.EmployeeId, e.Role, e.Department, e.FirstName, e.LastName, e.Email 
                FROM Employee e
                JOIN (SELECT EmployeeId, CONCAT(FirstName, ' ', LastName) AS FullName FROM Employee) f ON e.EmployeeId = f.EmployeeId
                WHERE e.Department = @Department;";

            using var connection = new SqlConnection(_connectionString);
            var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Department", department);
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
                throw new InvalidOperationException("No employees found in the given department.");
            }
            return employees;
        }

        //Gets employees by role
        public IEnumerable<Employee> GetByRole(Role role)
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
        }
    }
}
