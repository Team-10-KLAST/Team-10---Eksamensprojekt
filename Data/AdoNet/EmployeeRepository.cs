using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using Application.Interfaces.Repository;
using Application.Models;
using Data;
using Application.Interfaces;

namespace Data.AdoNet
{
    public class EmployeeRepository : IRepository<Employee>
    {
        // Dependency on DatabaseConnection, used to create SQL connections.
        private readonly DatabaseConnection _databaseConnection;

        private const string SpAddEmployee = "uspAddEmployee";
        private const string SpUpdateEmployee = "uspUpdateEmployee";
        private const string SpDeleteEmployee = "uspDeleteEmployee";
        private const string SpGetAllEmployees = "uspGetAllEmployees";
        private const string SpGetEmployeeByID = "uspGetEmployeeByID";

        public EmployeeRepository(DatabaseConnection databaseConnection)
        {
            _databaseConnection = databaseConnection;
        }

        // Adds a new employee using a stored procedure.
        public void Add(Employee employee)
        {
            try
            {
                using (var connection = _databaseConnection.CreateConnection())
                using (var command = new SqlCommand(SpAddEmployee, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.AddWithValue("@FirstName", employee.FirstName);
                    command.Parameters.AddWithValue("@LastName", employee.LastName);
                    command.Parameters.AddWithValue("@Email", employee.Email);
                    command.Parameters.AddWithValue("@DepartmentID", employee.DepartmentID);
                    command.Parameters.AddWithValue("@RoleID", employee.RoleID);
                    command.Parameters.AddWithValue("@TerminationDate",
                        employee.TerminationDate?.ToDateTime(TimeOnly.MinValue) ?? (object)DBNull.Value);

                    connection.Open();

                    var result = command.ExecuteScalar();

                    if (result == null || result == DBNull.Value)
                    {
                        throw new DataException("Could not get generated EmployeeId from stored procedure.");
                    }

                    employee.EmployeeID = Convert.ToInt32(result);
                }
            }
            catch (SqlException ex)
            {
                throw new DataException("Database error while adding Employee.", ex);
            }
            catch (Exception ex)
            {
                throw new DataException("Unexpected error while adding Employee.", ex);
            }
        }

        // Updates an existing employee using a stored procedure.
        public void Update(Employee employee)
        {
            try
            {
                using (var connection = _databaseConnection.CreateConnection())
                using (var command = new SqlCommand(SpUpdateEmployee, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.AddWithValue("@EmployeeID", employee.EmployeeID);
                    command.Parameters.AddWithValue("@FirstName", employee.FirstName);
                    command.Parameters.AddWithValue("@LastName", employee.LastName);
                    command.Parameters.AddWithValue("@Email", employee.Email);
                    command.Parameters.AddWithValue("@DepartmentID", employee.DepartmentID);
                    command.Parameters.AddWithValue("@RoleID", employee.RoleID);
                    command.Parameters.AddWithValue("@TerminationDate",
                        employee.TerminationDate?.ToDateTime(TimeOnly.MinValue) ?? (object)DBNull.Value);

                    connection.Open();

                    var affected = command.ExecuteNonQuery();

                    if (affected == 0)
                    {
                        throw new DataException($"No Employee with Id {employee.EmployeeID} to update.");
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new DataException("Database error while updating Employee.", ex);
            }
            catch (Exception ex)
            {
                throw new DataException("Unexpected error while updating Employee.", ex);
            }
        }

        // Deletes an employee by ID using a stored procedure.
        public void Delete(int id)
        {
            try
            {
                using (var connection = _databaseConnection.CreateConnection())
                using (var command = new SqlCommand(SpDeleteEmployee, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.AddWithValue("@EmployeeID", id);

                    connection.Open();

                    var affected = command.ExecuteNonQuery();

                    if (affected == 0)
                    {
                        throw new DataException($"No Employee with Id {id} to delete.");
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new DataException("Database error while deleting Employee.", ex);
            }
            catch (Exception ex)
            {
                throw new DataException("Unexpected error while deleting Employee.", ex);
            }
        }

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
            catch (SqlException ex)
            {
                throw new DataException("Database error while reading all Employees.", ex);
            }
            catch (Exception ex)
            {
                throw new DataException("Unexpected error while reading all Employees.", ex);
            }
        }

        // GET BY ID: Returns a single employee (or null) using a stored procedure.
        public Employee? GetByID(int id)
        {
            try
            {
                using (var connection = _databaseConnection.CreateConnection())
                using (var command = new SqlCommand(SpGetEmployeeByID, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.AddWithValue("@EmployeeID", id);

                    connection.Open();

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapEmployee(reader);
                        }
                    }
                }

                return null;
            }
            catch (SqlException ex)
            {
                throw new DataException("Database error while reading Employee by ID.", ex);
            }
            catch (Exception ex)
            {
                throw new DataException("Unexpected error while reading Employee by ID.", ex);
            }
        }

        // Helper method to map a SqlDataReader row to an Employee object.
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
    }
}
