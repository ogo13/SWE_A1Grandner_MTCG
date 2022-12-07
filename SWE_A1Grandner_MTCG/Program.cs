// See https://aka.ms/new-console-template for more information

using SWE_A1Grandner_MTCG;

Console.WriteLine("Hello, World!");

Server server = new Server(10001);

await server.Start();