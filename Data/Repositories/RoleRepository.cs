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
    public class RoleRepository : IRepository<Role>
    {
        // Holds the database connection dependency
        private readonly DatabaseConnection _databaseConnection;

        // Constructor injection of the DatabaseConnection
        public RoleRepository(DatabaseConnection databaseConnection)
        {
            _databaseConnection = databaseConnection;
        }

        // Stored procedure name
        private const string SpGetAllRoles = "uspGetAllRoles";

        // Retrieves all roles
        public IEnumerable<Role> GetAll()
        {
            var roles = new List<Role>();

            try
            {
                using (var connection = _databaseConnection.CreateConnection())
                using (var command = new SqlCommand(SpGetAllRoles, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    connection.Open();

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            roles.Add(new Role
                            {
                                RoleID = reader.GetInt32(reader.GetOrdinal("RoleID")),
                                Name = reader.GetString(reader.GetOrdinal("RoleName"))
                            });
                        }
                    }
                }
                return roles;
            }
            catch (SqlException exception)
            {
                throw new DataException("An error occurred while retrieving roles from the database.", exception);
            }
            catch (Exception exception)
            {
                throw new DataException("Unexpected error while retrieving roles.", exception);
            }
        }

        // Not yet implemented methods. Can be implemented later as needed.
        void IRepository<Role>.Add(Role entity) => throw new NotImplementedException();
        void IRepository<Role>.Delete(int id) => throw new NotImplementedException();
        Role? IRepository<Role>.GetByID(int id) => throw new NotImplementedException();
        void IRepository<Role>.Update(Role entity) => throw new NotImplementedException();
    }
}
