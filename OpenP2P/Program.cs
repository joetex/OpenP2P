
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
        public const int MAXSEND = 10000;

        static void Main(string[] args)
        {
            
            NetworkServer server = new NetworkServer(9000);
            List<NetworkClient> clients = new List<NetworkClient>();
            NetworkClient client = null;// new NetworkClient("127.0.0.1", 9000, 9002);

            //Stopwatch createClient = new Stopwatch();
            //createClient.Start();
            //for (int i=0; i< MAXSEND; i++)
            {
                client = new NetworkClient("127.0.0.1", 9000, 9002);
                
                //clients.Add(client);
            }

            NetworkThread.StartNetworkThreads();
            //createClient.Stop();
            //Console.WriteLine("Clients created in " + ((float)createClient.ElapsedMilliseconds / 1000f) + " seconds");

            Thread.Sleep(100);
            for (int i=0;i<MAXSEND; i++)
            {
                client.ConnectToServer("JoeOfTex");
            }

            Thread.Sleep(2000);
            int clientReceiveCnt = 0;
            for(int i=0; i<MAXSEND; i++)
            {
                //clientReceiveCnt += clients[i].receiveCnt;
            }
            Console.WriteLine("Reliable Count: " + NetworkThread.RELIABLEQUEUE.Count);
            Console.WriteLine("Ack Count: " + NetworkThread.ACKNOWLEDGED.Count);
            Console.WriteLine("StreamPool Count = " + NetworkThread.STREAMPOOL.streamCount);
            Console.WriteLine("Client Receive Cnt: " + NetworkClient.receiveCnt);
            Console.WriteLine("Server Receive Cnt: " + server.receiveCnt);
        }
    }
}
