using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace Data
{
    public sealed class DatabaseConnection
    {
        // Singleton instance
        private static DatabaseConnection? _instance;

        private readonly string _connectionString;

        // Private constructor prevents external instantiation
        private DatabaseConnection(string connectionString)
        {
            _connectionString = connectionString;
        }

        // Initializes the singleton instance once
        public static void Initialize(string connectionString)
        {
            if (_instance == null)
            {
                _instance = new DatabaseConnection(connectionString);
            }
            else
            {
                throw new InvalidOperationException("DatabaseConnection is already initialized.");
            }
        }

        // Returns the single instance of DatabaseConnection
        public static DatabaseConnection GetInstance()
        {
            if (_instance == null)
            {
                throw new InvalidOperationException("DatabaseConnection is not initialized. Call Instance() first.");
            }
            return _instance;
        }

        // Creates and returns a new SqlConnection
        public SqlConnection CreateConnection()
        {
            return new SqlConnection(_connectionString);
        }
    }
}
