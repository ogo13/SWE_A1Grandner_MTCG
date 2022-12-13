using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWE_A1Grandner_MTCG
{
    internal class DataHandler
    {
        private readonly string _connectionString;
        private NpgsqlConnection? _connection;

        public DataHandler(string connectionString, NpgsqlConnection connection)
        {
            _connectionString = connectionString;
        }

        public void Connect()
        {
            _connection = new NpgsqlConnection(_connectionString);
            _connection.Open();
        }

        public void Disconnect()
        {
            _connection?.Close();
        }

        public NpgsqlDataReader ExecuteQuery(string sql)
        {
            var command = new NpgsqlCommand(sql, _connection);
            return command.ExecuteReader();
        }
    }
}
