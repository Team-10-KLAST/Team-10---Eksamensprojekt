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

        // Retrieves all requests
        public IEnumerable<Request> GetAll()
        {
            var requests = new List<Request>();

            try
            {
                using (var connection = _databaseConnection.CreateConnection())
                using (var command = new SqlCommand(SpGetAllRequests, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    connection.Open();

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            requests.Add(MapRequest(reader));
                        }
                    }
                }
                return requests;
            }
            catch (SqlException exception)
            {
                throw new DataException("An error occurred while retrieving requests from the database.", exception);
            }
            catch (Exception exception)
            {
                throw new DataException("Unexpected error while retrieving requests.", exception);
            }
        }

        // Retrieves a request by its ID
        public Request? GetByID(int id)
        {
            if (id <= 0)
                throw new ArgumentOutOfRangeException(nameof(id), "RequestID must be greater than zero.");

            try
            {
                using (var connection = _databaseConnection.CreateConnection())
                using (var command = new SqlCommand(SpGetRequestByID, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add("@RequestID", SqlDbType.Int).Value = id;

                    connection.Open();

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                            return MapRequest(reader);
                    }
                }
                return null;
            }
            catch (SqlException exception)
            {
                throw new DataException($"An error occurred while retrieving the request with ID {id}.", exception);
            }
            catch (Exception exception)
            {
                throw new DataException("Unexpected error while retrieving request by ID.", exception);
            }
        }

        // Adds a new request
        public void Add(Request request)
        {
            if (request is null)
                throw new ArgumentNullException(nameof(request));

            try
            {
                using (var connection = _databaseConnection.CreateConnection())
                using (var command = new SqlCommand(SpAddRequest, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    AddRequestParameters(command, request);

                    connection.Open();

                    var result = command.ExecuteScalar();

                    if (result == null || result == DBNull.Value)
                        throw new KeyNotFoundException("Could not get generated RequestID from stored procedure.");

                    request.RequestID = Convert.ToInt32(result);
                }
            }
            catch (SqlException exception)
            {
                throw new DataException("An error occurred while adding a new request to the database.", exception);
            }
            catch (Exception exception)
            {
                throw new DataException("Unexpected error while adding request.", exception);
            }
        }

        // Updates an existing request
        public void Update(Request request)
        {
            if (request is null)
                throw new ArgumentNullException(nameof(request));

            if (request.RequestID <= 0)
                throw new ArgumentOutOfRangeException(nameof(request.RequestID), "RequestID must be greater than zero.");

            try
            {
                using (var connection = _databaseConnection.CreateConnection())
                using (var command = new SqlCommand(SpUpdateRequest, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.Add("@RequestID", SqlDbType.Int).Value = request.RequestID;
                    AddRequestParameters(command, request);

                    connection.Open();

                    var rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected == 0)
                        throw new KeyNotFoundException($"No request found with ID {request.RequestID} to update.");
                }
            }
            catch (SqlException exception)
            {
                throw new DataException($"An error occurred while updating the request with ID {request.RequestID}.", exception);
            }
            catch (Exception exception)
            {
                throw new DataException("Unexpected error while updating request.", exception);
            }
        }

        // Deletes a request
        public void Delete(int id)
        {
            if (id <= 0)
                throw new ArgumentOutOfRangeException(nameof(id), "RequestID must be greater than zero.");

            try
            {
                using (var connection = _databaseConnection.CreateConnection())
                using (var command = new SqlCommand(SpDeleteRequest, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add("@RequestID", SqlDbType.Int).Value = id;

                    connection.Open();

                    var rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected == 0)
                        throw new KeyNotFoundException($"No request found with ID {id} to delete.");
                }
            }
            catch (SqlException exception)
            {
                throw new DataException($"An error occurred while deleting the request with ID {id}.", exception);
            }
            catch (Exception exception)
            {
                throw new DataException("Unexpected error while deleting request.", exception);
            }
        }

        // Maps a SqlDataReader row to a Request object for GetAll and GetByID methods
        private static Request MapRequest(SqlDataReader reader)
        {
            return new Request
            {
                RequestID = reader.GetInt32(reader.GetOrdinal("RequestID")),
                RequestDate = DateOnly.FromDateTime(reader.GetDateTime(reader.GetOrdinal("RequestDate"))),
                NeededByDate = DateOnly.FromDateTime(reader.GetDateTime(reader.GetOrdinal("NeededByDate"))),
                Justification = reader.GetString(reader.GetOrdinal("Justification")),
                Status = (RequestStatus)reader.GetInt32(reader.GetOrdinal("RequestStatus"))
            };
        }

        // Adds parameters for Request to a SqlCommand for Add and Update methods
        private static void AddRequestParameters(SqlCommand command, Request request)
        {
            command.Parameters.Add("@RequestDate", SqlDbType.Date).Value =
                request.RequestDate.ToDateTime(TimeOnly.MinValue);

            command.Parameters.Add("@NeededByDate", SqlDbType.Date).Value =
                request.NeededByDate.ToDateTime(TimeOnly.MinValue);

            command.Parameters.Add("@Justification", SqlDbType.NVarChar, 200).Value =
                request.Justification;

            command.Parameters.Add("@RequestStatus", SqlDbType.Int).Value =
                (int)request.Status;
        }
    }
}
