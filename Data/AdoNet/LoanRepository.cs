using System;
using System.Collections.Generic;
using System.Data;
using Application.Interfaces.Repository;
using Application.Models;
using Microsoft.Data.SqlClient;

namespace Data.AdoNet
{
    public class LoanRepository : IRepository<Loan>
    {
        // Holds the database connection dependency
        private readonly DatabaseConnection _databaseConnection;

        // Stores the DatabaseConnection instance passed from DI
        public LoanRepository(DatabaseConnection databaseConnection)
        {
            _databaseConnection = databaseConnection;
        }

        // Stored procedure names
        private const string SpGetAllLoans = "uspGetAllLoans";
        private const string SpGetLoanById = "uspGetLoanById";
        private const string SpAddLoan = "uspAddLoan";
        private const string SpUpdateLoan = "uspUpdateLoan";
        private const string SpDeleteLoan = "uspDeleteLoan";

        // Retrieves all loans from the database using a stored procedure
        public IEnumerable<Loan> GetAll()
        {
            var loans = new List<Loan>();

            try
            {
                using var connection = _databaseConnection.CreateConnection();
                using var command = new SqlCommand(SpGetAllLoans, connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                connection.Open();
                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    // Reads the optional EndDate safely
                    var endDateOrdinal = reader.GetOrdinal("EndDate");
                    DateOnly? endDate = null;
                    if (!reader.IsDBNull(endDateOrdinal))
                    {
                        endDate = DateOnly.FromDateTime(reader.GetDateTime(endDateOrdinal));
                    }

                    loans.Add(new Loan
                    {
                        LoanID = reader.GetInt32(reader.GetOrdinal("LoanID")),
                        LoanStatus = reader.GetString(reader.GetOrdinal("Status")),
                        StartDate = DateOnly.FromDateTime(reader.GetDateTime(reader.GetOrdinal("StartDate"))),
                        EndDate = endDate,
                        RequestID = reader.GetInt32(reader.GetOrdinal("RequestID")),
                        BorrowerID = reader.GetInt32(reader.GetOrdinal("BorrowerID")),
                        ApproverID = reader.GetInt32(reader.GetOrdinal("ApproverID")),
                        DeviceID = reader.GetInt32(reader.GetOrdinal("DeviceID"))
                    });
                }

                return loans;
            }
            catch (SqlException exception)
            {
                throw new DataException("An error occurred while retrieving loans from the database.", exception);
            }
        }

