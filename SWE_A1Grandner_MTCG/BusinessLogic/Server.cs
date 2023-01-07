
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Sockets;
using System.Text;
using SWE_A1Grandner_MTCG.Databank;
using SWE_A1Grandner_MTCG.BattleLogic;
using HttpStatusCode = SWE_A1Grandner_MTCG.Enum.HttpStatusCode;

namespace SWE_A1Grandner_MTCG.BusinessLogic;
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
                await Task.Run(() => HandleConnection(client));


            }
        }

        private async Task HandleConnection(TcpClient client)
        {
            var response = await ReceiveRequest(client);
            var data = Encoding.UTF8.GetBytes(response.ToString());
            client.GetStream().Write(data, 0, data.Length);
        }

        private async Task<HttpResponse> ReceiveRequest(TcpClient client)
        {
            // Get the client's stream
            var stream = client.GetStream();

            // Read the request data from the stream
            var buffer = new byte[client.ReceiveBufferSize];
            var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
            var requestData = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            // Parse the request data
            Console.WriteLine(requestData);
            requestData = requestData.Replace(Environment.NewLine, "\n").Replace("\r", "\n")
                .Replace("\n", Environment.NewLine);
            var requestDataSplit = requestData.Split(Environment.NewLine + Environment.NewLine);
            var requestHeader = requestDataSplit[0].Split(Environment.NewLine);
            var firstHeaderLine = requestHeader[0].Split(" ");
            var dataDictionary = new Dictionary<string, string>
            {
                { "Method", firstHeaderLine[0] },
                { "Path", firstHeaderLine[1] },
                { "Data", requestDataSplit[1] }
            };

            for (var i = 1; i < requestHeader.Length; i++)
            {
                var pairs = requestHeader[i].Split(": ");
                dataDictionary.Add(pairs[0], pairs[1]);
            }


            Console.WriteLine(requestData);

            var requestHandler = new RequestHandler(dataDictionary, client);

            return await requestHandler.HandleRequest();


        }

    }



