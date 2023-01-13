using SWE_A1Grandner_MTCG.Database;
using Npgsql;
using SWE_A1Grandner_MTCG.MyEnum;

namespace SWE_A1Grandner_MTCG.BusinessLogic;

internal class DeleteActionHandler : IActionHandler
{
    private readonly Dictionary<string, string> _httpRequestDictionary;
    private readonly UserData _user;
    private readonly IDataHandler _dataHandler;

    public DeleteActionHandler(Dictionary<string, string> httpRequestDictionary, UserData user, IDataHandler dataHandler)
    {
        _httpRequestDictionary = httpRequestDictionary;
        _user = user;
        this._dataHandler = dataHandler;
    }

    public Task<HttpResponse> DeleteTrade()
    {
        try
        {
            var tradeId = Guid.Parse(_httpRequestDictionary["addendumPath"]);
            var trade = _dataHandler.GetTradeById(tradeId);
            if (trade.Owner != _user.Username)
            {
                return Task.Run(() => new HttpResponse(HttpStatusCode.Unauthorized, "Unauthorized"));
            }

            return _dataHandler.DeleteTrade(tradeId)
                ? Task.Run(() => new HttpResponse(HttpStatusCode.ActionSuccess, "Trade successfully deleted."))
                : Task.Run(() => new HttpResponse(HttpStatusCode.BadRequest, "Something went wrong."));
        }
        catch (NpgsqlException)
        {
            return Task.Run(() => new HttpResponse(HttpStatusCode.BadRequest, "Something went wrong."));
        }
        catch (ArgumentNullException)
        {
            return Task.Run(() => new HttpResponse(HttpStatusCode.NotFound, "No such Trade exists."));
        }
    }
}