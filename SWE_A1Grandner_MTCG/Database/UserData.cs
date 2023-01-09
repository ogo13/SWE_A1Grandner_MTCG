namespace SWE_A1Grandner_MTCG.Database
{
    public class UserData
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string? Name { get; set; }
        public string? Bio { get; set; }
        public string? Image { get; set; }
        public int Coins { get; set; }

        
        public UserData(string username, string password, string? name, string? bio, string? image, int coins)
        {
            Username = username;
            Password = password;
            Name = name;
            Bio = bio;
            Image = image;
            Coins = coins;
        }
        
    }
}
