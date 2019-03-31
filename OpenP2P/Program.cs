
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
        public const int MAXSEND = 10;

        static void Main(string[] args)
        {
            NetworkThread.StartNetworkThreads();
            NetworkServer server = new NetworkServer(9000);
            List<NetworkClient> clients = new List<NetworkClient>();
            NetworkClient client = null;// new NetworkClient("127.0.0.1", 9000, 9002);

            //Thread.Sleep(1000);

            for (int i=0; i< MAXSEND; i++)
            {
                client = new NetworkClient("127.0.0.1", 9000, 9002+i);
                
                clients.Add(client);
            }
            
            for(int i=0;i<MAXSEND; i++)
            {
                clients[i].ConnectToServer("JoeOfTex");
            }

            Thread.Sleep(3000);

            Console.WriteLine("Reliable Count: " + NetworkThread.RELIABLEQUEUE.Count);
            Console.WriteLine("Ack Count: " + NetworkThread.ACKNOWLEDGED.Count);
            Console.WriteLine("Client Receive Cnt: " + client.receiveCnt);
            Console.WriteLine("Server Receive Cnt: " + server.receiveCnt);
        }
    }
}
