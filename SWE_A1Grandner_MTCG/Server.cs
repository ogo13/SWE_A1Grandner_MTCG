using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SWE_A1Grandner_MTCG;
using JsonConverter = System.Text.Json.Serialization.JsonConverter;

namespace SWE_A1Grandner_MTCG
{
    internal class Server
    {
        public TcpListener Listener { get; set; }

        public Server(int port)
        {
            // Create a new TCP listener on localhost and the specified port
            Listener = new TcpListener(IPAddress.Loopback, port);
        }

        public async Task Start()
        {
            // Start listening for incoming connections
            Listener.Start();

            // Accept incoming connections asynchronously
            while (true)
            {
                Console.WriteLine("Waiting for connection!");
                // Wait for an incoming connection
                var client = await Listener.AcceptTcpClientAsync();

                // Handle the incoming request in a separate task
                Task.Run(() => HandleRequest(client));
            }
        }

        private async void HandleRequest(TcpClient client)
        {
            // Get the client's stream
            var stream = client.GetStream();

            // Read the request data from the stream
            var buffer = new byte[1024];
            var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
            var requestData = Encoding.UTF8.GetString(buffer, 0, bytesRead);

            // Parse the request data
            var requestLines = requestData.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            var firstRequestLine = requestLines[0].Split(" ");
            var method = firstRequestLine[0];
            var path = firstRequestLine[1];

            if (method == "GET")
            {
                await HandleGetRequest(client, path, requestLines);
            }
            else if (method == "POST")
            {
                await HandlePostRequest(client, path, requestLines);
            }
            else if (method == "PUT")
            {
                
            }

            // Handle the request based on the request method and path
            if (method == "GET" && path == "/")
            {
                // If the request method is GET and the path is /, return the homepage
                var responseBody = "<h1>Welcome to the Card Game Server</h1>";
                var responseData = Encoding.UTF8.GetBytes(responseBody);

                // Write the response data to the stream
                await stream.WriteAsync(responseData, 0, responseData.Length);
            }
        }

        private async Task HandleGetRequest(TcpClient client, string path, string[] requestLines)
        {

        }

        private async Task HandlePostRequest(TcpClient client, string path, string[] requestLines)
        {

            switch (path)
            {
                case "/users":
                {
                    UserData? userData = JsonConvert.DeserializeObject<UserData>(requestLines.Last());
                    //create user on database

                    break;
                }
                case "/sessions":
                {
                    UserData? userData = JsonConvert.DeserializeObject<UserData>(requestLines.Last());
                    //login user from database

                    break;
                }
                case "/packages":
                {
                    //check Authorization


                    List<CardData>? cards = JsonConvert.DeserializeObject<List<CardData>>(requestLines.Last());
                    //login user on database
                    break;
                }
                case "/tradings":
                {
                    //check Authorization


                    TradeData? trade = JsonConvert.DeserializeObject<TradeData>(requestLines.Last());
                    break;
                }

            }


            foreach (var line in requestLines)
            {
                    Console.WriteLine(line);
            }


        }
    }
}
