using System.Data;
using Npgsql;
using NpgsqlTypes;
using SWE_A1Grandner_MTCG.BattleLogic;
using SWE_A1Grandner_MTCG.Exceptions;
using SWE_A1Grandner_MTCG.MyEnum;

namespace SWE_A1Grandner_MTCG.Database;
public class DataHandler
{
    public static string ConnectionString => "Host=localhost;Username=postgres;Password=postgres;Database=MTCG";

    private static readonly Dictionary<string, string> QueryDictionary = new()
    {
        {"token", "SELECT * FROM public.user WHERE token = (@token);"},
        {"username", "SELECT * FROM public.user WHERE username = (@username);"},
        {"cardById", "SELECT * FROM public.card WHERE id = (@id);"},
        {"cardOwner", "SELECT * FROM public.card WHERE owner = (@username);"},
        {"deck", "SELECT * FROM public.card WHERE owner = (@username) AND deck = true;"},
        {"package", "SELECT * FROM public.package WHERE id = (SELECT MIN(id) FROM public.package);"},
        {"score", "SELECT * FROM public.score WHERE username = (@username);"},
        {"scoreBoard", "SELECT * FROM public.score;"},
        {"trade", "SELECT * FROM public.trade;"},
        {"tradeById", "SELECT * FROM public.trade WHERE id = (@id);"},
    };

