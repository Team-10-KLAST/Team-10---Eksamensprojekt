using Application.Interfaces.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Models;
using Microsoft.Data.SqlClient;
using System.Data;


namespace Data.AdoNet
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

        public IEnumerable<Decision> GetAll()
        {
            var decisions = new List<Decision>();

            using var connection = _databaseConnection.CreateConnection();
            using var command = new SqlCommand("uspGetAllDecisions", connection)
            {
                CommandType = System.Data.CommandType.StoredProcedure
            };

            connection.Open();
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                DateTime dt = reader.GetDateTime(reader.GetOrdinal("DecisionDate"));
                DateOnly decisionDate = DateOnly.FromDateTime(dt);

                decisions.Add(new Decision
                {
                    DecisionID = reader.GetInt32(reader.GetOrdinal("DecisionID")),
                    Status = (DecisionStatus)reader.GetInt32(reader.GetOrdinal("DecisionStatus")),
                    DecisionDate = decisionDate,
                    Comment = reader.GetString(reader.GetOrdinal("Comment")),
                    LoanID = reader.GetInt32(reader.GetOrdinal("LoanID"))
                });
            }
            return decisions;
        }

        public void Add(Decision entity)
        {
            using var connection = _databaseConnection.CreateConnection();
            using var command = new SqlCommand("uspAddDecision", connection)
            {
                CommandType = System.Data.CommandType.StoredProcedure
            };

            command.Parameters.Add("@DecisionStatus", SqlDbType.Int).Value = (int)entity.Status;
            command.Parameters.Add("@DecisionDate", SqlDbType.Date).Value = entity.DecisionDate.ToDateTime(TimeOnly.MinValue);
            command.Parameters.Add("@Comment", SqlDbType.NVarChar, 2000).Value = entity.Comment ?? string.Empty;
            command.Parameters.Add("@LoanID", SqlDbType.Int).Value = entity.LoanID;

            connection.Open();
            command.ExecuteNonQuery();
        }

        public void Delete(int id)
        {
            using var connection = _databaseConnection.CreateConnection();
            using var command = new SqlCommand("uspDeleteDecision", connection)
            {
                CommandType = System.Data.CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@DecisionID", id);

            connection.Open();
            command.ExecuteNonQuery();
        }

        public Decision? GetByID(int id)
        {
            using var connection = _databaseConnection.CreateConnection();
            using var command = new SqlCommand("uspGetDecisionById", connection)
            {
                CommandType = System.Data.CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@DecisionID", id);

            connection.Open();
            using var reader = command.ExecuteReader();
            
            if (reader.Read())
            {
                DateTime dt = reader.GetDateTime(reader.GetOrdinal("DecisionDate"));
                DateOnly decisionDate = DateOnly.FromDateTime(dt);

                return new Decision
                {
                    DecisionID = reader.GetInt32(reader.GetOrdinal("DecisionID")),
                    Status = (DecisionStatus)reader.GetInt32(reader.GetOrdinal("DecisionStatus")),
                    DecisionDate = decisionDate,
                    Comment = reader.GetString(reader.GetOrdinal("Comment")),
                    LoanID = reader.GetInt32(reader.GetOrdinal("LoanID"))
                };
            }
            return null;
        }

        public void Update(Decision entity)
        {
            using var connection = _databaseConnection.CreateConnection();
            using var command = new SqlCommand("uspUpdateDecision", connection)
            {
                CommandType = System.Data.CommandType.StoredProcedure
            };
            command.Parameters.Add("@DecisionID", SqlDbType.Int).Value = entity.DecisionID;
            command.Parameters.Add("@DecisionStatus", SqlDbType.Int).Value = (int)entity.Status;

            command.Parameters.Add("@DecisionDate", SqlDbType.Date).Value = entity.DecisionDate.ToDateTime(TimeOnly.MinValue);

            command.Parameters.Add("@Comment", SqlDbType.NVarChar,2000).Value = entity.Comment ?? string.Empty;

            command.Parameters.Add("@LoanID", SqlDbType.Int).Value = entity.LoanID;

            connection.Open();
            command.ExecuteNonQuery();
        }
    }
}
