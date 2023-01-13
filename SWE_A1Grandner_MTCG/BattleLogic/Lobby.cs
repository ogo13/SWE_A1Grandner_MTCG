using System.Collections.Concurrent;
using SWE_A1Grandner_MTCG.Database;

namespace SWE_A1Grandner_MTCG.BattleLogic;

public class Lobby
{
    private ConcurrentQueue<UserData> PlayerLobby { get; set; }
    public List<BattleLog> BattleLogs { get; set; }

    public Lobby()
    {
        PlayerLobby = new ConcurrentQueue<UserData>();
        BattleLogs = new List<BattleLog>();
    }
    public void AddPlayer(UserData player)
    {
        var dataHandler = new DataHandler();
        PlayerLobby.Enqueue(player);
        if (PlayerLobby.Count < 2)
        {
            return;
        }

        UserData? player1 = null;
        UserData? player2 = null;
        while (player1 == null || player2 == null)
        {
            if(player1 == null)
                PlayerLobby.TryDequeue(out player1);
            if(player2 == null)
                PlayerLobby.TryDequeue(out player2);
        }
        var battle = new Battle(player1, player2, new Deck(dataHandler.GetDeck(player1)), new Deck(dataHandler.GetDeck(player2)));
        var battleResult = battle.Fight();
        var player1Score = new Score(player1);
        var player2Score = new Score(player2);
        player1Score.AddResult(player2Score, battleResult.Outcome);

        BattleLogs.Add(battleResult);
    }

    public BattleLog GetResult(UserData player)
    {
        while (true)
        {
            foreach (var log in BattleLogs.ToList().Where(log => log.Players.Contains(player)))
            {
                log.HasBeenReturned++;
                if (log.HasBeenReturned == 2)
                {
                    BattleLogs.Remove(log);
                }
                return log;
            }
            Thread.Sleep(500);
        }
    }
}

