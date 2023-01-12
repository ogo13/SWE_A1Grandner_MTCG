// See https://aka.ms/new-console-template for more information

using Npgsql;
using SWE_A1Grandner_MTCG.BusinessLogic;
using System.Data.Common;
using SWE_A1Grandner_MTCG.BattleLogic;
using SWE_A1Grandner_MTCG.Database;
using SWE_A1Grandner_MTCG.MyEnum;



var server = new Server(10001);

var lol = server.Start();

await lol;
