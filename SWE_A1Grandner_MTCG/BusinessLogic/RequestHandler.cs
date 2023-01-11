using Newtonsoft.Json;
using Npgsql;
using System.Net.Sockets;
using System.ComponentModel.DataAnnotations;
using System.Data;
using SWE_A1Grandner_MTCG.Database;
using SWE_A1Grandner_MTCG.Exceptions;
using HttpStatusCode = SWE_A1Grandner_MTCG.MyEnum.HttpStatusCode;
using System.Collections.Generic;

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
    
    public Task<HttpResponse> HandleRequest()
    {
        return _httpRequestDictionary["Method"] switch
        {
            "GET" => HandleGetRequest(),
            "POST" => HandlePostRequest(),
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


        switch (_httpRequestDictionary["Path"])
        {
            case "cards":
            {
                var stack = ActionHandler.ShowAllCards(user);
                var jsonStack = JsonConvert.SerializeObject(stack);
                return Task.Run(() => new HttpResponse(HttpStatusCode.OK, jsonStack.Replace("},{", $"}},{Environment.NewLine}{{")));
            }
            case "deck":
            {
                var deck = ActionHandler.ShowDeck(user);
                //var jsonDeck = JsonConvert.SerializeObject(deck);
                var fancyDeck = ActionHandler.GetFancyDeck(deck);
                return Task.Run(() => new HttpResponse(HttpStatusCode.OK, fancyDeck));
                //return Task.Run(() => new HttpResponse(HttpStatusCode.OK, jsonDeck.Replace("},{", $"}},{Environment.NewLine}{{")));
            }
            case "deck?format=plain":
            {
                var deck = ActionHandler.ShowDeck(user);
                var jsonDeck = JsonConvert.SerializeObject(deck);
                return Task.Run(() => new HttpResponse(HttpStatusCode.OK, jsonDeck.Replace("},{", $"}},{Environment.NewLine}{{")));
            }
            case "users": 
            {
                if (user.Username != _httpRequestDictionary["addendumPath"])
                {
                    return Task.Run(() => new HttpResponse(HttpStatusCode.Unauthorized, "Unauthorized"));
                }
                user = ActionHandler.GetUserBio(user);
                var jsonUser = JsonConvert.SerializeObject(user);
                return Task.Run(() => new HttpResponse(HttpStatusCode.OK, jsonUser));
            }
            case "stats":
            {


                //check stats


                break;
            }
            case "score":
            {


                //check score   


                break;
            }
            case "tradings":
            {


                //check trades   


                break;
            }

        }

        return Task.Run(() => new HttpResponse(HttpStatusCode.BadRequest));
    }

    private Task<HttpResponse> HandlePostRequest()
    {

        if (_httpRequestDictionary["Path"] == "users")
        {
            var userData = JsonConvert.DeserializeObject<UserData>(_httpRequestDictionary["Data"]);
            //create user on database
            try
            {
                return ActionHandler.Register(userData)
                    ? Task.Run(() => new HttpResponse(HttpStatusCode.ActionSuccess, "User successfully created"))
                    : Task.Run(() => new HttpResponse(HttpStatusCode.BadRequest, "Something went wrong"));

            }
            catch (DuplicateNameException e)
            {
                Console.WriteLine(e.Message);
                return Task.Run(() => new HttpResponse(HttpStatusCode.Duplicate, "User with same username already registered"));
            }


        }
        if (_httpRequestDictionary["Path"] == "sessions")
        {
            var userData = JsonConvert.DeserializeObject<UserData>(_httpRequestDictionary["Data"]);
            
            //login user from database
            try
            {
                var login =  ActionHandler.Login(userData);
                return Task.Run(() => new HttpResponse(HttpStatusCode.OK, $"{login.Username}-mtcgToken"));
            }
            catch (UserDoesNotExistsException e)
            {
                Console.WriteLine(e.Message);
                return Task.Run(() => new HttpResponse(HttpStatusCode.Unauthorized, "Unauthorized"));
            }
            catch (ValidationException e)
            {
                Console.WriteLine(e.Message);
                return Task.Run(() => new HttpResponse(HttpStatusCode.Unauthorized, "Unauthorized"));
            }
        }


        //check Authorization
        
        if (!_httpRequestDictionary.ContainsKey("Authorization"))
        {
            return Task.Run(() => new HttpResponse(HttpStatusCode.Unauthorized, "Unauthorized"));
        }
        try
        {
            new DataHandler().GetUserBy("token", _httpRequestDictionary["Authorization"].Split(" ")[1]);
        }
        catch (ArgumentNullException)
        {
            return Task.Run(() => new HttpResponse(HttpStatusCode.Unauthorized, "Unauthorized"));
        }


        switch (_httpRequestDictionary["Path"])
        {
            case "packages":
                {
                    //check Admin
                    if (_httpRequestDictionary["Authorization"] != "Basic admin-mtcgToken") 
                    {
                        return Task.Run(() => new HttpResponse(HttpStatusCode.Unauthorized, "Unauthorized"));
                    }

                    try
                    {
                        var cards = JsonConvert.DeserializeObject<List<CardData>>(_httpRequestDictionary["Data"]);

                        var successfulPackage = ActionHandler.CreatePackage(cards);

                        return successfulPackage 
                            ? Task.Run(() => new HttpResponse(HttpStatusCode.ActionSuccess, "Package and cards successfully created")) 
                            : Task.Run(() => new HttpResponse(HttpStatusCode.BadRequest, "Something went wrong"));
                    }
                    catch (DuplicateNameException)
                    {
                        Console.WriteLine("Database corrupt!");
                        return Task.Run(() => new HttpResponse(HttpStatusCode.Duplicate, "At least one card in the packages already exists"));
                    }
                    catch (NpgsqlException)
                    {
                        return Task.Run(() => new HttpResponse(HttpStatusCode.BadRequest, "Something went wrong"));
                    }
                }
            case "transactions":
            {

                try
                {
                    var successfulTransaction = ActionHandler.BuyPackage(_httpRequestDictionary["Authorization"]);
                    var jsonCards = JsonConvert.SerializeObject(successfulTransaction);
                    return Task.Run(() => new HttpResponse(HttpStatusCode.OK, jsonCards.ToString()));
                }
                catch (UserDoesNotExistsException)
                {
                    return Task.Run(() => new HttpResponse(HttpStatusCode.NotFound));
                }
                catch (NotEnoughFundsException)
                {
                    return Task.Run(() => new HttpResponse(HttpStatusCode.BadRequest, "Not enough money for buying a card package"));
                }
                catch (ArgumentNullException)
                {
                    return Task.Run(() => new HttpResponse(HttpStatusCode.NotFound, "No card package available for buying"));
                }
            }
            case "tradings":
                {

                    TradeData? trade = JsonConvert.DeserializeObject<TradeData>(_httpRequestDictionary["Data"]);

                    break;
                }

        }

        return Task.Run(() => new HttpResponse(HttpStatusCode.BadRequest));
    }

    private Task<HttpResponse> HandlePutRequest()
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


        switch (_httpRequestDictionary["Path"])
        {
            case "users": 
            {
                if (user.Username != _httpRequestDictionary["addendumPath"])
                {
                    return Task.Run(() => new HttpResponse(HttpStatusCode.Unauthorized, "Unauthorized"));
                }

                //configure user
                var userInfo = JsonConvert.DeserializeObject<UserInfo>(_httpRequestDictionary["Data"]);
                var successfulUser = ActionHandler.ConfigureUser(user, userInfo);
                return successfulUser
                    ? Task.Run(() =>
                        new HttpResponse(HttpStatusCode.ActionSuccess, "User successfully configured"))
                    : Task.Run(() => new HttpResponse(HttpStatusCode.BadRequest, "Something went wrong"));

            }
            case "deck":
            {
                //configure deck
                try
                {
                    var cards = JsonConvert.DeserializeObject<List<Guid>>(_httpRequestDictionary["Data"]);
                    var successfulDeck = ActionHandler.ConfigureDeck(user, cards);

                    return successfulDeck
                        ? Task.Run(() =>
                            new HttpResponse(HttpStatusCode.ActionSuccess, "Deck successfully configured"))
                        : Task.Run(() => new HttpResponse(HttpStatusCode.BadRequest, "Something went wrong"));
                }
                catch (UnauthorizedAccessException)
                {
                    return Task.Run(() => new HttpResponse(HttpStatusCode.Unauthorized, "Unauthorized"));
                }
                catch (ArgumentNullException)
                {
                    return Task.Run(() => new HttpResponse(HttpStatusCode.BadRequest, "No cards declared"));
                }
                catch (InvalidOperationException)
                {
                    return Task.Run(() => new HttpResponse(HttpStatusCode.BadRequest, "Declare exactly four cards"));
                }
            }
        }
        return Task.Run(() => new HttpResponse(HttpStatusCode.BadRequest)); ;
    }
}



