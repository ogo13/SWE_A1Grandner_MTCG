using SWE_A1Grandner_MTCG.MyEnum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SWE_A1Grandner_MTCG.Database;

namespace SWE_A1Grandner_MTCG.BattleLogic
{
    public class BattleLog
    {
        public BattleOutcome? Outcome { get; set; }
        public StringBuilder Log { get; set; }
        public int Rounds { get; set; }
        public List<UserData> Players { get; set; }
        public int HasBeenReturned { get; set; }

        public BattleLog()
        {
            Log = new StringBuilder();
            Rounds = 0;
            Players = new List<UserData>();
            HasBeenReturned = 0;
        }
        public BattleLog(BattleOutcome outcome)
        {
            Outcome = outcome;
            Log = new StringBuilder();
            Rounds = 0;
            Players = new List<UserData>();
            HasBeenReturned = 0;
        }

        public override string ToString()
        {
            if (Outcome == BattleOutcome.Draw)
            {
                Log.AppendLine("Winner: Draw");
            }
            else
            {
                Log.AppendLine($"Winner: {Players[(int)Outcome!].Username}");
            }

            return Log.ToString();
        }
    }
}
