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
    public class DeviceDescriptionRepository : IRepository<DeviceDescription>
    {
        // Holds the database connection dependency
        private readonly DatabaseConnection _databaseConnection;

        // Constructor injection of the DatabaseConnection
        public DeviceDescriptionRepository(DatabaseConnection databaseConnection)
        {
            _databaseConnection = databaseConnection;
        }

        //Stored procedure names
        private const string SpGetAllDeviceDescriptions = "uspGetAllDeviceDescriptions";
        private const string SpGetDeviceDescriptionByID = "uspGetDeviceDescriptionByID";

        // Retrieves all device descriptions
        public IEnumerable<DeviceDescription> GetAll()
        {
            var deviceDescriptions = new List<DeviceDescription>();

            try
            {
                using (var connection = _databaseConnection.CreateConnection())
                using (var command = new SqlCommand(SpGetAllDeviceDescriptions, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    connection.Open();

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            deviceDescriptions.Add(MapDeviceDescription(reader));
                        }
                    }
                }
                return deviceDescriptions;
            }
            catch (SqlException exception)
            {
                throw new DataException("An error occurred while retrieving device descriptions from the database.", exception);
            }
            catch (Exception exception)
            {
                throw new DataException("Unexpected error while retrieving device descriptions.", exception);
            }
        }

        // Retrieves a device description by ID
        public DeviceDescription? GetByID(int id)
        {
            if (id <= 0)
                throw new ArgumentOutOfRangeException(nameof(id), "DeviceDescriptionID must be greater than zero.");

            try
            {
                using (var connection = _databaseConnection.CreateConnection())
                using (var command = new SqlCommand(SpGetDeviceDescriptionByID, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add("@DeviceDescriptionID", SqlDbType.Int).Value = id;

                    connection.Open();

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapDeviceDescription(reader);
                        }
                    }
                }
                return null;
            }
            catch (SqlException exception)
            {
                throw new DataException($"An error occurred while retrieving the device description with ID {id}.", exception);
            }
            catch (Exception exception)
            {
                throw new DataException("Unexpected error while retrieving device description by ID.", exception);
            }
        }

        // Maps a SqlDataReader row to a DeviceDescription object for GetAll and GetByID methods
        private static DeviceDescription MapDeviceDescription(SqlDataReader reader)
        {
            return new DeviceDescription
            {
                DeviceDescriptionID = reader.GetInt32(reader.GetOrdinal("DeviceDescriptionID")),
                DeviceType = reader.GetString(reader.GetOrdinal("DeviceType")),
                OperatingSystem = reader.GetString(reader.GetOrdinal("OperatingSystem")),
                Location = reader.GetString(reader.GetOrdinal("Location"))
            };
        }

        // Not yet implemented methods. Can be implemented later as needed.
        public void Add(DeviceDescription entity) => throw new NotImplementedException();
        public void Delete(int id) => throw new NotImplementedException();
        public void Update(DeviceDescription entity) => throw new NotImplementedException();
    }
}
