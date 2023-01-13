using Newtonsoft.Json;
using Npgsql;
using System.Net.Sockets;
using System.ComponentModel.DataAnnotations;
using System.Data;
using SWE_A1Grandner_MTCG.Database;
using SWE_A1Grandner_MTCG.Exceptions;
using HttpStatusCode = SWE_A1Grandner_MTCG.MyEnum.HttpStatusCode;
using System.Collections.Generic;
using SWE_A1Grandner_MTCG.BattleLogic;

namespace SWE_A1Grandner_MTCG.BusinessLogic;

internal class RequestHandler
{
    private readonly Dictionary<string, string> _httpRequestDictionary;
    private readonly TcpClient _client;

    public RequestHandler(Dictionary<string, string> httpRequestDictionary, TcpClient client)
    {
        _httpRequestDictionary = httpRequestDictionary;
        _client = client;
    }
    
    public Task<HttpResponse> HandleRequest(Lobby battleLobby)
    {
        return _httpRequestDictionary["Method"] switch
        {
            "GET" => HandleGetRequest(),
            "POST" => HandlePostRequest(battleLobby),
            "PUT" => HandlePutRequest(),
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
            user = new DataHandler().GetUserBy("token", _httpRequestDictionary["Authorization"].Split(" ")[1])!;
        }
        catch (ArgumentNullException)
        {
            return Task.Run(() => new HttpResponse(HttpStatusCode.Unauthorized, "Unauthorized"));
        }

        var actionHandler = new ActionHandler(_httpRequestDictionary, user, null);

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

        var actionHandler = new PostActionHandler(_httpRequestDictionary, null, null);

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

        actionHandler = new PostActionHandler(_httpRequestDictionary, user, battleLobby);

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

        var actionHandler = new ActionHandler(_httpRequestDictionary, user, null);

        return _httpRequestDictionary["Path"] switch
        {
            "users" => actionHandler.ConfigureUser(),
            "deck" => actionHandler.ConfigureDeck(),
            _ => Task.Run(() => new HttpResponse(HttpStatusCode.BadRequest))
        };

        ;
    }
}



