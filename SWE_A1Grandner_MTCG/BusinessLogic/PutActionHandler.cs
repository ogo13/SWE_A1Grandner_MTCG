using Newtonsoft.Json;
using Npgsql;
using SWE_A1Grandner_MTCG.Database;
using SWE_A1Grandner_MTCG.MyEnum;

namespace SWE_A1Grandner_MTCG.BusinessLogic;

public class PutActionHandler : IActionHandler
{
    private readonly Dictionary<string, string> _httpRequestDictionary;
    private readonly UserData? _user;

    public PutActionHandler(Dictionary<string, string> httpRequestDictionary, UserData? user)
    {
        _httpRequestDictionary = httpRequestDictionary;
        _user = user;
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
        if (!_httpRequestDictionary.ContainsKey("addendumPath"))
        {
            return Task.Run(() => new HttpResponse(HttpStatusCode.BadRequest, "Something went wrong."));
        }

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
}

