using Application.Interfaces.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Models;
using Microsoft.Data.SqlClient;
using System.Data;


namespace Data.Repositories
{
    public class DecisionRepository : IRepository<Decision>
    {
        // Holds the database connection dependency
        private readonly DatabaseConnection _databaseConnection;

        // Constructor injection of the DatabaseConnection
        public DecisionRepository(DatabaseConnection databaseConnection)
        {
            _databaseConnection = databaseConnection;
        }

        // Stored procedure names
        private const string SpGetAllDecisions = "uspGetAllDecisions";
        private const string SpGetDecisionByID = "uspGetDecisionByID";
        private const string SpAddDecision = "uspAddDecision";
        private const string SpUpdateDecision = "uspUpdateDecision";
        private const string SpDeleteDecision = "uspDeleteDecision";

        // Retrieves all decisions
        public IEnumerable<Decision> GetAll()
        {
            var decisions = new List<Decision>();

            try
            {
                using (var connection = _databaseConnection.CreateConnection())
                using (var command = new SqlCommand(SpGetAllDecisions, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    connection.Open();

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            decisions.Add(MapDecision(reader));
                        }
                    }
                }
                return decisions;
            }
            catch (SqlException exception)
            {
                throw new DataException("Database error while reading all Decisions.", exception);
            }
            catch (Exception exception)
            {
                throw new DataException("Unexpected error while reading all Decisions.", exception);
            }
        }

        // Retrieves a single decision by ID
        public Decision? GetByID(int decisionID)
        {
            if (decisionID <= 0)
                throw new ArgumentOutOfRangeException(nameof(decisionID), "DecisionID must be greater than zero.");

            try
            {
                using (var connection = _databaseConnection.CreateConnection())
                using (var command = new SqlCommand(SpGetDecisionByID, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add("@DecisionID", SqlDbType.Int).Value = decisionID;
                    connection.Open();

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapDecision(reader);
                        }
                    }
                }
                return null;
            }
            catch (SqlException exception)
            {
                throw new DataException($"An error occurred while retrieving the decision with ID {decisionID}.", exception);
            }
            catch (Exception exception)
            {
                throw new DataException("Unexpected error while retrieving decision by ID.", exception);
            }
        }

        // Adds a new decision
        public void Add(Decision decision)
        {
            if (decision is null)
                throw new ArgumentNullException(nameof(decision));

            try
            {
                using (var connection = _databaseConnection.CreateConnection())
                using (var command = new SqlCommand(SpAddDecision, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    AddDecisionParameters(command, decision);

                    connection.Open();

                    var result = command.ExecuteScalar();

                    if (result == null || result == DBNull.Value)
                        throw new KeyNotFoundException("Could not get generated DecisionID from stored procedure.");

                    decision.DecisionID = Convert.ToInt32(result);
                }
            }
            catch (SqlException exception)
            {
                throw new DataException("An error occurred while adding a new decision to the database.", exception);
            }
            catch (Exception exception)
            {
                throw new DataException("Unexpected error while adding decision.", exception);
            }
        }

        // Updates an existing decision
        public void Update(Decision decision)
        {
            if (decision is null)
                throw new ArgumentNullException(nameof(decision));

            if (decision.DecisionID <= 0)
                throw new ArgumentOutOfRangeException(nameof(decision.DecisionID), "DecisionID must be greater than zero.");

            try
            {
                using (var connection = _databaseConnection.CreateConnection())
                using (var command = new SqlCommand(SpUpdateDecision, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.Add("@DecisionID", SqlDbType.Int).Value = decision.DecisionID;
                    AddDecisionParameters(command, decision);

                    connection.Open();

                    var rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected == 0)
                        throw new KeyNotFoundException($"No decision found with ID {decision.DecisionID} to update.");
                }
            }
            catch (SqlException exception)
            {
                throw new DataException($"An error occurred while updating the decision with ID {decision.DecisionID}.", exception);
            }
            catch (Exception exception)
            {
                throw new DataException("Unexpected error while updating decision.", exception);
            }
        }

        // Deletes a decision
        public void Delete(int decisionID)
        {
            if (decisionID <= 0)
                throw new ArgumentOutOfRangeException(nameof(decisionID), "DecisionID must be greater than zero.");

            try
            {
                using (var connection = _databaseConnection.CreateConnection())
                using (var command = new SqlCommand(SpDeleteDecision, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add("@DecisionID", SqlDbType.Int).Value = decisionID;

                    connection.Open();

                    var rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected == 0)
                        throw new KeyNotFoundException($"No decision found with ID {decisionID} to delete.");
                }
            }
            catch (SqlException exception)
            {
                throw new DataException($"An error occurred while deleting the decision with ID {decisionID}.", exception);
            }
            catch (Exception exception)
            {
                throw new DataException("Unexpected error while deleting decision.", exception);
            }
        }

        // Maps a SqlDataReader row to a Decision object for GetAll and GetByID methods
        private static Decision MapDecision(SqlDataReader reader)
        {
            var decisionDateTime = reader.GetDateTime(reader.GetOrdinal("DecisionDate"));
            var decisionDate = DateOnly.FromDateTime(decisionDateTime);

            return new Decision
            {
                DecisionID = reader.GetInt32(reader.GetOrdinal("DecisionID")),
                Status = (DecisionStatus)reader.GetInt32(reader.GetOrdinal("DecisionStatus")),
                DecisionDate = decisionDate,
                Comment = reader.GetString(reader.GetOrdinal("Comment")),
                LoanID = reader.GetInt32(reader.GetOrdinal("LoanID"))
            };
        }

        // Adds parameters for Decision to a SqlCommand for Add and Update methods
        private static void AddDecisionParameters(SqlCommand command, Decision decision)
        {
            command.Parameters.Add("@DecisionStatus", SqlDbType.Int).Value = (int)decision.Status;

            command.Parameters.Add("@DecisionDate", SqlDbType.Date).Value =
                decision.DecisionDate.ToDateTime(TimeOnly.MinValue);

            command.Parameters.Add("@Comment", SqlDbType.NVarChar, 2000).Value =
                decision.Comment ?? string.Empty;

            command.Parameters.Add("@LoanID", SqlDbType.Int).Value = decision.LoanID;
        }
    }
}
