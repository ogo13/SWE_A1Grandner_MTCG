using System.Text;

namespace SWE_A1Grandner_MTCG.Database;

public class ScoreBoard
{
    public List<ScoreData> ScoreList { get; set; }

    public ScoreBoard(DataHandler dataHandler)
    {
        var bufferScoreList = dataHandler.GetScoreBoard();
        ScoreList = bufferScoreList.OrderByDescending(o => o.Elo).ToList();
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.AppendFormat("{0,10}|{1,10}|{2,10}|{3,10}|{4,10}|{5,10}", "username", "wins", "draws", "losses", "elo", "games");
        sb.AppendLine();
        sb.AppendLine("-----------------------------------------------------------------");
        int counter = 0;
        foreach (var score in ScoreList)
        {
            counter++;
            if (counter > 25)
                break;
            sb.AppendFormat("{0,10}|{1,10}|{2,10}|{3,10}|{4,10}|{5,10}", score.Username, score.Wins.ToString(),
                score.Draws.ToString(), score.Losses.ToString(), score.Elo.ToString(),
                (score.Wins + score.Draws + score.Losses).ToString());
            sb.AppendLine();
        }

        return sb.ToString();
    }
}

