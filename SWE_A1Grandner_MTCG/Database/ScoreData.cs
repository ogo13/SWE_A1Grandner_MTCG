namespace SWE_A1Grandner_MTCG.Database;

public class ScoreData
{
    public string Username { get; set; }
    public int Wins { get; set; }
    public int Draws { get; set; }
    public int Losses { get; set; }
    public int Elo { get; set; }

    public ScoreData(string username, int wins, int draws, int losses, int elo)
    {
        Username = username;
        Wins = wins;
        Draws = draws;
        Losses = losses;
        Elo = elo;
    }
} 

    

