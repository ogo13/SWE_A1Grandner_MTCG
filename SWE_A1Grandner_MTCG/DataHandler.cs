using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWE_A1Grandner_MTCG
{
    public class DataHandler
    {
        // Replace with your own connection string

        public static string ConnectionString { get; } =
            "Host=localhost;Username=postgres;Password=postgres;Database=MTCG";

        private readonly Dictionary<string, string> _sqlStatementDictionary = new Dictionary<string, string>()
        {
            {"cardInsert", "INSERT INTO card VALUES ($1) ($2) ($3);"},
            {"userInsert", "INSERT INTO user VALUES ($1) ($2) ($3);"},
            {"userSelect", "SELECT * FROM user WHERE token == ($1);"}
        };

        public async Task InsertData(string tableName, string[] parameters)
        {
            try
            {

                await using var connection = new NpgsqlConnection(ConnectionString);
                await connection.OpenAsync();

                await using var command = new NpgsqlCommand();
                command.Connection = connection;
                command.CommandText = _sqlStatementDictionary[tableName + "Insert"];

                command.Parameters.Add(parameters);
                await command.PrepareAsync();

                await command.ExecuteNonQueryAsync();

            }
            catch (NpgsqlException e)
            {
                // Handle exceptions
            }
        }
        public async Task<DataTable> GetData(string tableName, string[] parameters)
        {

            try
            {
                await using var connection = new NpgsqlConnection(ConnectionString);
                await connection.OpenAsync();

                await using var command = new NpgsqlCommand(_sqlStatementDictionary[tableName + "Select"], connection);

                command.Parameters.AddRange(parameters);
                await command.PrepareAsync();

                var dataTable = new DataTable();
                dataTable.Load(command.ExecuteReader());

                return dataTable;
            }
            catch (NpgsqlException ex)
            {
                // Handle exceptions

                throw;
            }
        }
    }
}



