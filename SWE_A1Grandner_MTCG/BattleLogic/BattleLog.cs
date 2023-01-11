using SWE_A1Grandner_MTCG.MyEnum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWE_A1Grandner_MTCG.BattleLogic
{
    public class BattleLog
    {
        public BattleOutcome Outcome { get; set; }
        public string Log { get; set; }

        public BattleLog(BattleOutcome outcome, string log)
        {
            Outcome = outcome;
            Log = log;
        }
        
    }
}
