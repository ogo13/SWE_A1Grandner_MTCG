using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;
using SWE_A1Grandner_MTCG.Database;
using SWE_A1Grandner_MTCG.MyEnum;
using Math = System.Math;

namespace SWE_A1Grandner_MTCG.BattleLogic
{
    public class Score
    {
        public UserData Player { get; set; }
        public int Wins { get; set; }
        public int Draws { get; set; }
        public int Losses { get; set; }
        public int Elo { get; set; }

        public Score(UserData player)
        {
            Player = player;
            var dh = new DataHandler();
            var score = dh.GetScore(player);
            Wins = score.Wins;
            Draws = score.Draws;
            Losses = score.Losses;
            Elo = score.Elo;
        }

        private double ExpectedValue(Score opponent)
        {
            return 1 / (1 + Math.Pow(10, (opponent.Elo - Elo) / 400.0));
        }

        public void AddResult(Score opponent, BattleOutcome? outcome)
        {
            switch (outcome)
            {
                case BattleOutcome.Draw:
                    AddDraw(opponent);
                    break;
                case BattleOutcome.Player1Win:
                    AddWin(opponent);
                    break;
                case BattleOutcome.Player2Win:
                    AddLose(opponent);
                    break;
                case null:
                default:
                    throw new ArgumentOutOfRangeException(nameof(outcome), outcome, null);
            }

            var dh = new DataHandler();
            dh.UpdateScore(this);
            dh.UpdateScore(opponent);
        }

        public void AddDraw(Score opponent)
        {
            var e = ExpectedValue(opponent);
            var eloDouble = Elo + 20 * (0.5 - e);
            Elo = (int)Math.Floor(eloDouble);
            var opoEloDouble = Elo + 20 * (0.5 - (1 - e));
            opponent.Elo = (int)Math.Round(opoEloDouble);
            Draws++;
            opponent.Draws++;
        }
        public void AddWin(Score opponent)
        {
            var e = ExpectedValue(opponent);
            var eloDouble = Elo + 20 * (1 - e);
            Elo = (int)Math.Floor(eloDouble);
            var opoEloDouble = opponent.Elo + 20 * (0 - (1 - e));
            opponent.Elo = (int)Math.Round(opoEloDouble);
            Wins++;
            opponent.Losses++;
        }
        public void AddLose(Score opponent)
        {
            opponent.AddWin(this);
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendFormat("{0,10}|{1,10}|{2,10}|{3,10}|{4,10}|{5,10}", "username", "wins", "draws", "losses", "elo", "games");
            sb.AppendLine();
            sb.AppendLine("-----------------------------------------------------------------");
            sb.AppendFormat("{0,10}|{1,10}|{2,10}|{3,10}|{4,10}|{5,10}", Player.Username, Wins.ToString(), Draws.ToString(), Losses.ToString(), Elo.ToString(), (Wins+Draws+Losses).ToString());
            sb.AppendLine();
            return sb.ToString();
        }
    }
}
