// See https://aka.ms/new-console-template for more information

using Npgsql;
using SWE_A1Grandner_MTCG.BusinessLogic;
using System.Data.Common;
using SWE_A1Grandner_MTCG.BattleLogic;
using SWE_A1Grandner_MTCG.Database;
using SWE_A1Grandner_MTCG.MyEnum;


Console.WriteLine("Hello, World!");

var u = new UserData("kienboec", "", null, null, null, 20);
var us = new UserData("altenhof", "", null, null, null, 20);



var bat = new Battle(u, us);
var log = bat.Fight();
Console.WriteLine(log.Log);

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

var lol = server.Start();

HttpResponse response = new HttpResponse();
response.Status = HttpStatusCode.OK;
response.Content = "{lol}";
response.ToString();

await lol;
