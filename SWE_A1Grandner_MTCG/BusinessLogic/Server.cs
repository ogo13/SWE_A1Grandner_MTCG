using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Newtonsoft.Json;
using JsonConverter = System.Text.Json.Serialization.JsonConverter;
using System.IO;
using Npgsql;
using SWE_A1Grandner_MTCG.Databank;
using SWE_A1Grandner_MTCG.Enum;
using SWE_A1Grandner_MTCG.BattleLogic;

namespace SWE_A1Grandner_MTCG.BusinessLogic
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

        private async Task<int> HandlePostRequest(TcpClient client, string path, Dictionary<string, string> requestHeaderDictionary, string data)
        {

            if (path == "/users")
            {
                var userData = JsonConvert.DeserializeObject<UserData>(data);
                //create user on database
                Task<int> registerTask;
                try
                {
                    registerTask = Register(userData);
                    await registerTask;
                    return 0;
                }
                catch (NpgsqlException e)
                {
                    Console.WriteLine(e.Message);
                }
                catch (ArgumentException e)
                {
                    Console.WriteLine(e.Message);
                }
                finally
                {

                }

            }
            else if (path == "/sessions")
            {
                UserData? userData = JsonConvert.DeserializeObject<UserData>(data);
                Task<string> loginTask;
                //login user from database
                try
                {
                    loginTask = Login(userData);
                    var res = await HttpResponse(await loginTask, HttpStatusCode.Success);
                    var stream = client.GetStream();
                    await stream.WriteAsync(Encoding.UTF8.GetBytes(res), 0, Encoding.UTF8.GetBytes(res).Length);
                    return 0;

                }
                catch (ArgumentNullException e)
                {
                    Console.WriteLine(e.ParamName + " " + e.Message);
                    var res = await HttpResponse("wrong Credentials", HttpStatusCode.BadRequest);
                    var stream = client.GetStream();
                    await stream.WriteAsync(Encoding.UTF8.GetBytes(res), 0, Encoding.UTF8.GetBytes(res).Length);
                    return 1;
                }
                catch (ValidationException e)
                {
                    Console.WriteLine(e.Message);
                    var res = await HttpResponse("wrong Credentials", HttpStatusCode.BadRequest);
                    var stream = client.GetStream();
                    await stream.WriteAsync(Encoding.UTF8.GetBytes(res), 0, Encoding.UTF8.GetBytes(res).Length);
                    return 1;
                }
                finally
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
                            return 1;
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

            return 1;
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

        private async Task<string> HttpResponse(string message, HttpStatusCode statusCode)
        {
            var response = "HTTP/1.1 ";

            switch (statusCode)
            {
                case HttpStatusCode.Success:
                    response += "200 OK";
                    break;
                case HttpStatusCode.BadRequest:
                    response += "400 Bad Request"
                                + "Connection: Closed" + Environment.NewLine
                                + "Content-Length: 0" + Environment.NewLine + Environment.NewLine;
                    return response;
                case HttpStatusCode.Unauthorized:
                    response += "401 Unauthorized"
                                + "Connection: Closed" + Environment.NewLine
                                + "Content-Length: 0" + Environment.NewLine + Environment.NewLine;
                    return response;
                case HttpStatusCode.NotFound:
                    response += "404 Not Found"
                                + "Connection: Closed" + Environment.NewLine
                                + "Content-Length: 0" + Environment.NewLine + Environment.NewLine;
                    return response;
            }

            response += Environment.NewLine
                        + "Connection: Closed" + Environment.NewLine
                        + "Content-Type: text/plain" + Environment.NewLine
                        + "Content-Length: " + message.Length + Environment.NewLine + Environment.NewLine
                        + message;

            return response;
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

        private async Task<string> Login(UserData? userData)
        {
            if (userData == null)
            {
                throw new ArgumentNullException(nameof(userData), "Userdata was null.");
            }
            //get users token from DB
            DataHandler dataHandler = new DataHandler();
            var dbData = await dataHandler.GetUserBy("username", userData.Username);
            if (dbData == null)
            {
                throw new ArgumentNullException(nameof(dbData), "Userdata was null.");
            }

            if (string.CompareOrdinal(userData.Password, dbData.Rows[0]["password"].ToString()) != 0)
            {
                throw new ValidationException("Password was wrong.");
            }

            string authorizationToken = dbData.Rows[0]["token"].ToString() ?? string.Empty;

            return authorizationToken;
        }

        private async Task<int> Register(UserData? userData)
        {
            if (userData == null)
            {
                throw new ArgumentNullException(nameof(userData), "Userdata was null.");
            }
            //get users token from DB
            DataHandler dataHandler = new DataHandler();
            var dbData = await dataHandler.InsertUser(userData);

            return dbData;
        }
    }
}
