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
        private const string SpGetDeviceDescriptionById = "uspGetDeviceDescriptionById";

        // Retrieves all devicedescriptions from the database using a stored procedure
        public IEnumerable<DeviceDescription> GetAll()
        {
            var deviceDescriptions = new List<DeviceDescription>();

            try
            {
                using var connection = _databaseConnection.CreateConnection();
                using var command = new SqlCommand(SpGetAllDeviceDescriptions, connection)
                {
                    CommandType = System.Data.CommandType.StoredProcedure
                };

                connection.Open();
                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    deviceDescriptions.Add(new DeviceDescription
                    {
                        DeviceDescriptionID = reader.GetInt32(reader.GetOrdinal("DeviceDescriptionID")),
                        DeviceType = reader.GetString(reader.GetOrdinal("DeviceType")),
                        OperatingSystem = reader.GetString(reader.GetOrdinal("OperatingSystem")),
                        Location = reader.GetString(reader.GetOrdinal("Location"))
                    });

                }
                return deviceDescriptions;
            }
            catch (SqlException exception)
            {
                throw new DataException("An error occurred while retrieving device descriptions from the database.", exception);
            }
        }

        // Retrieves a devicedescription by its ID using a stored procedure
        public DeviceDescription? GetByID(int id)
        {
            if (id <= 0)
                throw new ArgumentException("The ID must be a positive integer.", nameof(id));

            try
            {
                using var connection = _databaseConnection.CreateConnection();
                using var command = new SqlCommand(SpGetDeviceDescriptionById, connection)
                {
                    CommandType = System.Data.CommandType.StoredProcedure
                };
                command.Parameters.Add("@DeviceDescriptionID", SqlDbType.Int).Value = id;

                connection.Open();
                using var reader = command.ExecuteReader();

                if (reader.Read())
                {
                    return new DeviceDescription
                    {
                        DeviceDescriptionID = reader.GetInt32(reader.GetOrdinal("DeviceDescriptionID")),
                        DeviceType = reader.GetString(reader.GetOrdinal("DeviceType")),
                        OperatingSystem = reader.GetString(reader.GetOrdinal("OperatingSystem")),
                        Location = reader.GetString(reader.GetOrdinal("Location"))
                    };
                }
                return null;
            }
            catch (SqlException exception)
            {
                throw new DataException($"An error occurred while retrieving the device description with ID {id} from the database.", exception);
            }
        }

        // Not yet implemented methods. Can be implemented later as needed.
        public void Add(DeviceDescription entity) => throw new NotImplementedException();
        public void Delete(int id) => throw new NotImplementedException();
        public void Update(DeviceDescription entity) => throw new NotImplementedException();
    }
}
