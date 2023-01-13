using Newtonsoft.Json;
using Npgsql;
using SWE_A1Grandner_MTCG.BattleLogic;
using SWE_A1Grandner_MTCG.Database;
using SWE_A1Grandner_MTCG.Exceptions;
using System.Text;
using SWE_A1Grandner_MTCG.MyEnum;

namespace SWE_A1Grandner_MTCG.BusinessLogic;

public class GetActionHandler : IActionHandler
{
    private readonly Dictionary<string, string> _httpRequestDictionary;
    private readonly UserData _user;

    public GetActionHandler(Dictionary<string, string> httpRequestDictionary, UserData user)
    {
        _httpRequestDictionary = httpRequestDictionary;
        _user = user;
    }

    public Task<HttpResponse> ShowAllCards()
    {
        var dataHandler = new DataHandler();

        try
        {
            var stack = dataHandler.GetAllCards(_user);
            var jsonStack = JsonConvert.SerializeObject(stack);

            return Task.Run(() =>
                new HttpResponse(HttpStatusCode.OK, jsonStack.Replace("},{", $"}},{Environment.NewLine}{{")));
        }
        catch (NpgsqlException)
        {
            return Task.Run(() => new HttpResponse(HttpStatusCode.BadRequest, "Something went wrong."));
        }
    }
    public Task<HttpResponse> ShowFancyDeck()
    {
        var dataHandler = new DataHandler();
        try
        {
            var deck = dataHandler.GetDeck(_user);
            
            if (deck.Count == 0)
            {
                return Task.Run(() => new HttpResponse(HttpStatusCode.OK, ""));
            }
            if (deck.Count != 4)
            {
                return Task.Run(() => new HttpResponse(HttpStatusCode.BadRequest, "Something went wrong."));
            }

            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"┌────────────┐┌────────────┐┌────────────┐┌────────────┐");
            stringBuilder.AppendLine($"│░░░░░░░░░░░░││░░░░░░░░░░░░││░░░░░░░░░░░░││░░░░░░░░░░░░│");
            stringBuilder.AppendLine($"│░░░░░░░░░░░░││░░░░░░░░░░░░││░░░░░░░░░░░░││░░░░░░░░░░░░│");
            stringBuilder.AppendLine($"│░░░░░░░░░░░░││░░░░░░░░░░░░││░░░░░░░░░░░░││░░░░░░░░░░░░│");
            stringBuilder.AppendLine($"│░░░░░░░░░░░░││░░░░░░░░░░░░││░░░░░░░░░░░░││░░░░░░░░░░░░│");
            stringBuilder.AppendLine($"│░░░░░░░░░░░░││░░░░░░░░░░░░││░░░░░░░░░░░░││░░░░░░░░░░░░│");
            stringBuilder.AppendLine($"│            ││            ││            ││            │");
            stringBuilder.AppendLine($"│            ││            ││            ││            │");
            stringBuilder.AppendFormat("│{0,12}││{1,12}││{2,12}││{3,12}│", deck[0].Name, deck[1].Name, deck[2].Name, deck[3].Name);
            stringBuilder.AppendLine();
            stringBuilder.AppendFormat("│{0,12:0.0}││{1,12:0.0}││{2,12:0.0}││{3,12:0.0}│", deck[0].Damage, deck[1].Damage, deck[2].Damage,
                deck[3].Damage);
            stringBuilder.AppendLine();
            stringBuilder.AppendLine($"└────────────┘└────────────┘└────────────┘└────────────┘");
            stringBuilder.AppendLine();

            return Task.Run(() => new HttpResponse(HttpStatusCode.OK, stringBuilder.ToString()));
        }
        catch (NpgsqlException)
        {
            return Task.Run(() => new HttpResponse(HttpStatusCode.BadRequest, "Something went wrong."));
        }
    }
    public Task<HttpResponse> ShowDeck()
    {
        var dataHandler = new DataHandler();
        try
        {
            var deck = dataHandler.GetDeck(_user);

            var jsonDeck = JsonConvert.SerializeObject(deck);

            return Task.Run(() =>
                new HttpResponse(HttpStatusCode.OK, jsonDeck.Replace("},{", $"}},{Environment.NewLine}{{")));
        }
        catch (NpgsqlException)
        {
            return Task.Run(() => new HttpResponse(HttpStatusCode.BadRequest, "Something went wrong."));
        }
    }

    public Task<HttpResponse> GetUserBio()
    {
        if (!_httpRequestDictionary.ContainsKey("addendumPath"))
        {
            return Task.Run(() => new HttpResponse(HttpStatusCode.BadRequest, "Something went wrong."));
        }
        if (_user.Username != _httpRequestDictionary["addendumPath"])
        {
            return Task.Run(() => new HttpResponse(HttpStatusCode.Unauthorized, "Unauthorized"));
        }
        var dataHandler = new DataHandler();
        try
        {
            var user = dataHandler.GetUserBy("username", _user.Username);
            var jsonUser = JsonConvert.SerializeObject(user);
            return Task.Run(() => new HttpResponse(HttpStatusCode.OK, jsonUser));
        }
        catch (NpgsqlException)
        {
            return Task.Run(() => new HttpResponse(HttpStatusCode.BadRequest, "Something went wrong."));
        }

    }

    public Task<HttpResponse> CheckStats()
    {
        try
        {
            var stats = new Score(_user);
            return Task.Run(() => new HttpResponse(HttpStatusCode.OK, stats.ToString()));
        }
        catch (DatabaseCorruptedException)
        {
            Console.WriteLine("Database corrupted.");
            return Task.Run(() => new HttpResponse(HttpStatusCode.BadRequest, "Something went wrong"));
        }
        catch (NpgsqlException)
        {
            return Task.Run(() => new HttpResponse(HttpStatusCode.BadRequest, "Something went wrong"));
        }
    }
    public Task<HttpResponse> CheckScoreboard()
    {
        try
        {
            var scoreBoard = new ScoreBoard();
            return Task.Run(() => new HttpResponse(HttpStatusCode.OK, scoreBoard.ToString()));
        }
        catch (NpgsqlException)
        {
            return Task.Run(() => new HttpResponse(HttpStatusCode.BadRequest, "Something went wrong"));
        }
    }
    public Task<HttpResponse> CheckTrades()
    {
        try
        {
            var dataHandler = new DataHandler();
            var trades = dataHandler.GetAllTrades();
            var jsonTrades = JsonConvert.SerializeObject(trades);

            return Task.Run(() => new HttpResponse(HttpStatusCode.OK, jsonTrades));
        }
        catch (Exception)
        {
            return Task.Run(() => new HttpResponse(HttpStatusCode.BadRequest));
        }
    }



}

