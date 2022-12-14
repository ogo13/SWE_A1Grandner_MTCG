// See https://aka.ms/new-console-template for more information

using Npgsql;
using SWE_A1Grandner_MTCG;
using System.Data.Common;

async Task<string> Login(UserData? userData)
{
    if (userData == null)
    {
        throw new Exception();
    }
    //get users token from DB
    DataHandler dataHandler = new DataHandler();
    var dbData = await dataHandler.GetUserBy("username", userData.Username);
    if (dbData == null)
    {
        throw new Exception();
        return String.Empty;
    }

    if (String.CompareOrdinal(userData.Password, dbData.Rows[0]["password"].ToString()) != 0)
    {
        Console.WriteLine("HERE");
        throw new Exception();
    }

    string authorizationToken = dbData.Rows[0]["token"].ToString();

    return authorizationToken;
}

Console.WriteLine("Hello, World!");

DataHandler dh = new DataHandler();
Dictionary<string, string> p = new Dictionary<string, string>
{
    { "username", "felix" },
    { "password", "pw" }
};


var data = await dh.GetUserBy("username", "shahan");
Console.WriteLine(data.Rows.Count);
/*
var u = new UserData
{
    Username = "felix",
    Password = "pw2"
};
try
{
    var login = await Login(u);
    Console.WriteLine(login);
}
catch (Exception e)
{
    Console.WriteLine(e.Message);
}
*/

Server server = new Server(10001);

await server.Start();



