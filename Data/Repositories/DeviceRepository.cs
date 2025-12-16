using System;
using System.Collections.Generic;
using System.Data;
using Application.Interfaces.Repository;
using Application.Models;
using Microsoft.Data.SqlClient;

namespace Data.Repositories
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

        // Retrieves all devices from the database
        public IEnumerable<Device> GetAll()
        {
            var devices = new List<Device>();

            try
            {
                using (var connection = _databaseConnection.CreateConnection())
                using (var command = new SqlCommand(SpGetAllDevices, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    connection.Open();

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            devices.Add(MapDevice(reader));
                        }
                    }
                }
                return devices;
            }
            catch (SqlException exception)
            {
                throw new DataException("An error occurred while retrieving devices from the database.", exception);
            }
            catch (Exception exception)
            {
                throw new DataException("Unexpected error while retrieving devices.", exception);
            }
        }

        // Retrieves a single device by ID
        public Device? GetByID(int id)
        {
            if (id <= 0)
                throw new ArgumentOutOfRangeException(nameof(id), "DeviceID must be greater than zero.");

            try
            {
                using (var connection = _databaseConnection.CreateConnection())
                using (var command = new SqlCommand(SpGetDeviceByID, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add("@DeviceID", SqlDbType.Int).Value = id;

                    connection.Open();

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapDevice(reader);
                        }
                    }
                }
                return null;
            }
            catch (SqlException exception)
            {
                throw new DataException($"An error occurred while retrieving the device with ID {id}.", exception);
            }
            catch (Exception exception)
            {
                throw new DataException("Unexpected error while retrieving device by ID.", exception);
            }
        }

        // Adds a new device
        public void Add(Device device)
        {
            if (device is null)
                throw new ArgumentNullException(nameof(device));

            try
            {
                using (var connection = _databaseConnection.CreateConnection())
                using (var command = new SqlCommand(SpAddDevice, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    AddDeviceParameters(command, device);

                    connection.Open();

                    var result = command.ExecuteScalar();

                    if (result == null || result == DBNull.Value)
                        throw new KeyNotFoundException("Could not get generated DeviceID from stored procedure.");

                    device.DeviceID = Convert.ToInt32(result);
                }
            }
            catch (SqlException exception)
            {
                throw new DataException("An error occurred while adding a new device to the database.", exception);
            }
            catch (Exception exception)
            {
                throw new DataException("Unexpected error while adding device.", exception);
            }
        }

        // Updates an existing device
        public void Update(Device device)
        {
            if (device is null)
                throw new ArgumentNullException(nameof(device));

            if (device.DeviceID <= 0)
                throw new ArgumentOutOfRangeException(nameof(device.DeviceID), "DeviceID must be greater than zero.");

            try
            {
                using (var connection = _databaseConnection.CreateConnection())
                using (var command = new SqlCommand(SpUpdateDevice, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.Add("@DeviceID", SqlDbType.Int).Value = device.DeviceID;
                    AddDeviceParameters(command, device);

                    connection.Open();

                    var rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected == 0)
                        throw new KeyNotFoundException($"No device found with ID {device.DeviceID} to update.");
                }
            }
            catch (SqlException exception)
            {
                throw new DataException($"An error occurred while updating the device with ID {device.DeviceID}.", exception);
            }
            catch (Exception exception)
            {
                throw new DataException("Unexpected error while updating device.", exception);
            }
        }

        // Deletes a device
        public void Delete(int id)
        {
            if (id <= 0)
                throw new ArgumentOutOfRangeException(nameof(id), "DeviceID must be greater than zero.");

            try
            {
                using (var connection = _databaseConnection.CreateConnection())
                using (var command = new SqlCommand(SpDeleteDevice, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add("@DeviceID", SqlDbType.Int).Value = id;

                    connection.Open();

                    var rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected == 0)
                        throw new KeyNotFoundException($"No device found with ID {id} to delete.");
                }
            }
            catch (SqlException exception)
            {
                throw new DataException($"An error occurred while deleting the device with ID {id}.", exception);
            }
            catch (Exception exception)
            {
                throw new DataException("Unexpected error while deleting device.", exception);
            }
        }

        // Maps a SqlDataReader row to a Device object for GetAll and GetByID methods
        private static Device MapDevice(SqlDataReader reader)
        {
            var purchaseDateOrdinal = reader.GetOrdinal("PurchaseDate");
            var expectedEndDateOrdinal = reader.GetOrdinal("ExpectedEndDate");

            DateOnly? purchaseDate = reader.IsDBNull(purchaseDateOrdinal)
                ? null
                : DateOnly.FromDateTime(reader.GetDateTime(purchaseDateOrdinal));

            DateOnly? expectedEndDate = reader.IsDBNull(expectedEndDateOrdinal)
                ? null
                : DateOnly.FromDateTime(reader.GetDateTime(expectedEndDateOrdinal));

            return new Device
            {
                DeviceID = reader.GetInt32(reader.GetOrdinal("DeviceID")),
                DeviceDescriptionID = reader.GetInt32(reader.GetOrdinal("DeviceDescriptionID")),
                Status = (DeviceStatus)reader.GetInt32(reader.GetOrdinal("DeviceStatus")),
                IsWiped = reader.GetBoolean(reader.GetOrdinal("IsWiped")),
                PurchaseDate = purchaseDate,
                ExpectedEndDate = expectedEndDate
            };
        }

        // Adds parameters for Device to a SqlCommand for Add and Update methods
        private static void AddDeviceParameters(SqlCommand command, Device device)
        {
            command.Parameters.Add("@DeviceDescriptionID", SqlDbType.Int).Value = device.DeviceDescriptionID;
            command.Parameters.Add("@DeviceStatus", SqlDbType.Int).Value = (int)device.Status;
            command.Parameters.Add("@IsWiped", SqlDbType.Bit).Value = device.IsWiped;

            command.Parameters.Add("@PurchaseDate", SqlDbType.Date).Value =
                device.PurchaseDate.HasValue
                    ? device.PurchaseDate.Value.ToDateTime(TimeOnly.MinValue)
                    : (object)DBNull.Value;

            command.Parameters.Add("@ExpectedEndDate", SqlDbType.Date).Value =
                device.ExpectedEndDate.HasValue
                    ? device.ExpectedEndDate.Value.ToDateTime(TimeOnly.MinValue)
                    : (object)DBNull.Value;
        }
    }
}
