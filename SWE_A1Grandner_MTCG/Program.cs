// See https://aka.ms/new-console-template for more information

using Npgsql;
using SWE_A1Grandner_MTCG;
using System.Data.Common;

Console.WriteLine("Hello, World!");



Server server = new Server(10001);

await server.Start();



