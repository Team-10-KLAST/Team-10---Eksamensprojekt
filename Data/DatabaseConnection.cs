using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data
{
    public sealed class DatabaseConnection
    {
        private static DatabaseConnection? _instance;

        private readonly string _connectionString;
        private DatabaseConnection(string connectionString)
        {
            _connectionString = connectionString;
        }

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

        public static DatabaseConnection GetInstance()
        {
            if (_instance == null)
            {
                throw new InvalidOperationException("DatabaseConnection is not initialized. Call Instance() first.");
            }
            return _instance;
        }
    }
}
