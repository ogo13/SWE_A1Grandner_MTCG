﻿using Newtonsoft.Json;
using Npgsql;
using SWE_A1Grandner_MTCG.BattleLogic;
using SWE_A1Grandner_MTCG.Database;
using SWE_A1Grandner_MTCG.Exceptions;
using SWE_A1Grandner_MTCG.MyEnum;
using System.Data;

namespace SWE_A1Grandner_MTCG.BusinessLogic;

public class PostActionHandler : IActionHandler
{
    private readonly Dictionary<string, string> _httpRequestDictionary;
    private readonly UserData? _user;
    private readonly Lobby? _battleLobby;

    public PostActionHandler(Dictionary<string, string> httpRequestDictionary, UserData? user, Lobby? battleLobby)
    {
        _httpRequestDictionary = httpRequestDictionary;
        _user = user;
        _battleLobby = battleLobby;
    }


    public Task<HttpResponse> Register()
    {
        var userData = JsonConvert.DeserializeObject<UserData>(_httpRequestDictionary["Data"]);
        var dataHandler = new DataHandler();
        //create user on database
        try
        {
            if (userData == null)
            {
                return Task.Run(() => new HttpResponse(HttpStatusCode.BadRequest, "Something went wrong"));
            }
            if (!dataHandler.InsertUser(userData))
            {
                return Task.Run(() => new HttpResponse(HttpStatusCode.BadRequest, "Something went wrong"));
            }

            return Task.Run(() => new HttpResponse(HttpStatusCode.ActionSuccess, "User successfully created"));

        }
        catch (DuplicateNameException e)
        {
            Console.WriteLine(e.Message);
            return Task.Run(() => new HttpResponse(HttpStatusCode.Duplicate, "User with same username already registered"));
        }
    }
    public Task<HttpResponse> Login()
    {
        var userData = JsonConvert.DeserializeObject<UserData>(_httpRequestDictionary["Data"]);
        var dataHandler = new DataHandler();
        //create user on database
        try
        {
            if (userData == null)
            {
                return Task.Run(() => new HttpResponse(HttpStatusCode.BadRequest, "Something went wrong"));
            }
            //get users token from DB
            var dbData = dataHandler.GetUserBy("username", userData.Username);

            //if (dbData == null)
            //{
            //return Task.Run(() => new HttpResponse(HttpStatusCode.Unauthorized, "Unauthorized"));
            //}

            if (userData.Password != dbData.Password)
            {
                return Task.Run(() => new HttpResponse(HttpStatusCode.Unauthorized, "Unauthorized"));
            }

            return Task.Run(() => new HttpResponse(HttpStatusCode.OK, $"{dbData.Username}-mtcgToken"));

        }
        catch (UserDoesNotExistsException)
        {
            return Task.Run(() => new HttpResponse(HttpStatusCode.Unauthorized, "Unauthorized"));
        }
    }

    public Task<HttpResponse> CreatePackage()
    {
        if (_httpRequestDictionary["Authorization"] != "Basic admin-mtcgToken")
        {
            return Task.Run(() => new HttpResponse(HttpStatusCode.Unauthorized, "Unauthorized"));
        }

        var packageData = JsonConvert.DeserializeObject<List<CardData>>(_httpRequestDictionary["Data"]);
        var dataHandler = new DataHandler();

        try
        {
            if (packageData == null)
            {
                return Task.Run(() => new HttpResponse(HttpStatusCode.BadRequest, "Something went wrong."));
            }

            if (packageData.Count != 5)
            {
                return Task.Run(() =>
                    new HttpResponse(HttpStatusCode.BadRequest, "Not the right amount of cards for a package."));
            }

            var cardsIds = new List<Guid>();

            var success = true;
            foreach (var cardData in packageData)
            {
                success = success && dataHandler.InsertCard(cardData);
                cardsIds.Add(cardData.Id);
            }

            if (!(success && dataHandler.InsertPackage(cardsIds)))
            {
                Task.Run(() => new HttpResponse(HttpStatusCode.BadRequest, "Something went wrong"));
            }

            return Task.Run(() =>
                new HttpResponse(HttpStatusCode.ActionSuccess, "Package and cards successfully created"));
        }
        catch (DuplicateNameException)
        {
            return Task.Run(() =>
                new HttpResponse(HttpStatusCode.Duplicate, "At least one card in the packages already exists"));
        }
        catch (NpgsqlException)
        {
            return Task.Run(() => new HttpResponse(HttpStatusCode.BadRequest, "Something went wrong"));
        }

    }
    public Task<HttpResponse> BuyPackage()
    {
        var dataHandler = new DataHandler();

        try
        {
            var user = dataHandler.GetUserBy("token", _httpRequestDictionary["Authorization"].Split(" ")[1]);

            if (user.Coins < 5)
            {
                return Task.Run(() => new HttpResponse(HttpStatusCode.BadRequest, "Not enough money for buying a card package"));
            }

            var pack = dataHandler.GetPackage();
            user.Coins -= 5;
            dataHandler.UpdateUser(user);
            dataHandler.DeletePackage();
            dataHandler.UpdateOwnerInCards(pack, user.Username);

            var cards = dataHandler.GetCards(pack);
            var jsonCards = JsonConvert.SerializeObject(cards);


            return Task.Run(() => new HttpResponse(HttpStatusCode.OK, jsonCards));

        }
        catch (UserDoesNotExistsException)
        {
            return Task.Run(() => new HttpResponse(HttpStatusCode.Unauthorized, "Unauthorized"));
        }
        catch (ArgumentNullException)
        {
            return Task.Run(() => new HttpResponse(HttpStatusCode.NotFound, "No card package available for buying"));
        }
    }

