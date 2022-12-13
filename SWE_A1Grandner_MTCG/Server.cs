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
            var buffer = new byte[client.ReceiveBufferSize];
            var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
            var requestData = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            // Parse the request data
            var requestDataSplit = requestData.Split(new string[] { Environment.NewLine + Environment.NewLine }, StringSplitOptions.None);
            var requestHeader = requestDataSplit[0].Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            var firstHeaderLine = requestHeader[0].Split(" ");
            var method = firstHeaderLine[0];
            var path = firstHeaderLine[1];
            Dictionary<string, string> requestHeaderDictionary = new Dictionary<string, string>(); ;

            for (int i = 1; i < requestHeader.Length; i++)
            {
                var pairs = requestHeader[i].Split(": ");
                requestHeaderDictionary.Add(pairs[0], pairs[1]);
            }

            
            Console.WriteLine(requestData);
            

            if (method == "GET")
            {
                await HandleGetRequest(client, path, requestHeaderDictionary, requestDataSplit[1]);
            }
            else if (method == "POST")
            {
                await HandlePostRequest(client, path, requestHeaderDictionary, requestDataSplit[1]);
            }
            else if (method == "PUT")
            {
                await HandlePutRequest(client, path, requestHeaderDictionary, requestDataSplit[1]);
            }

        }

        private async Task HandleGetRequest(TcpClient client, string path, Dictionary<string, string> requestHeaderDictionary, string data)
        {

            //check Authorization exception
            User user;
            try
            {
                user = CheckAuthorization(requestHeaderDictionary["Authorization"]);
            }
            catch
            {

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

        private async Task HandlePostRequest(TcpClient client, string path, Dictionary<string, string> requestHeaderDictionary, string data)
        {

            if(path == "/users")
            {
                var userData = JsonConvert.DeserializeObject<UserData>(data);
                //create user on database
                

            }
            else if(path == "/sessions")
            {
                UserData? userData = JsonConvert.DeserializeObject<UserData>(data);
                //login user from database
                try
                {
                    Login(userData);
                }
                catch
                {

                }
            }
            
            //check Authorization
            try
            {
                User user = CheckAuthorization(requestHeaderDictionary["Authorization"]);
            }
            catch
            {
                throw;
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
                    List<CardData>? cards = JsonConvert.DeserializeObject<List<CardData>>(data);
                    break;
                }
                case "/tradings":
                {

                    TradeData? trade = JsonConvert.DeserializeObject<TradeData>(data);

                    break;
                }
            }
        }

        private async Task HandlePutRequest(TcpClient client, string path, Dictionary<string, string> requestHeaderDictionary, string data)
        {
            //check Authorization
            User user;
            try
            {
                user = CheckAuthorization(requestHeaderDictionary["Authorization"]);
            }
            catch
            {
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
        
        private User CheckAuthorization(string? authToken) //dictionary mitgeben
        {
            //get user from DB
            if (authToken == null)
            {
                throw new ArgumentNullException(nameof(authToken)); //todo no token exception
            }
            try
            {
                return new User(authToken);
            }
            catch
            {
                throw new InvalidOperationException(); //todo not authenticated exception 
            }
        }

        private string Login(UserData? userData)
        {
            if (userData == null)
            {
                throw new Exception();
            }
            //get users token from DB
            string authotizationToken = "admin-mtcgToken";

            return authotizationToken;
        }
    }
}
