using Npgsql;
using NpgsqlTypes;
using SWE_A1Grandner_MTCG.BattleLogic;
using SWE_A1Grandner_MTCG.Exceptions;
using SWE_A1Grandner_MTCG.MyEnum;
using System.Data;

namespace SWE_A1Grandner_MTCG.Database;

public interface IDataHandler
{
    public bool InsertUser(UserData user);

    public bool InsertCard(CardData cardData);


    public bool InsertPackage(List<Guid> uuids);

    public void InsertScore(UserData user);

    public bool InsertTrade(TradeData tradeData);



    public bool UpdateOwnerInCards(List<Guid> uuids, string user);

    public bool UpdateOwnerInOneCard(Guid uuid, string user);

    public bool UpdateUser(UserData user);

    public bool UpdateScore(Score score);



    public UserData GetUserBy(string method, string parameter);

    public List<Guid> GetPackage();

    public CardData GetCardById(Guid id);
    public List<CardData> GetCards(List<Guid> uuids);

    public List<CardData> GetAllCards(UserData user);

    public List<CardData> GetDeck(UserData user);

    public ScoreData GetScore(UserData user);
    public List<ScoreData> GetScoreBoard();

    public List<TradeData> GetAllTrades();

    public TradeData GetTradeById(Guid id);

    public bool DeletePackage();
    public bool DeleteTrade(Guid trade);



    public bool ResetCards(UserData user);
    public bool SetDeck(List<Guid> uuids);
}