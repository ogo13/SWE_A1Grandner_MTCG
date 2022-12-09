using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
using System.IO;

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

            foreach (var line in requestLines)
            {
                Console.WriteLine(line);
            }

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
                await HandlePutRequest(client, path, requestLines);
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

            //check Authorization
            User? user = CheckAuthorization(requestLines);
            if (user == null)
            {

                return;
            }

            switch (path)
            {
                case "/cards":
                {

                    break;
                }
                case "/deck":
                {

                    break;
                }
                case "/users": //noch ned ganz fertig
                {


                    //edit user data
                    break;
                }
                case "/stats":
                {


                    //check stats


                    break;
                }
                case "/score":
                {


                    //check score   


                    break;
                }
                case "/tradings":
                {


                    //check trades   


                    break;
                }

            }
        }

        private async Task HandlePostRequest(TcpClient client, string path, string[] requestLines)
        {

            if(path == "/users")
            {
                UserData? userData = JsonConvert.DeserializeObject<UserData>(requestLines.Last());
                //create user on database

            }
            else if(path == "/sessions")
            {
                UserData? userData = JsonConvert.DeserializeObject<UserData>(requestLines.Last());
                //login user from database
                Login(userData);
            }
            
            //vlt User user
            //check Authorization
            User? user = CheckAuthorization(requestLines);
            if (user == null)
            {
                return;
            }


            switch (path)
            {
                case "/packages":
                {
                    if (false) //vlt if(!user.isAdmin())
                    {
                        //not admin
                        return;
                    }

                    List<CardData>? cards = JsonConvert.DeserializeObject<List<CardData>>(requestLines.Last());

                    break;
                }
                case "/tradings":
                {

                    TradeData? trade = JsonConvert.DeserializeObject<TradeData>(requestLines.Last());

                    break;
                }

            }


            foreach (var line in requestLines)
            {
                    Console.WriteLine(line);
            }


        }

        private async Task HandlePutRequest(TcpClient client, string path, string[] requestLines)
        {
            //check Authorization
            User? user = CheckAuthorization(requestLines);
            if (user == null)
            {
                return;
            }

            switch (path)
            {
                case "/users": //fehlt noch etwas
                {
                    //configure user

                    break;
                }
                case "/deck":
                {
                    //configure deck

                    break;
                }
            }
        }

        [SuppressMessage("ReSharper", "StringCompareIsCultureSpecific.1")]
        private User? CheckAuthorization(string[] requestLines)
        {
            var fifthRequestLine = requestLines[4].Split(" ");
            var sixthRequestLine = requestLines[5].Split(" ");

            if (string.Compare(fifthRequestLine[0], "Authorization:")==0)
            {
                return new User(fifthRequestLine[2]);
            }
            else if(string.Compare(sixthRequestLine[0], "Authorization:") == 0)
            {
                return new User(sixthRequestLine[2]);
            }
            else
            {
                return null;
            }
        }

        private string? Login(UserData? userData)
        {
            if (userData == null)
            {
                return null;
            }
            //get users token from DB
            string authotizationToken = "admin-mtcgToken";

            return authotizationToken;
        }
    }
}
