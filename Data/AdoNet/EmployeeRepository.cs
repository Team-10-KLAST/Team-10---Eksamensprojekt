using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using Application.Interfaces.Repository;
using Application.Models;
using Data;

namespace Data.AdoNet
{
    public class EmployeeRepository : IRepository<Employee>
    {
        // Shared DatabaseConnection singleton.
        private readonly DatabaseConnection _databaseConnection;
        
        // Constructor with DatabaseConnection parameter.
        public EmployeeRepository(DatabaseConnection databaseConnection)
        {
            _databaseConnection = databaseConnection;
        }

        // Insert new employee and set generated EmployeeId
        public void Add(Employee employee)
        {
            const string sql = @"
                INSERT INTO Employee (FirstName, LastName, Email, DepartmentID, RoleID)
                VALUES (@FirstName, @LastName, @Email, @DepartmentID, @RoleID);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            // Create DB connection from DatabaseConnection.
            using (var connection = _databaseConnection.CreateConnection())
            using (var command = new SqlCommand(sql, connection))
            {
                // Connects the Employee object with the SQL statement..
                command.Parameters.AddWithValue("@FirstName", employee.FirstName);
                command.Parameters.AddWithValue("@LastName", employee.LastName);
                command.Parameters.AddWithValue("@Email", employee.Email);
                command.Parameters.AddWithValue("@DepartmentID", employee.DepartmentId);
                command.Parameters.AddWithValue("@RoleID", employee.RoleId);

                connection.Open();

                // Get new identity value (EmployeeID).
                var result = command.ExecuteScalar();
                if (result == null)
                    throw new DataException("Could not get generated EmployeeId.");

                employee.EmployeeId = Convert.ToInt32(result);
            }
        }

        // Update existing employee by EmployeeId.
        public void Update(Employee employee)
        {
            const string sql = @"
                UPDATE Employee
                SET FirstName    = @FirstName,
                    LastName     = @LastName,
                    Email        = @Email,
                    DepartmentID = @DepartmentID,
                    RoleID       = @RoleID
                WHERE EmployeeID = @EmployeeID;";

            using (var connection = _databaseConnection.CreateConnection())
            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@FirstName", employee.FirstName);
                command.Parameters.AddWithValue("@LastName", employee.LastName);
                command.Parameters.AddWithValue("@Email", employee.Email);
                command.Parameters.AddWithValue("@DepartmentID", employee.DepartmentId);
                command.Parameters.AddWithValue("@RoleID", employee.RoleId);
                command.Parameters.AddWithValue("@EmployeeID", employee.EmployeeId);

                connection.Open();

                // Number of affected rows.
                var affected = command.ExecuteNonQuery();
                if (affected == 0)
                    // No row updated => id did not exist.
                    throw new DataException($"No Employee with Id {employee.EmployeeId} to update.");
            }
        }

        // Delete employee by EmployeeId.
        public void Delete(int id)
        {
            const string sql = @"DELETE FROM Employee WHERE EmployeeID = @EmployeeID;";

            using (var connection = _databaseConnection.CreateConnection())
            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@EmployeeID", id);

                connection.Open();

                var affected = command.ExecuteNonQuery();
                if (affected == 0)
                    // Id did not exist in table.
                    throw new DataException($"No Employee with Id {id} to delete.");
            }
        }

        // Get all employees.
        public IEnumerable<Employee> GetAll()
        {
            const string sql = @"
                SELECT EmployeeID, FirstName, LastName, Email, DepartmentID, RoleID
                FROM Employee;";

            var employees = new List<Employee>();

            using (var connection = _databaseConnection.CreateConnection())
            using (var command = new SqlCommand(sql, connection))
            {
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

        // Get one employee by EmployeeId.
        public Employee? GetById(int id)
        {
            const string sql = @"
                SELECT EmployeeID, FirstName, LastName, Email, DepartmentID, RoleID
                FROM Employee
                WHERE EmployeeID = @EmployeeID;";

            using (var connection = _databaseConnection.CreateConnection())
            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@EmployeeID", id);

                connection.Open();

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                        return MapEmployee(reader);
                }
            }

            // Not found.
            return null;
        }

        // Take one row from the database and turn it into a single Employee object.
        private static Employee MapEmployee(SqlDataReader reader)
        {
            // GetOrdinal finds the column index by name.
            return new Employee
            {
                EmployeeId = reader.GetInt32(reader.GetOrdinal("EmployeeID")),
                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                LastName = reader.GetString(reader.GetOrdinal("LastName")),
                Email = reader.GetString(reader.GetOrdinal("Email")),
                DepartmentId = reader.GetInt32(reader.GetOrdinal("DepartmentID")),
                RoleId = reader.GetInt32(reader.GetOrdinal("RoleID"))
            };
        }
    }
}
