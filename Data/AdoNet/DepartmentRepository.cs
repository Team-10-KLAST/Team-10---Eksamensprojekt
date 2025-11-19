using System;
using System.Collections.Generic;
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
        private readonly DatabaseConnection _databaseConnection;

        public DepartmentRepository(DatabaseConnection databaseConnection)
        {
            _databaseConnection = databaseConnection;
        }

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
                        DepartmentId = reader.GetInt32(reader.GetOrdinal("DepartmentID")),
                        Name = reader.GetString(reader.GetOrdinal("DepartmentName"))
                    });
                }
                return departments;
            }
            catch (SqlException exception)
            {
                throw new Exception("An error occurred while retrieving departments from the database.", exception);
            }
            catch (Exception exception)
            {
                throw new Exception("An unexpected error occurred in DepartmentRepository.GetAll().", exception);
            }
        }

        // Not implemented methods. Can be implemented later as needed.
        void IRepository<Department>.Add(Department entity) => throw new NotImplementedException();
        void IRepository<Department>.Delete(int id) => throw new NotImplementedException();
        Department? IRepository<Department>.GetById(int id) => throw new NotImplementedException();
        void IRepository<Department>.Update(Department entity) => throw new NotImplementedException();
    }

}
