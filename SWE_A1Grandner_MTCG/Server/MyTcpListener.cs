using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SWE_A1Grandner_MTCG.Server
{
    internal class MyTcpListener
    {
        public TcpListener Server { get; set; }

        public MyTcpListener(IPAddress localAddress, Int32 port)
        {
            Server = new TcpListener(localAddress, port);
            RunServer();
        }

        public void RunServer()
        {
            Server.Start();

            //Buffer
            Byte[] bytes = new Byte[256];
            String? data = null;

            while (true)
            {
                Console.Write("Waiting for connection... ");

                using TcpClient client = Server.AcceptTcpClient();
                Console.WriteLine("Connected!");

                NetworkStream stream = client.GetStream();

                int i = 0;

                while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                {
                    data = System.Text.Encoding.ASCII.GetString(bytes, 0, i);
                    Console.WriteLine("Received: {0}", data);

                    data = data.ToUpper();

                    byte[] msg = System.Text.Encoding.ASCII.GetBytes(data);
                }
            }

        }
    }
}
