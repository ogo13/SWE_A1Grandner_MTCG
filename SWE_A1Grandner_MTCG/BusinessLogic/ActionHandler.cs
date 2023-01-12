using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
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
    internal static class ActionHandler
    {
        public static bool Register(UserData? userData)
        {
            if (userData == null)
            {
                throw new ArgumentNullException(nameof(userData), "Userdata was null.");
            }
            //get users token from DB
            var dataHandler = new DataHandler();

            return dataHandler.InsertUser(userData);
        }
        public static UserData Login(UserData? userData)
        {
            if (userData == null)
            {
                throw new ArgumentNullException(nameof(userData), "Userdata was null.");
            }

            //get users token from DB
            var dataHandler = new DataHandler();
            var dbData = dataHandler.GetUserBy("username", userData.Username);

            if (dbData == null)
            {
                throw new UserDoesNotExistsException();
            }

            if (userData.Password != dbData.Password)
            {
                throw new ValidationException("Password was wrong.");
            }

            return dbData;
        }


        public static bool CreatePackage(List<CardData>? packageData)
        {
            if (packageData == null)
            {
                throw new ArgumentNullException(nameof(packageData), "packageData was null.");
            }

            if (packageData.Count != 5)
            {
                throw new InvalidOperationException("Not the right amount of cards");
            }

            var dataHandler = new DataHandler();
            var cardsIds = new List<Guid>();

            var success = true;



            //get users token from DB


            foreach (var cardData in packageData)
            {
                success = success && dataHandler.InsertCard(cardData);
                cardsIds.Add(cardData.Id);
            }

            return success && dataHandler.InsertPackage(cardsIds);
        }
        public static List<CardData> BuyPackage(string token)
        {
            var dataHandler = new DataHandler();
            UserData? user;

            try
            {
                user = dataHandler.GetUserBy("token", token.Split(" ")[1]);
            }
            catch (ArgumentNullException e)
            {
                throw new UserDoesNotExistsException();
            }

            if (user!.Coins < 5)
            {
                throw new NotEnoughFundsException();
            }

            var pack = dataHandler.GetPackage();
            user.Coins -= 5;
            dataHandler.UpdateUser(user);
            dataHandler.DeletePackage();
            dataHandler.UpdateOwnerInCards(pack, user.Username);

            return dataHandler.GetCards(pack);

        }


        public static List<CardData> ShowAllCards(UserData user)
        {
            var dataHandler = new DataHandler();

            var stack = dataHandler.GetAllCards(user);
            
            return stack;
        }
        public static List<CardData> ShowDeck(UserData user)
        {
            var dataHandler = new DataHandler();

            var deck = dataHandler.GetDeck(user);

            return deck;
        }
        public static string ShowFancyDeck(List<CardData> cards)
        {
            var names = new List<string>();
            var damages = new List<string>();

            if (cards.Count == 0)
            {
                return string.Empty;
            }

            for (var index = 0; index < cards.Count; index++)
            {
                names.Add(cards[index].Name);
                var toAdd = 12 - names[index].Length;
                for (int i = 0; i < toAdd; i++)
                {
                    names[index] += " ";
                }
                damages.Add(cards[index].Damage.ToString());
                var toAdd2 = 10 - damages[index].Length;
                damages[index] += ".0";
                for (int i = 0; i < toAdd2; i++)
                {
                    damages[index] += " ";
                }
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
            stringBuilder.AppendFormat("│{0,10}││{1,10}││{2,10}││{3,10}│", names[0], names[1], names[2], names[3]);
            stringBuilder.AppendLine();
            stringBuilder.AppendFormat("│{0,10}││{1,10}││{2,10}││{3,10}│",  damages[0], damages[1], damages[2], damages[3]);
            stringBuilder.AppendLine();
            stringBuilder.AppendLine($"└────────────┘└────────────┘└────────────┘└────────────┘");

            return stringBuilder.ToString();
        }


        public static bool ConfigureDeck(UserData user, List<Guid> cards)
        {
            if (cards == null)
            {
                throw new ArgumentNullException(nameof(cards), "Cards were null.");
            }
            if (cards.Count != 4)
            {
                throw new InvalidOperationException();
            }

            var dataHandler = new DataHandler();


            //check if cards belong to user
            var allCardsOfUserBuffer = dataHandler.GetAllCards(user);
            var allCardsOfUser = new List<Guid>();
            foreach (var uuid in allCardsOfUserBuffer)
            {
                allCardsOfUser.Add(uuid.Id);
            }

            var ownership = cards.Select(card => allCardsOfUser.Contains(card)).ToList();
            if (ownership.Any(c => c == false))
            {
                throw new UnauthorizedAccessException();
            }


            dataHandler.ResetCards(user);

            dataHandler.SetDeck(cards);

            return true;
        }


        public static bool ConfigureUser(UserData user, UserInfo userInfo)
        {
            var dataHandler = new DataHandler();

            user.Name = userInfo.Name;
            user.Bio = userInfo.Bio;
            user.Image = userInfo.Image;

            dataHandler.UpdateUser(user);

            return true;
        }
        public static UserData GetUserBio(UserData user)
        {
            
            var dataHandler = new DataHandler();

            return dataHandler.GetUserBy("username", user.Username)!;

        }

        public static Score CheckStats(UserData user)
        {
            return new Score(user);
        }

        public static Task<HttpResponse> CheckTrades()
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

        public static Task<HttpResponse> PostTrade(string data, UserData user)
        {
            try
            {
                var dataHandler = new DataHandler();
                var trade = JsonConvert.DeserializeObject<TradeData>(data);
                if (trade == null)
                {
                    return Task.Run(() => new HttpResponse(HttpStatusCode.BadRequest, "Something went wrong."));
                }

                trade.Owner = user.Username;

                dataHandler.InsertTrade(trade);
                return Task.Run(() => new HttpResponse(HttpStatusCode.OK, "Trade successfully created"));
            }
            catch (NpgsqlException)
            {
                return Task.Run(() => new HttpResponse(HttpStatusCode.BadRequest, "Something went wrong."));
            }
        }

    }
}
