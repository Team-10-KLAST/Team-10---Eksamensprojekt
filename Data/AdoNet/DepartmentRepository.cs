using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Interfaces.Repository;
using Application.Models;
using Microsoft.Data.SqlClient;

namespace Data.AdoNet
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

        // Retrieves all departments from the database
        public IEnumerable<Department> GetAll()
        {
            const string procedure = "uspGetAllDepartments";
            var departments = new List<Department>();

            try
            {
                using var connection = _databaseConnection.CreateConnection();
                using var command = new SqlCommand(procedure, connection)
                {
                    CommandType = System.Data.CommandType.StoredProcedure
                };

                connection.Open();
                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    departments.Add(new Department
                    {
                        DepartmentID = reader.GetInt32(reader.GetOrdinal("DepartmentID")),
                        Name = reader.GetString(reader.GetOrdinal("DepartmentName"))
                    });
                }
                return departments;
            }
            catch (SqlException exception)
            {
                throw new DataException("An error occurred while retrieving departments from the database.", exception);
            }
        }

        // Not yet implemented methods. Can be implemented later as needed.
        void IRepository<Department>.Add(Department entity) => throw new NotImplementedException();
        void IRepository<Department>.Delete(int id) => throw new NotImplementedException();
        Department? IRepository<Department>.GetByID(int id) => throw new NotImplementedException();
        void IRepository<Department>.Update(Department entity) => throw new NotImplementedException();
    }

}
