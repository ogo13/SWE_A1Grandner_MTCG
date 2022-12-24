
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Sockets;
using System.Text;
using SWE_A1Grandner_MTCG.Databank;
using SWE_A1Grandner_MTCG.BattleLogic;
using HttpStatusCode = SWE_A1Grandner_MTCG.Enum.HttpStatusCode;

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
                await Task.Run(() => ReceiveRequest(client));
            }
        }

        private async void ReceiveRequest(TcpClient client)
        {
            // Get the client's stream
            var stream = client.GetStream();

            // Read the request data from the stream
            var buffer = new byte[client.ReceiveBufferSize];
            var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
            var requestData = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            // Parse the request data
            requestData = requestData.Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", Environment.NewLine);
            var requestDataSplit = requestData.Split(new string[] { Environment.NewLine + Environment.NewLine }, StringSplitOptions.None);
            var requestHeader = requestDataSplit[0].Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            var firstHeaderLine = requestHeader[0].Split(" ");
            var method = firstHeaderLine[0];
            var path = firstHeaderLine[1];
            Dictionary<string, string> dataDictionary = new Dictionary<string, string>();

            dataDictionary.Add("Method", method);
            dataDictionary.Add("Path", path);
            dataDictionary.Add("Data", requestDataSplit[1]);

            for (int i = 1; i < requestHeader.Length; i++)
            {
                var pairs = requestHeader[i].Split(": ");
                dataDictionary.Add(pairs[0], pairs[1]);
            }


            Console.WriteLine(requestData);

            RequestHandler requestHandler = new RequestHandler(dataDictionary, client);

            await requestHandler.HandleRequest();

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
