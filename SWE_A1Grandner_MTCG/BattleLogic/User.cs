namespace SWE_A1Grandner_MTCG.BattleLogic;

public class User
{
    public string Username { get; set; }
    public string Password { get; set; }
    public bool IsAdmin { get; private set; }

    public User(string authorizationToken)
    {
        IsAdmin = true;
        Username = "admin";
        Password = "password";
        //db get user from auth token
    }

}