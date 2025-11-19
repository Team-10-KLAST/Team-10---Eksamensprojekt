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
    public class RoleRepository : IRepository<Role>
    {
        private readonly DatabaseConnection _databaseConnection;

        public RoleRepository(DatabaseConnection databaseConnection)
        {
            _databaseConnection = databaseConnection;
        }

        public IEnumerable<Role> GetAll()
        {
            const string procedure = "uspGetAllRoles";
            var roles = new List<Role>();

            try
            {
                using var connection = _databaseConnection.CreateConnection();
                using var command = new SqlCommand(procedure, connection)
                {
                    CommandType = System.Data.CommandType.StoredProcedure
                };

                connection.Open();
                using (var reader = command.ExecuteReader())
                    while (reader.Read())
                    {
                        roles.Add(new Role
                        {
                            RoleId = reader.GetInt32(reader.GetOrdinal("RoleID")),
                            Name = reader.GetString(reader.GetOrdinal("RoleName"))
                        });
                    }
                return roles;
            }
            catch (SqlException exception)
            {
                throw new Exception("An error occurred while retrieving roles from the database.", exception);
            }
            catch (Exception exception)
            {
                throw new Exception("An unexpected error occurred in RoleRepository.GetAll().", exception);
            }
        }

        // Not implemented methods. Can be implemented later as needed.
        void IRepository<Role>.Add(Role entity) => throw new NotImplementedException();
        void IRepository<Role>.Delete(int id) => throw new NotImplementedException();
        Role? IRepository<Role>.GetById(int id) => throw new NotImplementedException();
        void IRepository<Role>.Update(Role entity) => throw new NotImplementedException();
    }
}
