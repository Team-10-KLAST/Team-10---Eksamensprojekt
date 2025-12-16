using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Interfaces.Repository;
using Application.Models;
using Microsoft.Data.SqlClient;

namespace Data.Repositories
{
    public class DepartmentRepository : IRepository<Department>
    {
        // Holds the database connection dependency
        private readonly DatabaseConnection _databaseConnection;

        // Constructor injection of the DatabaseConnection
        public DepartmentRepository(DatabaseConnection databaseConnection)
        {
            _databaseConnection = databaseConnection;
        }

        // Stored procedure name
        private const string SpGetAllDepartments = "uspGetAllDepartments";


        // Retrieves all departments
        public IEnumerable<Department> GetAll()
        {
            var departments = new List<Department>();

            try
            {
                using (var connection = _databaseConnection.CreateConnection())
                using (var command = new SqlCommand(SpGetAllDepartments, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    connection.Open();

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            departments.Add(new Department
                            {
                                DepartmentID = reader.GetInt32(reader.GetOrdinal("DepartmentID")),
                                Name = reader.GetString(reader.GetOrdinal("DepartmentName"))
                            });
                        }
                    }
                }
                return departments;
            }
            catch (SqlException exception)
            {
                throw new DataException("An error occurred while retrieving departments from the database.", exception);
            }
            catch (Exception exception)
            {
                throw new DataException("Unexpected error while retrieving departments.", exception);
            }
        }

        // Not yet implemented methods. Can be implemented later as needed.
        void IRepository<Department>.Add(Department entity) => throw new NotImplementedException();
        void IRepository<Department>.Delete(int id) => throw new NotImplementedException();
        Department? IRepository<Department>.GetByID(int id) => throw new NotImplementedException();
        void IRepository<Department>.Update(Department entity) => throw new NotImplementedException();
    }
}