    public bool InsertUser(UserData user)
    {
        using var connection = new NpgsqlConnection(ConnectionString);
        try
        {
            connection.Open();

            using var command = new NpgsqlCommand();
            command.Connection = connection;
            command.CommandText =
                "INSERT INTO public.user(username, password, token, coins) VALUES (@username, @password, @token, @coins);";

            command.Parameters.AddWithValue("username", NpgsqlDbType.Varchar, user.Username);
            command.Parameters.AddWithValue("password", NpgsqlDbType.Varchar, user.Password);
            command.Parameters.AddWithValue("token", NpgsqlDbType.Varchar, user.Username + "-mtcgToken");
            command.Parameters.AddWithValue("coins", NpgsqlDbType.Integer, 20);

            if (command.ExecuteNonQuery() == 0)
            {
                return false;
            }

            InsertScore(user);

        }
        catch (NpgsqlException)
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
        catch (NpgsqlException)
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
        finally { connection.Close(); }

        return true;
    }
    private void InsertScore(UserData user)
    {
        using var connection = new NpgsqlConnection(ConnectionString);
        try
        {
            connection.Open();

            using var command = new NpgsqlCommand();
            command.Connection = connection;
            command.CommandText = "INSERT INTO public.score(username) VALUES (@username);";

            command.Parameters.AddWithValue("username", NpgsqlDbType.Varchar, user.Username);

            command.ExecuteNonQuery();

        }
        finally { connection.Close(); }
    }
    public bool InsertTrade(TradeData tradeData)
    {
        using var connection = new NpgsqlConnection(ConnectionString);
        try
        {
            connection.Open();

            using var command = new NpgsqlCommand();
            command.Connection = connection;
            command.CommandText = "INSERT INTO public.trade(id, card, type, minimumdamage, owner) VALUES (@id, @card, @type, @minimumdamage, @owner);";

            command.Parameters.AddWithValue("id", NpgsqlDbType.Uuid, tradeData.Id);
            command.Parameters.AddWithValue("card", NpgsqlDbType.Uuid, tradeData.CardToTrade); 
            command.Parameters.AddWithValue("type", NpgsqlDbType.Varchar, tradeData.Type.ToString());
            command.Parameters.AddWithValue("minimumdamage", NpgsqlDbType.Double, tradeData.MinimumDamage);
            command.Parameters.AddWithValue("owner", NpgsqlDbType.Varchar, tradeData.Owner!);

            command.ExecuteNonQuery();

        }
        catch (NpgsqlException)
        {
            // Handle exceptions
            throw new DuplicateNameException("Trade already exists");
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
        finally { connection.Close(); }

        return true;
    }

    public bool UpdateOwnerInOneCard(Guid uuid, string user)
    {
        using var connection = new NpgsqlConnection(ConnectionString);
        try
        {
            connection.Open();

            using var command = new NpgsqlCommand();
            command.Connection = connection;
            command.CommandText =
                "UPDATE public.card SET owner = (@user) WHERE id = (@card1);";

            command.Parameters.AddWithValue("user", NpgsqlDbType.Varchar, user);
            command.Parameters.AddWithValue("card1", NpgsqlDbType.Uuid, uuid);

            if (command.ExecuteNonQuery() != 1)
            {
                throw new ArgumentNullException();
            }
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

            
#pragma warning disable CS8604
            command.Parameters.AddWithValue("name", NpgsqlDbType.Varchar, user.Name);
            command.Parameters.AddWithValue("bio", NpgsqlDbType.Varchar, user.Bio);
            command.Parameters.AddWithValue("image", NpgsqlDbType.Varchar, user.Image);
#pragma warning restore CS8604
            command.Parameters.AddWithValue("coins", NpgsqlDbType.Integer, user.Coins);
            command.Parameters.AddWithValue("username", NpgsqlDbType.Varchar, user.Username);



            if (command.ExecuteNonQuery() != 1)
            {
                throw new DatabaseCorruptedException();
            }

        }
        finally { connection.Close(); }

        return true;
    }
    public bool UpdateScore(Score score)
    {
        using var connection = new NpgsqlConnection(ConnectionString);
        try
        {
            connection.Open();

            using var command = new NpgsqlCommand();
            command.Connection = connection;
            command.CommandText =
                "UPDATE public.score SET wins = (@wins), draws = (@draws), losses = (@losses), elo = (@elo) WHERE username = (@username);";

            command.Parameters.AddWithValue("wins", NpgsqlDbType.Integer, score.Wins);
            command.Parameters.AddWithValue("draws", NpgsqlDbType.Integer, score.Draws);
            command.Parameters.AddWithValue("losses", NpgsqlDbType.Integer, score.Losses);
            command.Parameters.AddWithValue("elo", NpgsqlDbType.Integer, score.Elo);
            command.Parameters.AddWithValue("username", NpgsqlDbType.Varchar, score.Player.Username);

            if (command.ExecuteNonQuery() != 1)
            {
                throw new DatabaseCorruptedException();
            }

        }
        finally { connection.Close(); }

        return true;
    }



    public UserData GetUserBy(string method, string parameter)
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

    public CardData GetCardById(Guid id)
    {
        var dataTable = new DataTable();
        using var connection = new NpgsqlConnection(ConnectionString);
        try
        {
            connection.Open();

            using var command = new NpgsqlCommand();
            command.Connection = connection;
            command.CommandText = QueryDictionary["cardById"];

            command.Parameters.AddWithValue("id", NpgsqlDbType.Uuid, id);


            dataTable.Load(command.ExecuteReader());

            if (dataTable.Rows.Count == 0)
            {
                throw new ArgumentNullException();
            }
            if (dataTable.Rows.Count > 1)
            {
                throw new DatabaseCorruptedException();
            }


        }
        finally { connection.Close(); }
        return new CardData(id, dataTable.Rows[0]["name"].ToString()!, (double)dataTable.Rows[0]["damage"], dataTable.Rows[0]["owner"].ToString(), (bool)dataTable.Rows[0]["deck"]);
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
                command.CommandText = QueryDictionary["cardById"];

                command.Parameters.AddWithValue("id", NpgsqlDbType.Uuid, card);

                var dataTable = new DataTable();
                dataTable.Load(command.ExecuteReader());

                Guid guid = (Guid)dataTable.Rows[0]["id"];
                string name = dataTable.Rows[0]["name"].ToString()!;
                double damage = (double)dataTable.Rows[0]["damage"];
                string owner = dataTable.Rows[0]["owner"].ToString() ?? string.Empty;
                bool deck = (bool)dataTable.Rows[0]["deck"];
                var dbCard = new CardData(guid, name, damage, owner, deck);
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
                string name = dataTable.Rows[index]["name"].ToString()!;
                double damage = (double)dataTable.Rows[index]["damage"];
                string owner = dataTable.Rows[index]["owner"].ToString()!;
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
                string name = dataTable.Rows[index]["name"].ToString()!;
                double damage = (double)dataTable.Rows[index]["damage"];
                string owner = dataTable.Rows[index]["owner"].ToString()!;
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
    public ScoreData GetScore(UserData user)
    {
        var dataTable = new DataTable();
        using var connection = new NpgsqlConnection(ConnectionString);
        try
        {
            connection.Open();

            using var command = new NpgsqlCommand();
            command.Connection = connection;
            command.CommandText = QueryDictionary["score"];

            command.Parameters.AddWithValue("username", NpgsqlDbType.Varchar, user.Username);

            
            dataTable.Load(command.ExecuteReader());

            if (dataTable.Rows.Count == 0)
            {
                throw new ArgumentNullException();
            }
            if (dataTable.Rows.Count > 1)
            {
                throw new DatabaseCorruptedException();
            }
        }
        finally { connection.Close(); }

        return new ScoreData(user.Username, (int)dataTable.Rows[0]["wins"], (int)dataTable.Rows[0]["draws"], (int)dataTable.Rows[0]["losses"], (int)dataTable.Rows[0]["elo"]);
    }
    public List<ScoreData> GetScoreBoard()
    {
        var scoreBoard = new List<ScoreData>();
        var dataTable = new DataTable();
        using var connection = new NpgsqlConnection(ConnectionString);
        try
        {
            connection.Open();

            using var command = new NpgsqlCommand();
            command.Connection = connection;
            command.CommandText = QueryDictionary["scoreBoard"];


            dataTable.Load(command.ExecuteReader());

            for (var index = 0; index < dataTable.Rows.Count; index++)
            {
                var username = dataTable.Rows[index]["username"].ToString()!;
                var wins = (int)dataTable.Rows[index]["wins"];
                var draws = (int)dataTable.Rows[index]["draws"];
                var losses = (int)dataTable.Rows[index]["losses"];
                var elo = (int)dataTable.Rows[index]["elo"];
                var dbScoreData = new ScoreData(username, wins, draws, losses, elo); 
                scoreBoard.Add(dbScoreData);
            }

        }
        
        finally { connection.Close(); }

        return scoreBoard;
    }
    public List<TradeData> GetAllTrades()
    {
        var trades = new List<TradeData>();
        var dataTable = new DataTable();
        using var connection = new NpgsqlConnection(ConnectionString);
        try
        {
            connection.Open();

            using var command = new NpgsqlCommand();
            command.Connection = connection;
            command.CommandText = QueryDictionary["trade"];

            dataTable.Load(command.ExecuteReader());

            for (var index = 0; index < dataTable.Rows.Count; index++)
            {
                var guid = (Guid)dataTable.Rows[index]["id"];
                var card = (Guid)dataTable.Rows[index]["card"];
                Enum.TryParse<TradeType>(dataTable.Rows[index]["type"].ToString(), out var type);
                var minimumdamage = (double)dataTable.Rows[index]["minimumdamage"];
                var owner = dataTable.Rows[index]["owner"].ToString()!;
                var dbTrade = new TradeData(guid, card, type, minimumdamage, owner);
                trades.Add(dbTrade);
            }
        }
        finally { connection.Close(); }

        return trades;
    }

    public TradeData GetTradeById(Guid id)
    {
        var dataTable = new DataTable();
        using var connection = new NpgsqlConnection(ConnectionString);
        try
        {
            connection.Open();

            using var command = new NpgsqlCommand();
            command.Connection = connection;
            command.CommandText = QueryDictionary["tradeById"];

            command.Parameters.AddWithValue("id", NpgsqlDbType.Uuid, id);


            dataTable.Load(command.ExecuteReader());

            if (dataTable.Rows.Count == 0)
            {
                throw new ArgumentNullException();
            }
            if (dataTable.Rows.Count > 1)
            {
                throw new DatabaseCorruptedException();
            }

            
        }
        finally { connection.Close(); }
        Enum.TryParse<TradeType>(dataTable.Rows[0]["type"].ToString(), out var type);
        return new TradeData(id, (Guid)dataTable.Rows[0]["card"], type, (double)dataTable.Rows[0]["minimumdamage"], dataTable.Rows[0]["owner"].ToString());
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
        finally { connection.Close(); }

        return true;
    }

    public bool DeleteTrade(Guid trade)
    {
        using var connection = new NpgsqlConnection(ConnectionString);
        try
        {
            connection.Open();

            using var command = new NpgsqlCommand();
            command.Connection = connection;
            command.CommandText =
                "DELETE FROM public.trade WHERE id = (@id);";

            command.Parameters.AddWithValue("id", NpgsqlDbType.Uuid, trade);

            if (command.ExecuteNonQuery() != 1)
            {
                throw new NpgsqlException();
            }

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
            command.ExecuteNonQuery();

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
        finally { connection.Close(); }
        return true;
    }

}




