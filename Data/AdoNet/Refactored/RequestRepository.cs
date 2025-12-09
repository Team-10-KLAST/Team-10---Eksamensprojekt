using System;
using System.Collections.Generic;
using System.Data;
using Application.Interfaces; // Nødvendig for IDbAccess
using Application.Interfaces.Repository;
using Application.Models;
using Microsoft.Data.SqlClient;

namespace Data.AdoNet.Refactored
{
    // Bemærk: Denne RequestRepository afhænger nu af IDbAccess i stedet for DatabaseConnection,
    // hvilket gør den testbar med Moq.
    public class RequestRepository : IRepository<Request>
    {
        private readonly IDbAccess _dbAccess;

        // Constructor injection af IDbAccess
        public RequestRepository(IDbAccess dbAccess)
        {
            _dbAccess = dbAccess;
        }

        // Stored procedure names
        private const string SpGetAllRequests = "uspGetAllRequests";
        private const string SpGetRequestByID = "uspGetRequestByID";
        private const string SpAddRequest = "uspAddRequest";
        private const string SpUpdateRequest = "uspUpdateRequest";
        private const string SpDeleteRequest = "uspDeleteRequest";

        // Hjælpefunktion til at mappe Request-objekt til Dictionary for IDbAccess
        private Dictionary<string, object> MapToParameters(Request request)
        {
            return new Dictionary<string, object>
            {
                { "@RequestID", request.RequestID },
                { "@RequestDate", request.RequestDate.ToDateTime(new TimeOnly(0, 0)) },
                { "@Justification", request.Justification },
                { "@RequestStatus", (int)request.Status }
            };
        }

        public IEnumerable<Request> GetAll()
        {
            var requests = new List<Request>();
            try
            {
                // Bruger IDbAccess.ExecuteReader
                using var reader = _dbAccess.ExecuteReader(SpGetAllRequests);
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

        public Request? GetByID(int id)
        {
            if (id <= 0)
                throw new ArgumentOutOfRangeException(nameof(id), "RequestID must be greater than zero.");

            try
            {
                var parameters = new Dictionary<string, object> { { "@RequestID", id } };

                // Bruger IDbAccess.ExecuteReader
                using var reader = _dbAccess.ExecuteReader(SpGetRequestByID, parameters);

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

        public void Add(Request request)
        {
            try
            {
                var parameters = new Dictionary<string, object>
                {
                    { "@RequestDate", request.RequestDate.ToDateTime(new TimeOnly(0, 0)) },
                    { "@Justification", request.Justification },
                    { "@RequestStatus", (int)request.Status }
                };

                // Bruger IDbAccess.ExecuteScalar
                var result = _dbAccess.ExecuteScalar(SpAddRequest, parameters);

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

        public void Update(Request request)
        {
            if (request.RequestID <= 0)
                throw new ArgumentOutOfRangeException(nameof(request.RequestID), "RequestID must be greater than zero.");

            try
            {
                var parameters = MapToParameters(request); // Bruger RequestID

                // Bruger IDbAccess.ExecuteNonQuery
                var rowsAffected = _dbAccess.ExecuteNonQuery(SpUpdateRequest, parameters);

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

        public void Delete(int id)
        {
            if (id <= 0)
                throw new ArgumentOutOfRangeException(nameof(id), "RequestID must be greater than zero.");

            try
            {
                var parameters = new Dictionary<string, object> { { "@RequestID", id } };

                // Bruger IDbAccess.ExecuteNonQuery
                var rowsAffected = _dbAccess.ExecuteNonQuery(SpDeleteRequest, parameters);

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