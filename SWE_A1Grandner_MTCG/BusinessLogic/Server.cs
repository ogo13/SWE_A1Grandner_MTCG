
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Npgsql;
using SWE_A1Grandner_MTCG.BattleLogic;
using HttpStatusCode = SWE_A1Grandner_MTCG.MyEnum.HttpStatusCode;

namespace SWE_A1Grandner_MTCG.BusinessLogic;
internal class Server
    {
        public TcpListener Listener { get; set; }
        public List<Task> Tasks { get; set; }
        public Lobby BattleLobby { get; set; }

        public Server(int port)
        {
            // Create a new TCP listener on localhost and the specified port
            Listener = new TcpListener(IPAddress.Loopback, port);
            Tasks = new List<Task>();
            BattleLobby = new Lobby();
        }

        public async Task Start()
        {
            // Start listening for incoming connections
            Listener.Start();

            // Accept incoming connections asynchronously
            while (true)
            {
                if (Tasks.Count != 0)
                {
                    foreach (var task in Tasks.ToList().Where(task => task.IsCompleted))
                    {
                        // Console.WriteLine($"Task no. {task.Id} has completed");
                        await task;
                        Tasks.Remove(task);
                    }
                }
                Console.WriteLine("Waiting for connection!");
                // Wait for an incoming connection
                var client = await Listener.AcceptTcpClientAsync();

                // Handle the incoming request in a separate task
                Tasks.Add(Task.Run(() => HandleConnection(client)));

                



            }
        }

        private async void HandleConnection(TcpClient client)
        {
            HttpResponse response;
            try
            {
                response = await ReceiveRequest(client);
            }
            catch(NpgsqlException)
            {
                response = new HttpResponse(HttpStatusCode.BadRequest, "Something went wrong");
            }
            var data = Encoding.UTF8.GetBytes(response.ToString());
            client.GetStream().Write(data, 0, data.Length);
        }

        private async Task<HttpResponse> ReceiveRequest(TcpClient client)
        {
            // Get the client's stream
            var stream = client.GetStream();

            // Read the request data from the stream
            var buffer = new byte[client.ReceiveBufferSize];
            var bytesRead = stream.Read(buffer, 0, buffer.Length);
            var requestData = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            // Parse the request data
            requestData = requestData.Replace(Environment.NewLine, "\n").Replace("\r", "\n")
                .Replace("\n", Environment.NewLine);
            var requestDataSplit = requestData.Split(Environment.NewLine + Environment.NewLine);
            var requestHeader = requestDataSplit[0].Split(Environment.NewLine);
            var firstHeaderLine = requestHeader[0].Split(" ");

            var dataDictionary = new Dictionary<string, string>
            {
                { "Method", firstHeaderLine[0] },
                { "Path", firstHeaderLine[1].Split("/")[1] },
                { "Data", requestDataSplit[1] }
            };
            if (firstHeaderLine[1].Split("/").Length > 2)
            {
                dataDictionary.Add("addendumPath", firstHeaderLine[1].Split("/")[2]);
            }
            

            foreach (var headerLine in requestHeader.Skip(1))
            {
                var pairs = headerLine.Split(": ");
                dataDictionary.Add(pairs[0], pairs[1]);
            }

            Console.WriteLine(requestData);

            var requestHandler = new RequestHandler(dataDictionary, client);

            return await requestHandler.HandleRequest(BattleLobby);
        }
    }



