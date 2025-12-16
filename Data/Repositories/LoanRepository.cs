using System;
using System.Collections.Generic;
using System.Data;
using Application.Interfaces.Repository;
using Application.Models;
using Microsoft.Data.SqlClient;

namespace Data.Repositories
{
    public class LoanRepository : IRepository<Loan>
    {
        // Holds the database connection dependency
        private readonly DatabaseConnection _databaseConnection;

        // Constructor injection of the DatabaseConnection
        public LoanRepository(DatabaseConnection databaseConnection)
        {
            _databaseConnection = databaseConnection;
        }

        // Stored procedure names
        private const string SpGetAllLoans = "uspGetAllLoans";
        private const string SpGetLoanByID = "uspGetLoanByID";
        private const string SpAddLoan = "uspAddLoan";
        private const string SpUpdateLoan = "uspUpdateLoan";
        private const string SpDeleteLoan = "uspDeleteLoan";

        // Retrieves all loans
        public IEnumerable<Loan> GetAll()
        {
            var loans = new List<Loan>();

            try
            {
                using (var connection = _databaseConnection.CreateConnection())
                using (var command = new SqlCommand(SpGetAllLoans, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    connection.Open();

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            loans.Add(MapLoan(reader));
                        }
                    }
                }
                return loans;
            }
            catch (SqlException exception)
            {
                throw new DataException("An error occurred while retrieving loans from the database.", exception);
            }
            catch (Exception exception)
            {
                throw new DataException("Unexpected error while retrieving loans.", exception);
            }
        }

        // Retrieves a single loan by its LoanID
        public Loan? GetByID(int loanID)
        {
            if (loanID <= 0)
                throw new ArgumentOutOfRangeException(nameof(loanID), "LoanID must be greater than zero.");

            try
            {
                using (var connection = _databaseConnection.CreateConnection())
                using (var command = new SqlCommand(SpGetLoanByID, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add("@LoanID", SqlDbType.Int).Value = loanID;

                    connection.Open();

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                            return MapLoan(reader);
                    }
                }

                return null;
            }
            catch (SqlException exception)
            {
                throw new DataException($"An error occurred while retrieving the loan with ID {loanID}.", exception);
            }
            catch (Exception exception)
            {
                throw new DataException("Unexpected error while retrieving loan by ID.", exception);
            }
        }