        // Retrieves a single loan by its LoanID using a stored procedure
        public Loan? GetById(int loanID)
        {
            if (loanID <= 0)
                throw new ArgumentOutOfRangeException(nameof(loanID), "LoanID must be greater than zero.");

            try
            {
                using var connection = _databaseConnection.CreateConnection();
                using var command = new SqlCommand(SpGetLoanById, connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                // Sends the LoanID to the stored procedure
                command.Parameters.Add("@LoanID", SqlDbType.Int).Value = loanID;

                connection.Open();
                using var reader = command.ExecuteReader();

                if (reader.Read())
                {
                    // Reads the optional EndDate safely
                    var endDateOrdinal = reader.GetOrdinal("EndDate");
                    DateOnly? endDate = null;
                    if (!reader.IsDBNull(endDateOrdinal))
                    {
                        endDate = DateOnly.FromDateTime(reader.GetDateTime(endDateOrdinal));
                    }

                    // Builds and returns a Loan object from the current record
                    return new Loan
                    {
                        LoanID = reader.GetInt32(reader.GetOrdinal("LoanID")),
                        LoanStatus = reader.GetString(reader.GetOrdinal("Status")),
                        StartDate = DateOnly.FromDateTime(reader.GetDateTime(reader.GetOrdinal("StartDate"))),
                        EndDate = endDate,
                        RequestID = reader.GetInt32(reader.GetOrdinal("RequestID")),
                        BorrowerID = reader.GetInt32(reader.GetOrdinal("BorrowerID")),
                        ApproverID = reader.GetInt32(reader.GetOrdinal("ApproverID")),
                        DeviceID = reader.GetInt32(reader.GetOrdinal("DeviceID"))
                    };
                }

                // Returns null when no Loan with the given LoanID is found
                return null;
            }
            catch (SqlException exception)
            {
                throw new DataException($"An error occurred while retrieving the loan with ID {loanID} from the database.", exception);
            }
        }

        // Adds a new loan to the database using a stored procedure
        public void Add(Loan loan)
        {
            if (loan is null)
                throw new ArgumentNullException(nameof(loan));

            try
            {
                using var connection = _databaseConnection.CreateConnection();
                using var command = new SqlCommand(SpAddLoan, connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                command.Parameters.Add("@Status", SqlDbType.NVarChar, 50).Value = loan.LoanStatus;
                command.Parameters.Add("@RequestID", SqlDbType.Int).Value = loan.RequestID;
                command.Parameters.Add("@BorrowerID", SqlDbType.Int).Value = loan.BorrowerID;
                command.Parameters.Add("@ApproverID", SqlDbType.Int).Value = loan.ApproverID;
                command.Parameters.Add("@DeviceID", SqlDbType.Int).Value = loan.DeviceID;
                command.Parameters.Add("@StartDate", SqlDbType.Date).Value =
                    loan.StartDate.ToDateTime(new TimeOnly(0, 0));

                // Sends EndDate as a Date value or DBNull when not set
                var endDateParam = command.Parameters.Add("@EndDate", SqlDbType.Date);
                if (loan.EndDate.HasValue)
                {
                    endDateParam.Value = loan.EndDate.Value.ToDateTime(new TimeOnly(0, 0));
                }
                else
                {
                    endDateParam.Value = DBNull.Value;
                }

                connection.Open();

                var result = command.ExecuteScalar();

                if (result == null || result == DBNull.Value)
                    throw new KeyNotFoundException("Could not get generated LoanID from stored procedure.");

                loan.LoanID = Convert.ToInt32(result);
            }
            catch (SqlException exception)
            {
                throw new DataException("An error occurred while adding a new loan to the database.", exception);
            }
        }

        // Updates an existing loan in the database using a stored procedure
        public void Update(Loan loan)
        {
            if (loan is null)
                throw new ArgumentNullException(nameof(loan));

            if (loan.LoanID <= 0)
                throw new ArgumentOutOfRangeException(nameof(loan.LoanID), "LoanID must be greater than zero.");

            try
            {
                using var connection = _databaseConnection.CreateConnection();
                using var command = new SqlCommand(SpUpdateLoan, connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                command.Parameters.Add("@LoanID", SqlDbType.Int).Value = loan.LoanID;
                command.Parameters.Add("@Status", SqlDbType.NVarChar, 50).Value = loan.LoanStatus;
                command.Parameters.Add("@RequestID", SqlDbType.Int).Value = loan.RequestID;
                command.Parameters.Add("@BorrowerID", SqlDbType.Int).Value = loan.BorrowerID;
                command.Parameters.Add("@ApproverID", SqlDbType.Int).Value = loan.ApproverID;
                command.Parameters.Add("@DeviceID", SqlDbType.Int).Value = loan.DeviceID;
                command.Parameters.Add("@StartDate", SqlDbType.Date).Value =
                    loan.StartDate.ToDateTime(new TimeOnly(0, 0));

                // Sends EndDate as a Date value or DBNull when not set
                var endDateParam = command.Parameters.Add("@EndDate", SqlDbType.Date);
                if (loan.EndDate.HasValue)
                {
                    endDateParam.Value = loan.EndDate.Value.ToDateTime(new TimeOnly(0, 0));
                }
                else
                {
                    endDateParam.Value = DBNull.Value;
                }

                connection.Open();
                var rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected == 0)
                    throw new KeyNotFoundException($"No loan found with ID {loan.LoanID} to update.");
            }
            catch (SqlException exception)
            {
                throw new DataException($"An error occurred while updating the loan with ID {loan.LoanID} in the database.", exception);
            }
        }

        // Deletes a loan from the database using a stored procedure
        public void Delete(int loanID)
        {
            if (loanID <= 0)
                throw new ArgumentOutOfRangeException(nameof(loanID), "LoanID must be greater than zero.");

            try
            {
                using var connection = _databaseConnection.CreateConnection();
                using var command = new SqlCommand(SpDeleteLoan, connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                command.Parameters.Add("@LoanID", SqlDbType.Int).Value = loanID;

                connection.Open();
                var rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected == 0)
                    throw new KeyNotFoundException($"No loan found with ID {loanID} to delete.");
            }
            catch (SqlException exception)
            {
                throw new DataException($"An error occurred while deleting the loan with ID {loanID} from the database.", exception);
            }
        }
    }
}
