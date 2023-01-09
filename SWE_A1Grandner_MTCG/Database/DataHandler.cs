using System.Data;
using System.Data.Common;
using System.Drawing;
using System.Reflection.Metadata;
using Npgsql;
using NpgsqlTypes;
using SWE_A1Grandner_MTCG.BattleLogic;
using SWE_A1Grandner_MTCG.Exceptions;

namespace SWE_A1Grandner_MTCG.Database
{
    public class DataHandler
    {
        // Replace with your own connection string
        public static string ConnectionString => "Host=localhost;Username=postgres;Password=postgres;Database=MTCG";

        private static readonly Dictionary<string, string> QueryDictionary = new Dictionary<string, string>()
        {
            {"token", "SELECT * FROM public.user WHERE token = (@token);"},
            {"username", "SELECT * FROM public.user WHERE username = (@username);"},
            {"cardId", "SELECT * FROM public.card WHERE id = (@id);"},
            {"cardOwner", "SELECT * FROM public.card WHERE owner = (@username);"},
            {"deck", "SELECT * FROM public.card WHERE owner = (@username) AND deck = true;"},
            {"package", "SELECT * FROM public.package WHERE id = (SELECT MIN(id) FROM public.package);"}
        };

        public bool InsertUser(UserData userData)
        {
            using var connection = new NpgsqlConnection(ConnectionString);
            try
            {
                connection.Open();

                using var command = new NpgsqlCommand();
                command.Connection = connection;
                command.CommandText =
                    "INSERT INTO public.user(username, password, token, coins) VALUES (@username, @password, @token, @coins);";

                command.Parameters.AddWithValue("username", NpgsqlDbType.Varchar, userData.Username);
                command.Parameters.AddWithValue("password", NpgsqlDbType.Varchar, userData.Password);
                command.Parameters.AddWithValue("token", NpgsqlDbType.Varchar, userData.Username + "-mtcgToken");
                command.Parameters.AddWithValue("coins", NpgsqlDbType.Integer, 20);

                if (command.ExecuteNonQuery() == 0)
                {
                    return false;
                }
            }
            catch (NpgsqlException e)
            {
                throw new DuplicateNameException("User already exists");
            }
            finally { connection.Close(); }

            return true;
        }

        public bool InsertCard(CardData cardData)
        {
            using var connection = new NpgsqlConnection(ConnectionString);
            try
            {
                connection.Open();

                using var command = new NpgsqlCommand();
                command.Connection = connection;
                command.CommandText = "INSERT INTO public.card(id, name, damage) VALUES (@id, @name, @damage);";

                command.Parameters.AddWithValue("id", NpgsqlDbType.Uuid, cardData.Id);
                command.Parameters.AddWithValue("name", NpgsqlDbType.Varchar, cardData.Name);
                command.Parameters.AddWithValue("damage", NpgsqlDbType.Double, cardData.Damage);

                command.ExecuteNonQuery();

            }
            catch (NpgsqlException e)
            {
                // Handle exceptions
                throw new DuplicateNameException("Card already exists");
            }
            finally { connection.Close(); }

            return true;
        }

        public bool InsertPackage(List<Guid> uuids)
        {
            using var connection = new NpgsqlConnection(ConnectionString);
            try
            {
                connection.Open();

                using var command = new NpgsqlCommand();
                command.Connection = connection;
                command.CommandText = "INSERT INTO public.package(card1, card2, card3, card4, card5) VALUES (@card1, @card2, @card3, @card4, @card5);";

                command.Parameters.AddWithValue("card1", NpgsqlDbType.Uuid, uuids[0]);
                command.Parameters.AddWithValue("card2", NpgsqlDbType.Uuid, uuids[1]);
                command.Parameters.AddWithValue("card3", NpgsqlDbType.Uuid, uuids[2]);
                command.Parameters.AddWithValue("card4", NpgsqlDbType.Uuid, uuids[3]);
                command.Parameters.AddWithValue("card5", NpgsqlDbType.Uuid, uuids[4]);

                command.ExecuteNonQuery();

            }
            catch (NpgsqlException e)
            {
                // Handle exceptions
                throw;
            }
            finally { connection.Close(); }

            return true;
        }

        public bool UpdateOwnerInCards(List<Guid> uuids, string user)
        {
            using var connection = new NpgsqlConnection(ConnectionString);
            try
            {
                connection.Open();

                using var command = new NpgsqlCommand();
                command.Connection = connection;
                command.CommandText =
                    "UPDATE public.card SET owner = (@user) WHERE id = (@card1) OR id = (@card2) OR id = (@card3) OR id = (@card4) OR id = (@card5);";

                command.Parameters.AddWithValue("user", NpgsqlDbType.Varchar, user);
                command.Parameters.AddWithValue("card1", NpgsqlDbType.Uuid, uuids[0]);
                command.Parameters.AddWithValue("card2", NpgsqlDbType.Uuid, uuids[1]);
                command.Parameters.AddWithValue("card3", NpgsqlDbType.Uuid, uuids[2]);
                command.Parameters.AddWithValue("card4", NpgsqlDbType.Uuid, uuids[3]);
                command.Parameters.AddWithValue("card5", NpgsqlDbType.Uuid, uuids[4]);

                if (command.ExecuteNonQuery() != 5)
                {
                    throw new NpgsqlException();
                }

            }
            catch (NpgsqlException e)
            {
                // Handle exceptions
                throw;
            }
            finally { connection.Close(); }

            return true;
        }

