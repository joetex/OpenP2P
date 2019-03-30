
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OpenP2P
{
    class Program
    {
        public const int MAXSEND = 3000;

        static void Main(string[] args)
        {
            NetworkServer server = new NetworkServer(9000);
            List<NetworkClient> clients = new List<NetworkClient>();
            NetworkClient client = new NetworkClient("127.0.0.1", 9000, 9002);

            //Thread.Sleep(1000);

            for (int i=0; i< MAXSEND; i++)
            {
                client.ConnectToServer("JoeOfTex");
            }
            

            Thread.Sleep(500);

            Console.WriteLine("Reliable Count: " + client.protocol.threads.RELIABLEQUEUE.Count);
            Console.WriteLine("Ack Count: " + client.protocol.threads.ACKNOWLEDGED.Count);
            Console.WriteLine("Client Receive Cnt: " + client.receiveCnt);
            Console.WriteLine("Server Receive Cnt: " + server.receiveCnt);
        }
    }
}
