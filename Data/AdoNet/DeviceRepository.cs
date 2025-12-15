using System;
using System.Collections.Generic;
using System.Data;
using Application.Interfaces.Repository;
using Application.Models;
using Microsoft.Data.SqlClient;

namespace Data.AdoNet
{
    public class DeviceRepository : IRepository<Device>
    {
        // Holds the database connection dependency
        private readonly DatabaseConnection _databaseConnection;

        // Constructor injection of the DatabaseConnection
        public DeviceRepository(DatabaseConnection databaseConnection)
        {
            _databaseConnection = databaseConnection;
        }

        // Stored procedure names
        private const string SpGetAllDevices = "uspGetAllDevices";
        private const string SpGetDeviceByID = "uspGetDeviceByID";
        private const string SpAddDevice = "uspAddDevice";
        private const string SpUpdateDevice = "uspUpdateDevice";
        private const string SpDeleteDevice = "uspDeleteDevice";

        // Retrieves all devices from the database using a stored procedure
        public IEnumerable<Device> GetAll()
        {
            var devices = new List<Device>();

            try
            {
                using var connection = _databaseConnection.CreateConnection();
                using var command = new SqlCommand(SpGetAllDevices, connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                connection.Open();
                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    devices.Add(new Device
                    {
                        DeviceID = reader.GetInt32(reader.GetOrdinal("DeviceID")),
                        DeviceDescriptionID = reader.GetInt32(reader.GetOrdinal("DeviceDescriptionID")),
                        Status = (DeviceStatus)reader.GetInt32(reader.GetOrdinal("DeviceStatus")),
                        IsWiped = reader.GetBoolean(reader.GetOrdinal("IsWiped")),
                        PurchaseDate = DateOnly.FromDateTime(reader.GetDateTime(reader.GetOrdinal("PurchaseDate"))),
                        ExpectedEndDate = DateOnly.FromDateTime(reader.GetDateTime(reader.GetOrdinal("ExpectedEndDate")))
                    });
                }

                return devices;
            }
            catch (SqlException exception)
            {
                // Wraps SQL exceptions in a DataException with a clear message
                throw new DataException("An error occurred while retrieving devices from the database.", exception);
            }
        }

        // Retrieves a single device by its ID using a stored procedure
        public Device? GetByID(int id)
        {
            if (id <= 0)
                throw new ArgumentOutOfRangeException(nameof(id), "DeviceID must be greater than zero.");

            try
            {
                using var connection = _databaseConnection.CreateConnection();
                using var command = new SqlCommand(SpGetDeviceByID, connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                // Sends the device ID as a parameter to the stored procedure
                command.Parameters.Add("@DeviceID", SqlDbType.Int).Value = id;

                connection.Open();
                using var reader = command.ExecuteReader();

                if (reader.Read())
                {
                    // Builds and returns a Device object from the current record
                    return new Device
                    {
                        DeviceID = reader.GetInt32(reader.GetOrdinal("DeviceID")),
                        DeviceDescriptionID = reader.GetInt32(reader.GetOrdinal("DeviceDescriptionID")),
                        Status = (DeviceStatus)reader.GetInt32(reader.GetOrdinal("DeviceStatus")),
                        IsWiped = reader.GetBoolean(reader.GetOrdinal("IsWiped")),
                        PurchaseDate = DateOnly.FromDateTime(reader.GetDateTime(reader.GetOrdinal("PurchaseDate"))),
                        ExpectedEndDate = DateOnly.FromDateTime(reader.GetDateTime(reader.GetOrdinal("ExpectedEndDate")))
                    };
                }

                // Returns null when no device with the given ID is found
                return null;
            }
            catch (SqlException exception)
            {
                // Wraps SQL exceptions in a DataException with a clear message
                throw new DataException($"An error occurred while retrieving the device with ID {id} from the database.", exception);
            }
        }

        // Adds a new device to the database using a stored procedure
        public void Add(Device device)
        {
            if (device is null)
                throw new ArgumentNullException(nameof(device));

            try
            {
                using var connection = _databaseConnection.CreateConnection();
                using var command = new SqlCommand(SpAddDevice, connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                command.Parameters.Add("@DeviceDescriptionID", SqlDbType.Int).Value = device.DeviceDescriptionID;
                command.Parameters.Add("@DeviceStatus", SqlDbType.Int).Value = (int)device.Status;
                command.Parameters.Add("@IsWiped", SqlDbType.Bit).Value = device.IsWiped;
                command.Parameters.Add("@PurchaseDate", SqlDbType.Date).Value =
                    device.PurchaseDate.ToDateTime(new TimeOnly(0, 0));
                command.Parameters.Add("@ExpectedEndDate", SqlDbType.Date).Value =
                    device.ExpectedEndDate.ToDateTime(new TimeOnly(0, 0));

                connection.Open();

                var result = command.ExecuteScalar();

                // Validates that an ID was returned
                if (result == null || result == DBNull.Value)
                    throw new KeyNotFoundException("Could not get generated DeviceID from stored procedure.");

                device.DeviceID = Convert.ToInt32(result);
            }
            catch (SqlException exception)
            {
                throw new DataException("An error occurred while adding a new device to the database.", exception);
            }
        }

        // Updates an existing device in the database using a stored procedure
        public void Update(Device device)
        {
            if (device is null)
                throw new ArgumentNullException(nameof(device));

            if (device.DeviceID <= 0)
                throw new ArgumentOutOfRangeException(nameof(device.DeviceID), "DeviceID must be greater than zero.");

            try
            {
                using var connection = _databaseConnection.CreateConnection();
                using var command = new SqlCommand(SpUpdateDevice, connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                command.Parameters.Add("@DeviceID", SqlDbType.Int).Value = device.DeviceID;
                command.Parameters.Add("@DeviceDescriptionID", SqlDbType.Int).Value = device.DeviceDescriptionID;
                command.Parameters.Add("@DeviceStatus", SqlDbType.Int).Value = (int)device.Status;
                command.Parameters.Add("@IsWiped", SqlDbType.Bit).Value = device.IsWiped;
                command.Parameters.Add("@PurchaseDate", SqlDbType.Date).Value =
                    device.PurchaseDate.ToDateTime(new TimeOnly(0, 0));
                command.Parameters.Add("@ExpectedEndDate", SqlDbType.Date).Value =
                    device.ExpectedEndDate.ToDateTime(new TimeOnly(0, 0));

                connection.Open();

                // Executes the update and checks how many rows were affected
                var rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected == 0)
                    throw new KeyNotFoundException($"No device found with ID {device.DeviceID} to update.");
            }
            catch (SqlException exception)
            {
                throw new DataException($"An error occurred while updating the device with ID {device.DeviceID} in the database.", exception);
            }
        }

        // Deletes a device from the database using a stored procedure
        public void Delete(int id)
        {
            if (id <= 0)
                throw new ArgumentOutOfRangeException(nameof(id), "DeviceID must be greater than zero.");

            try
            {
                using var connection = _databaseConnection.CreateConnection();
                using var command = new SqlCommand(SpDeleteDevice, connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                command.Parameters.Add("@DeviceID", SqlDbType.Int).Value = id;

                connection.Open();
                var rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected == 0)
                    throw new KeyNotFoundException($"No device found with ID {id} to delete.");
            }
            catch (SqlException exception)
            {
                throw new DataException($"An error occurred while deleting the device with ID {id} from the database.", exception);
            }
        }
    }
}
