using Newtonsoft.Json;
using Npgsql;
using SWE_A1Grandner_MTCG.Database;
using SWE_A1Grandner_MTCG.MyEnum;

namespace SWE_A1Grandner_MTCG.BusinessLogic;

public class PutActionHandler : IActionHandler
{
    private readonly Dictionary<string, string> _httpRequestDictionary;
    private readonly UserData? _user;
    private readonly IDataHandler _dataHandler;

    public PutActionHandler(Dictionary<string, string> httpRequestDictionary, UserData? user, IDataHandler dataHandler)
    {
        _httpRequestDictionary = httpRequestDictionary;
        _user = user;
        _dataHandler = dataHandler;
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


            //check if cards belong to user
            var allCardsOfUserBuffer = _dataHandler.GetAllCards(_user!);
            var allCardsOfUser = allCardsOfUserBuffer.Select(uuid => uuid.Id).ToList();

            var ownership = cards.Select(card => allCardsOfUser.Contains(card)).ToList();
            if (ownership.Any(c => c == false))
            {
                return Task.Run(() => new HttpResponse(HttpStatusCode.Unauthorized, "Unauthorized"));
            }

            _dataHandler.ResetCards(_user!);
            _dataHandler.SetDeck(cards);

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

        try
        {
            var userInfo = JsonConvert.DeserializeObject<UserInfo>(_httpRequestDictionary["Data"]);

            _user.Name = userInfo!.Name;
            _user.Bio = userInfo.Bio;
            _user.Image = userInfo.Image;

            _dataHandler.UpdateUser(_user);

            return Task.Run(() => new HttpResponse(HttpStatusCode.ActionSuccess, "User successfully configured"));
        }
        catch (Exception)
        {
            return Task.Run(() => new HttpResponse(HttpStatusCode.BadRequest, "Something went wrong"));
        }
    }
}

