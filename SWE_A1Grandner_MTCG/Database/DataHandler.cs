using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NpgsqlTypes;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace SWE_A1Grandner_MTCG.Databank
{
    public class DataHandler
    {
        // Replace with your own connection string
        public NpgsqlConnection Connection { get; }
        public static string ConnectionString => "Host=localhost;Username=postgres;Password=postgres;Database=MTCG";

        private static readonly Dictionary<string, string> SelectSqlStatementDictionary = new Dictionary<string, string>()
        {
            {"token", "SELECT * FROM public.user WHERE token = (@token);"},
            {"username", "SELECT * FROM public.user WHERE username = (@username);"}
        };

        public DataHandler()
        {
            Connection = new NpgsqlConnection(ConnectionString);
        }

        public async Task<int> InsertUser(UserData userData)
        {
            try
            {

                await using var connection = new NpgsqlConnection(ConnectionString);
                connection.Open();

                await using var command = new NpgsqlCommand();
                command.Connection = connection;
                command.CommandText =
                    "INSERT INTO public.user(username, password, token) VALUES (@username, @password, @token);";

                command.Parameters.AddWithValue("username", NpgsqlDbType.Varchar, userData.Username);
                command.Parameters.AddWithValue("password", NpgsqlDbType.Varchar, userData.Password);
                command.Parameters.AddWithValue("token", NpgsqlDbType.Varchar, userData.Username + "-mtcgToken");

                await command.PrepareAsync();

                await command.ExecuteNonQueryAsync();

                return 0;
            }
            catch (NpgsqlException e)
            {
                // Handle exceptions
                throw;
            }
            finally
            {

            }
        }

        public async Task InsertCard(Dictionary<string, string> parameters)
        {
            try
            {

                await using var connection = new NpgsqlConnection(ConnectionString);
                await connection.OpenAsync();

                await using var command = new NpgsqlCommand();
                command.Connection = connection;
                command.CommandText = "INSERT INTO public.card(id, name, damage) VALUES (@id, @name, @damage);";

                command.Parameters.AddWithValue("id", NpgsqlDbType.Varchar, parameters["id"]);
                command.Parameters.AddWithValue("name", NpgsqlDbType.Varchar, parameters["name"]);
                command.Parameters.AddWithValue("damage", NpgsqlDbType.Varchar, parameters["damage"]);

                await command.PrepareAsync();

                await command.ExecuteNonQueryAsync();

            }
            catch (NpgsqlException e)
            {
                // Handle exceptions
                throw;
            }
        }

        public async Task<int> InsertData(string tableName, string[] parameters)
        {
            try
            {
                
                var open = Connection.OpenAsync();

                var command = new NpgsqlCommand();
                command.Connection = Connection;
                command.CommandText = SelectSqlStatementDictionary[tableName + "Insert"];

                command.Parameters.Add(parameters);
                await open;
                await command.PrepareAsync();
                await command.ExecuteNonQueryAsync();
                return 1;

            }
            catch (NpgsqlException e)
            {
                // Handle exceptions
                return 0;
            }
            finally
            {
                await Connection.CloseAsync();
            }
        }
        public async Task<DataTable> GetUserBy(string method, string parameter)
        {

            try
            {
                await using var connection = new NpgsqlConnection(ConnectionString);
                await connection.OpenAsync();

                await using var command = new NpgsqlCommand(SelectSqlStatementDictionary[method], connection);

                command.Parameters.AddWithValue(method, NpgsqlDbType.Varchar, parameter);
                await command.PrepareAsync();

                var dataTable = new DataTable();
                dataTable.Load(command.ExecuteReader());

                return dataTable;
            }
            catch (NpgsqlException e)
            {
                // Handle exceptions
                Console.WriteLine(e.Message);
                throw;
            }
        }

    }
}