        public bool UpdateUser(UserData user)
        {
            using var connection = new NpgsqlConnection(ConnectionString);
            try
            {
                connection.Open();

                using var command = new NpgsqlCommand();
                command.Connection = connection;
                command.CommandText =
                    "UPDATE public.user SET name = (@name), bio = (@bio), image = (@image), coins = (@coins) WHERE username = (@username);";

                
                command.Parameters.AddWithValue("name", NpgsqlDbType.Varchar, user.Name);
                command.Parameters.AddWithValue("bio", NpgsqlDbType.Varchar, user.Bio);
                command.Parameters.AddWithValue("image", NpgsqlDbType.Varchar, user.Image);
                command.Parameters.AddWithValue("coins", NpgsqlDbType.Integer, user.Coins);
                command.Parameters.AddWithValue("username", NpgsqlDbType.Varchar, user.Username);



                if (command.ExecuteNonQuery() != 1)
                {
                    throw new DatabaseCorruptedException();
                }

            }
            catch (NpgsqlException e)
            {
                // Handle exceptions
                throw;
            }
            finally { connection.Close(); }

            return true;
        }

        public UserData? GetUserBy(string method, string parameter)
        {
            UserData user;
            using var connection = new NpgsqlConnection(ConnectionString);
            try
            {
                connection.Open();

                using var command = new NpgsqlCommand();
                command.Connection = connection;
                command.CommandText = QueryDictionary[method];

                command.Parameters.AddWithValue(method, NpgsqlDbType.Varchar, parameter);
                
                var dataTable = new DataTable();
                dataTable.Load(command.ExecuteReader());


                if (dataTable.Rows.Count == 0)
                {
                    throw new UserDoesNotExistsException();
                }
                

                if (dataTable.Rows.Count > 1)
                {
                    throw new DatabaseCorruptedException();
                }

                if (dataTable.Rows[0]["username"].ToString() == null ||
                    dataTable.Rows[0]["password"].ToString() == null)
                {
                    throw new ArgumentNullException();
                }

                user = new UserData(
                        dataTable.Rows[0]["username"].ToString()!,
                        dataTable.Rows[0]["password"].ToString()!, 
                        dataTable.Rows[0]["name"].ToString(),
                        dataTable.Rows[0]["bio"].ToString(), 
                        dataTable.Rows[0]["image"].ToString(),
                        (int)dataTable.Rows[0]["coins"]
                    );
                
            }
            catch (NpgsqlException e)
            {
                // Handle exceptions
                Console.WriteLine(e.Message);
                throw;
            }
            finally { connection.Close(); }

            return user;
        }

        public List<Guid> GetPackage()
        {
            var package = new List<Guid>();
            using var connection = new NpgsqlConnection(ConnectionString);

            try
            {
                connection.Open();

                using var command = new NpgsqlCommand();
                command.Connection = connection;
                command.CommandText = QueryDictionary["package"];

                var dataTable = new DataTable();
                dataTable.Load(command.ExecuteReader()); 
                
                if (dataTable.Rows.Count == 0)
                {
                    throw new ArgumentNullException();
                }

                if (dataTable.Rows.Count > 1)
                {
                    throw new DatabaseCorruptedException();
                }

                package = new List<Guid>
                {
                    (Guid)dataTable.Rows[0]["card1"],
                    (Guid)dataTable.Rows[0]["card2"],
                    (Guid)dataTable.Rows[0]["card3"],
                    (Guid)dataTable.Rows[0]["card4"],
                    (Guid)dataTable.Rows[0]["card5"]
                };
            }
            catch (NpgsqlException e)
            {
                // Handle exceptions
                Console.WriteLine(e.Message);
            }
            finally { connection.Close(); }

            return package;
        }