    public Task<HttpResponse> Battle()
    {
        try
        {
            _battleLobby!.AddPlayer(_user!);
            var result = _battleLobby.GetResult(_user!).ToString();
            return Task.Run(() => new HttpResponse(HttpStatusCode.OK, result));
        }
        catch (NpgsqlException)
        {
            return Task.Run(() => new HttpResponse(HttpStatusCode.BadRequest, "Something went wrong"));
        }
    }

    public Task<HttpResponse> Trade()
    {
        return _httpRequestDictionary.ContainsKey("addendumPath") ? DoTrade() : PostTrade();
    }
    private Task<HttpResponse> PostTrade()
    {
        try
        {
            var dataHandler = new DataHandler();
            var trade = JsonConvert.DeserializeObject<TradeData>(_httpRequestDictionary["Data"]);
            if (trade == null)
            {
                return Task.Run(() => new HttpResponse(HttpStatusCode.BadRequest, "Something went wrong."));
            }

            if (dataHandler.GetCardById(trade.CardToTrade).Owner != _user!.Username)
            {
                return Task.Run(() => new HttpResponse(HttpStatusCode.BadRequest, "Something went wrong."));
            }

            trade.Owner = _user!.Username;

            dataHandler.InsertTrade(trade);
            return Task.Run(() => new HttpResponse(HttpStatusCode.OK, "Trade successfully created"));
        }
        catch (DuplicateNameException)
        {
            return Task.Run(() => new HttpResponse(HttpStatusCode.BadRequest, "Trade with this id already exists."));
        }
    }
    private Task<HttpResponse> DoTrade()
    {
        try
        {
            var dataHandler = new DataHandler();
            var tradeId = Guid.Parse(_httpRequestDictionary["addendumPath"]);
            var cardId = Guid.Parse(_httpRequestDictionary["Data"].Trim('"'));
            var trade = dataHandler.GetTradeById(tradeId);
            var card = dataHandler.GetCardById(cardId);
            var cardType = card.Name.Contains("Spell") ? TradeType.Spell : TradeType.Monster;

            if (trade.Owner == _user!.Username)
            {
                return Task.Run(() => new HttpResponse(HttpStatusCode.BadRequest, "You cannot trade with yourself."));
            }

            if (trade.MinimumDamage > card.Damage)
            {
                return Task.Run(() => new HttpResponse(HttpStatusCode.BadRequest, "Your card does not fit this trade."));
            }

            if (trade.Type != cardType)
            {
                return Task.Run(() => new HttpResponse(HttpStatusCode.BadRequest, "Your card does not fit this trade."));
            }

            if (!ExecuteTrade(trade, card))
            {
                return Task.Run(() => new HttpResponse(HttpStatusCode.BadRequest, "Something went wrong."));
            }

            return Task.Run(() => new HttpResponse(HttpStatusCode.OK, "Trade successful"));

        }
        catch (ArgumentNullException)
        {
            return Task.Run(() => new HttpResponse(HttpStatusCode.NotFound, "No Trade with this id."));
        }
        catch (NpgsqlException)
        {
            return Task.Run(() => new HttpResponse(HttpStatusCode.BadRequest, "Something went wrong."));
        }
    }
    private bool ExecuteTrade(TradeData trade, CardData card)
    {
        var dataHandler = new DataHandler();
        var tradeCard = dataHandler.GetCardById(trade.CardToTrade);
        card.Owner = trade.Owner;
        tradeCard.Owner = _user!.Username;
        dataHandler.UpdateOwnerInOneCard(card.Id, card.Owner!);
        dataHandler.UpdateOwnerInOneCard(tradeCard.Id, tradeCard.Owner!);
        return dataHandler.DeleteTrade(trade.Id);
    }
}

