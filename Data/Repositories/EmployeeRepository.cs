using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using Application.Interfaces.Repository;
using Application.Models;
using Data;
using Application.Interfaces;

namespace Data.Repositories
{
    public class EmployeeRepository : IRepository<Employee>
    {
        // Dependency on DatabaseConnection, used to create SQL connections.
        private readonly DatabaseConnection _databaseConnection;

        // Constructor injection of the DatabaseConnection
        public EmployeeRepository(DatabaseConnection databaseConnection)
        {
            _databaseConnection = databaseConnection;
        }

        // Stored procedure names
        private const string SpAddEmployee = "uspAddEmployee";
        private const string SpUpdateEmployee = "uspUpdateEmployee";
        private const string SpDeleteEmployee = "uspDeleteEmployee";
        private const string SpGetAllEmployees = "uspGetAllEmployees";
        private const string SpGetEmployeeByID = "uspGetEmployeeByID";

        // Retrieves all employees
        public IEnumerable<Employee> GetAll()
        {
            var employees = new List<Employee>();

            try
            {
                using (var connection = _databaseConnection.CreateConnection())
                using (var command = new SqlCommand(SpGetAllEmployees, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    connection.Open();

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            employees.Add(MapEmployee(reader));
                        }
                    }
                }
                return employees;
            }
            catch (SqlException exception)
            {
                throw new DataException("An error occurred while retrieving employees from the database.", exception);
            }
            catch (Exception exception)
            {
                throw new DataException("Unexpected error while retrieving employees.", exception);
            }
        }

        // Retrieves a single employee by ID
        public Employee? GetByID(int id)
        {
            if (id <= 0)
                throw new ArgumentOutOfRangeException(nameof(id), "EmployeeID must be greater than zero.");

            try
            {
                using (var connection = _databaseConnection.CreateConnection())
                using (var command = new SqlCommand(SpGetEmployeeByID, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add("@EmployeeID", SqlDbType.Int).Value = id;

                    connection.Open();

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                            return MapEmployee(reader);
                    }
                }

                return null;
            }
            catch (SqlException exception)
            {
                throw new DataException($"An error occurred while retrieving the employee with ID {id}.", exception);
            }
            catch (Exception exception)
            {
                throw new DataException("Unexpected error while retrieving employee by ID.", exception);
            }
        }

        // Adds a new employee
        public void Add(Employee employee)
        {
            if (employee is null)
                throw new ArgumentNullException(nameof(employee));

            try
            {
                using (var connection = _databaseConnection.CreateConnection())
                using (var command = new SqlCommand(SpAddEmployee, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    AddEmployeeParameters(command, employee);

                    connection.Open();

                    var result = command.ExecuteScalar();

                    if (result == null || result == DBNull.Value)
                        throw new KeyNotFoundException("Could not get generated EmployeeID from stored procedure.");

                    employee.EmployeeID = Convert.ToInt32(result);
                }
            }
            catch (SqlException exception)
            {
                throw new DataException("An error occurred while adding a new employee to the database.", exception);
            }
            catch (Exception exception)
            {
                throw new DataException("Unexpected error while adding employee.", exception);
            }
        }

        // Updates an existing employee
        public void Update(Employee employee)
        {
            if (employee is null)
                throw new ArgumentNullException(nameof(employee));

            if (employee.EmployeeID <= 0)
                throw new ArgumentOutOfRangeException(nameof(employee.EmployeeID), "EmployeeID must be greater than zero.");

            try
            {
                using (var connection = _databaseConnection.CreateConnection())
                using (var command = new SqlCommand(SpUpdateEmployee, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.Add("@EmployeeID", SqlDbType.Int).Value = employee.EmployeeID;
                    AddEmployeeParameters(command, employee);

                    connection.Open();

                    var rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected == 0)
                        throw new KeyNotFoundException($"No employee found with ID {employee.EmployeeID} to update.");
                }
            }
            catch (SqlException exception)
            {
                throw new DataException($"An error occurred while updating the employee with ID {employee.EmployeeID}.", exception);
            }
            catch (Exception exception)
            {
                throw new DataException("Unexpected error while updating employee.", exception);
            }
        }

        // Deletes an employee
        public void Delete(int id)
        {
            if (id <= 0)
                throw new ArgumentOutOfRangeException(nameof(id), "EmployeeID must be greater than zero.");

            try
            {
                using (var connection = _databaseConnection.CreateConnection())
                using (var command = new SqlCommand(SpDeleteEmployee, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add("@EmployeeID", SqlDbType.Int).Value = id;

                    connection.Open();

                    var rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected == 0)
                        throw new KeyNotFoundException($"No employee found with ID {id} to delete.");
                }
            }
            catch (SqlException exception)
            {
                throw new DataException($"An error occurred while deleting the employee with ID {id}.", exception);
            }
            catch (Exception exception)
            {
                throw new DataException("Unexpected error while deleting employee.", exception);
            }
        }

        // Maps a SqlDataReader row to an Employee object for GetAll and GetByID methods.
        private static Employee MapEmployee(SqlDataReader reader)
        {
            return new Employee
            {
                EmployeeID = reader.GetInt32(reader.GetOrdinal("EmployeeID")),
                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                LastName = reader.GetString(reader.GetOrdinal("LastName")),
                Email = reader.GetString(reader.GetOrdinal("Email")),
                DepartmentID = reader.GetInt32(reader.GetOrdinal("DepartmentID")),
                RoleID = reader.GetInt32(reader.GetOrdinal("RoleID")),
                TerminationDate = reader.IsDBNull(reader.GetOrdinal("TerminationDate"))? null
                    : DateOnly.FromDateTime(reader.GetDateTime(reader.GetOrdinal("TerminationDate")))
            };
        }

        // Adds parameters for Employee to a SqlCommand for Add and Update methods.
        private static void AddEmployeeParameters(SqlCommand command, Employee employee)
        {
            command.Parameters.Add("@FirstName", SqlDbType.NVarChar, 100).Value = employee.FirstName;
            command.Parameters.Add("@LastName", SqlDbType.NVarChar, 100).Value = employee.LastName;
            command.Parameters.Add("@Email", SqlDbType.NVarChar, 255).Value = employee.Email;
            command.Parameters.Add("@DepartmentID", SqlDbType.Int).Value = employee.DepartmentID;
            command.Parameters.Add("@RoleID", SqlDbType.Int).Value = employee.RoleID;

            command.Parameters.Add("@TerminationDate", SqlDbType.Date).Value =
                employee.TerminationDate.HasValue
                    ? employee.TerminationDate.Value.ToDateTime(TimeOnly.MinValue)
                    : (object)DBNull.Value;
        }
    }
}
