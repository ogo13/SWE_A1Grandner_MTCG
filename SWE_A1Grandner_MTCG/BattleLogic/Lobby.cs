using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using SWE_A1Grandner_MTCG.Database;

namespace SWE_A1Grandner_MTCG.BattleLogic
{
    internal class Lobby
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
            var battle = new Battle(player1, player2);
            BattleLogs.Add(battle.Fight());
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
            }
        }
    }
}
