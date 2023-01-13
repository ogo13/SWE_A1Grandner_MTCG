using SWE_A1Grandner_MTCG.Database;
using HttpStatusCode = SWE_A1Grandner_MTCG.MyEnum.HttpStatusCode;
using SWE_A1Grandner_MTCG.BattleLogic;

namespace SWE_A1Grandner_MTCG.BusinessLogic;

public class RequestHandler
{
    private readonly Dictionary<string, string> _httpRequestDictionary;

    public RequestHandler(Dictionary<string, string> httpRequestDictionary)
    {
        _httpRequestDictionary = httpRequestDictionary;
    }
    
    public Task<HttpResponse> HandleRequest(Lobby battleLobby)
    {
        return _httpRequestDictionary["Method"] switch
        {
            "GET" => HandleGetRequest(),
            "POST" => HandlePostRequest(battleLobby),
            "PUT" => HandlePutRequest(),
            "DELETE" => HandleDeleteRequest(),
            _ => Task.Run(() => new HttpResponse(HttpStatusCode.BadRequest))
        };
    }

    private Task<HttpResponse> HandleDeleteRequest()
    {
        UserData user;

        //check Authorization exception
        try
        {
            user = new DataHandler().GetUserBy("token", _httpRequestDictionary["Authorization"].Split(" ")[1]);
        }
        catch (ArgumentNullException)
        {
            return Task.Run(() => new HttpResponse(HttpStatusCode.Unauthorized, "Unauthorized"));
        }

        var actionHandler = new DeleteActionHandler(_httpRequestDictionary, user, new DataHandler());

        return _httpRequestDictionary["Path"] switch
        {
            "tradings" => actionHandler.DeleteTrade(),
            _ => Task.Run(() => new HttpResponse(HttpStatusCode.BadRequest))
        };

    }

    private Task<HttpResponse> HandleGetRequest()
    {
        UserData user;
        
        //check Authorization exception
        if (!_httpRequestDictionary.ContainsKey("Authorization"))
        {
            return Task.Run(() => new HttpResponse(HttpStatusCode.Unauthorized, "Unauthorized"));
        }
        try
        {
            user = new DataHandler().GetUserBy("token", _httpRequestDictionary["Authorization"].Split(" ")[1]);
        }
        catch (ArgumentNullException)
        {
            return Task.Run(() => new HttpResponse(HttpStatusCode.Unauthorized, "Unauthorized"));
        }

        var actionHandler = new GetActionHandler(_httpRequestDictionary, user, new DataHandler());

        return _httpRequestDictionary["Path"] switch
        {
            "cards" => actionHandler.ShowAllCards(),
            "deck" => actionHandler.ShowFancyDeck(),
            "deck?format=plain" => actionHandler.ShowDeck(),
            "users" => actionHandler.GetUserBio(),
            "stats" => actionHandler.CheckStats(),
            "score" => actionHandler.CheckScoreboard(),
            "tradings" => actionHandler.CheckTrades(),
            _ => Task.Run(() => new HttpResponse(HttpStatusCode.BadRequest))
        };
    }

    private Task<HttpResponse> HandlePostRequest(Lobby battleLobby)
    {

        var actionHandler = new PostActionHandler(_httpRequestDictionary, null, null, new DataHandler());

        switch (_httpRequestDictionary["Path"])
        {
            case "users":
                return actionHandler.Register();
            case "sessions":
                return actionHandler.Login();
        }

        //check Authorization

        UserData user;
        //check Authorization exception
        try
        {
            user = new DataHandler().GetUserBy("token", _httpRequestDictionary["Authorization"].Split(" ")[1]);
        }
        catch (Exception)
        {
            return Task.Run(() => new HttpResponse(HttpStatusCode.Unauthorized, "Unauthorized"));
        }

        actionHandler = new PostActionHandler(_httpRequestDictionary, user, battleLobby, new DataHandler());

        return _httpRequestDictionary["Path"] switch
        {
            "packages" => actionHandler.CreatePackage(),
            "transactions" => actionHandler.BuyPackage(),
            "battles" => actionHandler.Battle(),
            "tradings" => actionHandler.Trade(),
            _ => Task.Run(() => new HttpResponse(HttpStatusCode.BadRequest))
        };
    }

    private Task<HttpResponse> HandlePutRequest()
    {
        UserData user;
        //check Authorization exception
        
        try
        {
            user = new DataHandler().GetUserBy("token", _httpRequestDictionary["Authorization"].Split(" ")[1]);
        }
        catch (Exception)
        {
            return Task.Run(() => new HttpResponse(HttpStatusCode.Unauthorized, "Unauthorized"));
        }

        var actionHandler = new PutActionHandler(_httpRequestDictionary, user, new DataHandler());

        return _httpRequestDictionary["Path"] switch
        {
            "users" => actionHandler.ConfigureUser(),
            "deck" => actionHandler.ConfigureDeck(),
            _ => Task.Run(() => new HttpResponse(HttpStatusCode.BadRequest))
        };

        
    }
}