        public List<CardData> GetCards(List<Guid> uuids)
        {
            var package = new List<CardData>();
            using var connection = new NpgsqlConnection(ConnectionString);
            try
            {
                connection.Open();

                foreach (var card in uuids)
                {
                    using var command = new NpgsqlCommand();
                    command.Connection = connection;
                    command.CommandText = QueryDictionary["cardId"];

                    command.Parameters.AddWithValue("id", NpgsqlDbType.Uuid, card);

                    var dataTable = new DataTable();
                    dataTable.Load(command.ExecuteReader());

                    Guid guid = (Guid)dataTable.Rows[0]["id"];
                    string name = dataTable.Rows[0]["name"].ToString();
                    double damage = (double)dataTable.Rows[0]["damage"];
                    string owner = dataTable.Rows[0]["owner"].ToString();
                    var dbCard = new CardData(guid, name, damage, owner, false);
                    package.Add(dbCard);
                }


            }
            catch (NpgsqlException e)
            {
                // Handle exceptions
                Console.WriteLine(e.Message);
            }
            finally { connection.Close(); }

            return package;
        }

        public List<CardData> GetAllCards(UserData user)
        {
            var stack = new List<CardData>();
            using var connection = new NpgsqlConnection(ConnectionString);
            try
            {
                connection.Open();
                
                using var command = new NpgsqlCommand();
                command.Connection = connection;
                command.CommandText = QueryDictionary["cardOwner"];

                command.Parameters.AddWithValue("username", NpgsqlDbType.Varchar, user.Username);

                var dataTable = new DataTable();
                dataTable.Load(command.ExecuteReader());

                for (var index = 0; index < dataTable.Rows.Count; index++)
                {
                    Guid guid = (Guid)dataTable.Rows[index]["id"];
                    string name = dataTable.Rows[index]["name"].ToString();
                    double damage = (double)dataTable.Rows[index]["damage"];
                    string owner = dataTable.Rows[index]["owner"].ToString();
                    bool deck = (bool)dataTable.Rows[index]["deck"];
                    var dbCard = new CardData(guid, name, damage, owner, deck);
                    stack.Add(dbCard);
                }
            }
            catch (NpgsqlException e)
            {
                // Handle exceptions
                Console.WriteLine(e.Message);
            }
            finally { connection.Close(); }

            return stack;
        }

        public List<CardData> GetDeck(UserData user)
        {
            var stack = new List<CardData>();
            using var connection = new NpgsqlConnection(ConnectionString);
            try
            {
                connection.Open();

                using var command = new NpgsqlCommand();
                command.Connection = connection;
                command.CommandText = QueryDictionary["deck"];

                command.Parameters.AddWithValue("username", NpgsqlDbType.Varchar, user.Username);

                var dataTable = new DataTable();
                dataTable.Load(command.ExecuteReader());

                for (var index = 0; index < dataTable.Rows.Count; index++)
                {
                    Guid guid = (Guid)dataTable.Rows[index]["id"];
                    string name = dataTable.Rows[index]["name"].ToString();
                    double damage = (double)dataTable.Rows[index]["damage"];
                    string owner = dataTable.Rows[index]["owner"].ToString();
                    bool deck = (bool)dataTable.Rows[index]["deck"];
                    var dbCard = new CardData(guid, name, damage, owner, deck);
                    stack.Add(dbCard);
                }
            }
            catch (NpgsqlException e)
            {
                // Handle exceptions
                Console.WriteLine(e.Message);
            }
            finally { connection.Close(); }

            return stack;
        }

        public bool DeletePackage()
        {
            using var connection = new NpgsqlConnection(ConnectionString);
            try
            {
                connection.Open();

                using var command = new NpgsqlCommand();
                command.Connection = connection;
                command.CommandText =
                    "DELETE FROM public.package WHERE id = (SELECT MIN(id) FROM public.package);";



                if (command.ExecuteNonQuery() != 1)
                {
                    throw new NpgsqlException();
                }

            }
            catch (NpgsqlException e)
            {
                // Handle exceptions
                throw;
            }
            finally { connection.Close(); }

            return true;
        }

        public bool ResetCards(UserData user)
        {
            using var connection = new NpgsqlConnection(ConnectionString);
            try
            {
                connection.Open();

                using var command = new NpgsqlCommand();
                command.Connection = connection;
                command.CommandText =
                    "UPDATE public.card SET deck = false WHERE owner = (@username);";

                command.Parameters.AddWithValue("username", NpgsqlDbType.Varchar, user.Username);
            }
            catch (NpgsqlException e)
            {
                // Handle exceptions
                throw;
            }
            finally { connection.Close(); }

            return true;
        }

        public bool SetDeck(List<Guid> uuids)
        {
            using var connection = new NpgsqlConnection(ConnectionString);
            try
            {
                connection.Open();

                foreach (var card in uuids)
                {
                    using var command = new NpgsqlCommand();
                    command.Connection = connection;
                    command.CommandText = "UPDATE public.card SET deck = true WHERE id = (@id);";

                    command.Parameters.AddWithValue("id", NpgsqlDbType.Uuid, card);

                    if (command.ExecuteNonQuery() != 1)
                    {
                        throw new DatabaseCorruptedException();
                    }
                }
            }
            catch (NpgsqlException e)
            {
                // Handle exceptions
                throw;
            }
            finally { connection.Close(); }
            return true;
        }

    }
}



