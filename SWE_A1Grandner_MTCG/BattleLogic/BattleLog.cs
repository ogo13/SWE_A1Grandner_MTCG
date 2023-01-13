using SWE_A1Grandner_MTCG.MyEnum;
using System.Text;
using SWE_A1Grandner_MTCG.Database;

namespace SWE_A1Grandner_MTCG.BattleLogic;

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

    public override string ToString()
    {
        var retStr = Log.ToString();
        if (Outcome == BattleOutcome.Draw)
        {
            retStr = $"{retStr}No winner: Draw{Environment.NewLine}";
        }
        else
        {
            retStr = $"{retStr}Winner: {Players[(int)Outcome!].Username}";
        }

        return retStr;
    }
}

