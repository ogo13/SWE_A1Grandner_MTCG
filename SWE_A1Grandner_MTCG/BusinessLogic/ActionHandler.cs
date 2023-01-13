using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Npgsql;
using SWE_A1Grandner_MTCG.BattleLogic;
using SWE_A1Grandner_MTCG.Database;
using SWE_A1Grandner_MTCG.Exceptions;
using SWE_A1Grandner_MTCG.MyEnum;

namespace SWE_A1Grandner_MTCG.BusinessLogic
{
    internal class ActionHandler
    {
        protected Dictionary<string, string> _httpRequestDictionary;
        protected UserData? _user;
        protected Lobby? _battleLobby;

        public ActionHandler()
        {
            _httpRequestDictionary = new Dictionary<string, string>();
        }
        public ActionHandler(Dictionary<string, string> httpRequestDictionary, UserData? user, Lobby? battleLobby)
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
            catch (UserDoesNotExistsException e)
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

            try{
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
            catch (ArgumentNullException e)
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
            return dataHandler.DeleteTrade(trade);
        }


        public Task<HttpResponse> ShowAllCards()
        {
            var dataHandler = new DataHandler();

            try
            {
                var stack = dataHandler.GetAllCards(_user!);
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
                var deck = dataHandler.GetDeck(_user!);

                var names = new List<string>();
                var damages = new List<string>();

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
                var deck = dataHandler.GetDeck(_user!);

                var jsonDeck = JsonConvert.SerializeObject(deck);

                return Task.Run(() =>
                    new HttpResponse(HttpStatusCode.OK, jsonDeck.Replace("},{", $"}},{Environment.NewLine}{{")));
            }
            catch (NpgsqlException)
            {
                return Task.Run(() => new HttpResponse(HttpStatusCode.BadRequest, "Something went wrong."));
            }
        }

        public Task<HttpResponse> ConfigureDeck()
        {
            try
            {
                var cards = JsonConvert.DeserializeObject<List<Guid>>(_httpRequestDictionary["Data"]);

                if (cards == null)
                {
                    return Task.Run(() => new HttpResponse(HttpStatusCode.BadRequest, "No cards declared"));
                }

                if (cards.Count != 4)
                {
                    return Task.Run(() => new HttpResponse(HttpStatusCode.BadRequest, "Declare exactly four cards"));
                }

                var dataHandler = new DataHandler();


                //check if cards belong to user
                var allCardsOfUserBuffer = dataHandler.GetAllCards(_user!);
                var allCardsOfUser = allCardsOfUserBuffer.Select(uuid => uuid.Id).ToList();

                var ownership = cards.Select(card => allCardsOfUser.Contains(card)).ToList();
                if (ownership.Any(c => c == false))
                {
                    return Task.Run(() => new HttpResponse(HttpStatusCode.Unauthorized, "Unauthorized"));
                }

                dataHandler.ResetCards(_user!);
                dataHandler.SetDeck(cards);

                return Task.Run(() =>
                    new HttpResponse(HttpStatusCode.ActionSuccess, "Deck successfully configured"));

            }
            catch (NpgsqlException)
            {
                return Task.Run(() => new HttpResponse(HttpStatusCode.BadRequest, "Something went wrong"));
            }
        }

        public Task<HttpResponse> ConfigureUser()
        {

            if (_user!.Username != _httpRequestDictionary["addendumPath"])
            {
                return Task.Run(() => new HttpResponse(HttpStatusCode.Unauthorized, "Unauthorized"));
            }

            var dataHandler = new DataHandler();

            try
            {
                var userInfo = JsonConvert.DeserializeObject<UserInfo>(_httpRequestDictionary["Data"]);

                _user.Name = userInfo!.Name;
                _user.Bio = userInfo.Bio;
                _user.Image = userInfo.Image;

                dataHandler.UpdateUser(_user);

                return Task.Run(() => new HttpResponse(HttpStatusCode.ActionSuccess, "User successfully configured"));
            }
            catch (Exception)
            {
                return Task.Run(() => new HttpResponse(HttpStatusCode.BadRequest, "Something went wrong"));
            }
        }
        public Task<HttpResponse> GetUserBio()
        {
            if (_user!.Username != _httpRequestDictionary["addendumPath"])
            {
                return Task.Run(() => new HttpResponse(HttpStatusCode.Unauthorized, "Unauthorized"));
            }
            var dataHandler = new DataHandler();
            try
            {
                var user = dataHandler.GetUserBy("username", _user!.Username);
                var jsonUser = JsonConvert.SerializeObject(user);
                return Task.Run(() => new HttpResponse(HttpStatusCode.OK, jsonUser));
            }
            catch (NpgsqlException)
            {
                return Task.Run(() => new HttpResponse(HttpStatusCode.BadRequest, "Something went wrong"));
            }

        }

        public Task<HttpResponse> CheckStats()
        {
            try
            {
                var stats = new Score(_user!);
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
}