        // Adds a new loan
        public void Add(Loan loan)
        {
            if (loan is null)
                throw new ArgumentNullException(nameof(loan));

            try
            {
                using (var connection = _databaseConnection.CreateConnection())
                using (var command = new SqlCommand(SpAddLoan, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    AddLoanParameters(command, loan);

                    connection.Open();

                    var result = command.ExecuteScalar();

                    if (result == null || result == DBNull.Value)
                        throw new KeyNotFoundException("Could not get generated LoanID from stored procedure.");

                    loan.LoanID = Convert.ToInt32(result);
                }
            }
            catch (SqlException exception)
            {
                throw new DataException("An error occurred while adding a new loan to the database.", exception);
            }
            catch (Exception exception)
            {
                throw new DataException("Unexpected error while adding loan.", exception);
            }
        }

        // Updates an existing loan
        public void Update(Loan loan)
        {
            if (loan is null)
                throw new ArgumentNullException(nameof(loan));

            if (loan.LoanID <= 0)
                throw new ArgumentOutOfRangeException(nameof(loan.LoanID), "LoanID must be greater than zero.");

            try
            {
                using (var connection = _databaseConnection.CreateConnection())
                using (var command = new SqlCommand(SpUpdateLoan, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.Add("@LoanID", SqlDbType.Int).Value = loan.LoanID;
                    AddLoanParameters(command, loan);

                    connection.Open();

                    var rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected == 0)
                        throw new KeyNotFoundException($"No loan found with ID {loan.LoanID} to update.");
                }
            }
            catch (SqlException exception)
            {
                throw new DataException($"An error occurred while updating the loan with ID {loan.LoanID}.", exception);
            }
            catch (Exception exception)
            {
                throw new DataException("Unexpected error while updating loan.", exception);
            }
        }

        // Deletes a loan
        public void Delete(int loanID)
        {
            if (loanID <= 0)
                throw new ArgumentOutOfRangeException(nameof(loanID), "LoanID must be greater than zero.");

            try
            {
                using (var connection = _databaseConnection.CreateConnection())
                using (var command = new SqlCommand(SpDeleteLoan, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add("@LoanID", SqlDbType.Int).Value = loanID;

                    connection.Open();

                    var rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected == 0)
                        throw new KeyNotFoundException($"No loan found with ID {loanID} to delete.");
                }
            }
            catch (SqlException exception)
            {
                throw new DataException($"An error occurred while deleting the loan with ID {loanID}.", exception);
            }
            catch (Exception exception)
            {
                throw new DataException("Unexpected error while deleting loan.", exception);
            }
        }

        // Maps a SqlDataReader row to a Loan object for GetAll and GetByID methods
        private static Loan MapLoan(SqlDataReader reader)
        {
            var endDateOrdinal = reader.GetOrdinal("EndDate");
            var approverOrdinal = reader.GetOrdinal("ApproverID");
            var startDateOrdinal = reader.GetOrdinal("StartDate");
            var requestOrdinal = reader.GetOrdinal("RequestID");

            return new Loan
            {
                LoanID = reader.GetInt32(reader.GetOrdinal("LoanID")),
                Status = (LoanStatus)reader.GetInt32(reader.GetOrdinal("LoanStatus")),
                StartDate = reader.IsDBNull(startDateOrdinal)
                    ? null
                    : DateOnly.FromDateTime(reader.GetDateTime(startDateOrdinal)),
                EndDate = reader.IsDBNull(endDateOrdinal)
                    ? null
                    : DateOnly.FromDateTime(reader.GetDateTime(endDateOrdinal)),
                RequestID = reader.IsDBNull(requestOrdinal)
                ? null
                : reader.GetInt32(requestOrdinal),
                BorrowerID = reader.GetInt32(reader.GetOrdinal("BorrowerID")),
                ApproverID = reader.IsDBNull(approverOrdinal)
                    ? null
                    : reader.GetInt32(approverOrdinal),
                DeviceID = reader.GetInt32(reader.GetOrdinal("DeviceID"))
            };
        }

        // Adds parameters for a Loan object to a SqlCommand for Add and Update methods
        private static void AddLoanParameters(SqlCommand command, Loan loan)
        {
            command.Parameters.Add("@Status", SqlDbType.Int).Value = (int)loan.Status;
            var requestParam = command.Parameters.Add("@RequestID", SqlDbType.Int);
            requestParam.Value = loan.RequestID.HasValue
                ? loan.RequestID.Value
                : DBNull.Value;

            command.Parameters.Add("@BorrowerID", SqlDbType.Int).Value = loan.BorrowerID;

            var approverParam = command.Parameters.Add("@ApproverID", SqlDbType.Int);
            approverParam.Value = loan.ApproverID.HasValue
                ? loan.ApproverID.Value
                : DBNull.Value;

            command.Parameters.Add("@DeviceID", SqlDbType.Int).Value = loan.DeviceID;

            var startDateParam = command.Parameters.Add("@StartDate", SqlDbType.Date);
            startDateParam.Value = loan.StartDate.HasValue
                ? loan.StartDate.Value.ToDateTime(new TimeOnly(0, 0))
                : DBNull.Value;

            var endDateParam = command.Parameters.Add("@EndDate", SqlDbType.Date);
            endDateParam.Value = loan.EndDate.HasValue
                ? loan.EndDate.Value.ToDateTime(new TimeOnly(0, 0))
                : DBNull.Value;
        }
    }
}
