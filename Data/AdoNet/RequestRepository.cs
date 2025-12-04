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
    public class RequestRepository : IRepository<Request>
    {
        // Holds the database connection dependency
        private readonly DatabaseConnection _databaseConnection;

        // Constructor injection of the DatabaseConnection
        public RequestRepository(DatabaseConnection databaseConnection)
        {
            _databaseConnection = databaseConnection;
        }

        //Stored procedure names
        private const string SpGetAllRequests = "uspGetAllRequests";
        private const string SpGetRequestByID = "uspGetRequestByID";
        private const string SpAddRequest = "uspAddRequest";
        private const string SpUpdateRequest = "uspUpdateRequest";
        private const string SpDeleteRequest = "uspDeleteRequest";

        // Retrieves all requests from the database using a stored procedure
        public IEnumerable<Request> GetAll()
        {
            var requests = new List<Request>();

            try
            {
                using var connection = _databaseConnection.CreateConnection();
                using var command = new SqlCommand(SpGetAllRequests, connection)
                {
                    CommandType = System.Data.CommandType.StoredProcedure
                };

                connection.Open();
                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    requests.Add(new Request
                    {
                        RequestID = reader.GetInt32(reader.GetOrdinal("RequestID")),
                        RequestDate = DateOnly.FromDateTime(reader.GetDateTime(reader.GetOrdinal("RequestDate"))),
                        Justification = reader.GetString(reader.GetOrdinal("Justification")),
                        Status = (RequestStatus)reader.GetInt32(reader.GetOrdinal("RequestStatus"))
                    });
                }
                return requests;
            }
            catch (SqlException exception)
            {
                throw new DataException("An error occurred while retrieving requests from the database.", exception);
            }
        }

        // Retrieves a request by its ID using a stored procedure
        public Request? GetByID(int id)
        {
            if (id <= 0)
                throw new ArgumentOutOfRangeException(nameof(id), "RequestID must be greater than zero.");

            try
            {
                using var connection = _databaseConnection.CreateConnection();
                using var command = new SqlCommand(SpGetRequestByID, connection)
                {
                    CommandType = System.Data.CommandType.StoredProcedure
                };
                command.Parameters.Add("@RequestID", SqlDbType.Int).Value = id;

                connection.Open();
                using var reader = command.ExecuteReader();

                if (reader.Read())
                {
                    return new Request
                    {
                        RequestID = reader.GetInt32(reader.GetOrdinal("RequestID")),
                        RequestDate = DateOnly.FromDateTime(reader.GetDateTime(reader.GetOrdinal("RequestDate"))),
                        Justification = reader.GetString(reader.GetOrdinal("Justification")),
                        Status = (RequestStatus)reader.GetInt32(reader.GetOrdinal("RequestStatus"))
                    };
                }
                return null;
            }
            catch (SqlException exception)
            {
                throw new DataException($"An error occurred while retrieving the request with ID {id} from the database.", exception);
            }
        }

        // Adds a new request to the database using a stored procedure
        public void Add(Request request)
        {
            try
            {
                using var connection = _databaseConnection.CreateConnection();
                using var command = new SqlCommand(SpAddRequest, connection)
                {
                    CommandType = System.Data.CommandType.StoredProcedure
                };

                command.Parameters.Add("@RequestDate", SqlDbType.Date).Value = request.RequestDate.ToDateTime(new TimeOnly(0, 0));
                command.Parameters.Add("@Justification", SqlDbType.NVarChar, 200).Value = request.Justification;
                command.Parameters.Add("@RequestStatus", SqlDbType.Int).Value = (int)request.Status;

                connection.Open();

                var result = command.ExecuteScalar();

                if (result == null || result == DBNull.Value)
                {
                    throw new KeyNotFoundException("Could not get generated RequestID from stored procedure.");
                }
                request.RequestID = Convert.ToInt32(result);
            }
            catch (SqlException exception)
            {
                throw new DataException("An error occurred while adding a new request to the database.", exception);
            }
        }

        // Updates an existing request in the database using a stored procedure
        public void Update(Request request)
        {
            if (request.RequestID <= 0)
                throw new ArgumentOutOfRangeException(nameof(request.RequestID), "RequestID must be greater than zero.");

            try
            {
                using var connection = _databaseConnection.CreateConnection();
                using var command = new SqlCommand(SpUpdateRequest, connection)
                {
                    CommandType = System.Data.CommandType.StoredProcedure
                };
                command.Parameters.Add("@RequestID", SqlDbType.Int).Value = request.RequestID;
                command.Parameters.Add("@RequestDate", SqlDbType.Date).Value = request.RequestDate.ToDateTime(new TimeOnly(0, 0));
                command.Parameters.Add("@Justification", SqlDbType.NVarChar, 200).Value = request.Justification;
                command.Parameters.Add("@RequestStatus", SqlDbType.Int).Value = (int)request.Status;

                connection.Open();
                var rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected == 0)
                {
                    throw new KeyNotFoundException($"No request found with ID {request.RequestID} to update.");
                }
            }
            catch (SqlException exception)
            {
                throw new DataException($"An error occurred while updating the request with ID {request.RequestID} in the database.", exception);
            }
        }

        // Deletes a request from the database using a stored procedure
        public void Delete(int id)
        {
            if (id <= 0)
                throw new ArgumentOutOfRangeException(nameof(id), "RequestID must be greater than zero.");

            try
            {
                using var connection = _databaseConnection.CreateConnection();
                using var command = new SqlCommand(SpDeleteRequest, connection)
                {
                    CommandType = System.Data.CommandType.StoredProcedure
                };
                command.Parameters.Add("@RequestID", SqlDbType.Int).Value = id;

                connection.Open();
                var rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected == 0)
                {
                    throw new KeyNotFoundException($"No request found with ID {id} to delete.");
                }
            }
            catch (SqlException exception)
            {
                throw new DataException($"An error occurred while deleting the request with ID {id} from the database.", exception);
            }
        }
    }
}
