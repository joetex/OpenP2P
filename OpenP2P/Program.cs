
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
        public const int MAXSEND = 100000;

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

            
            NetworkConfig.ProfileEnable();
            

            //createClient.Stop();
            //Console.WriteLine("Clients created in " + ((float)createClient.ElapsedMilliseconds / 1000f) + " seconds");

            //Thread.Sleep(100);
            for (int i=0;i<MAXSEND; i++)
            {
                client.ConnectToServer("JoeOfTex");
                if (i % 500 == 0)
                    Thread.Sleep(1);
            }

            
            /*
            NetworkConfig.ProfileBegin("TEST_SEND_LOOP");
            int clientReceiveCnt = 0;
            for(int i=0; i<MAXSEND; i++)
            {
                int test = 1;
                test = test * test;
                //clientReceiveCnt += clients[i].receiveCnt;
            }
            NetworkConfig.ProfileEnd("TEST_SEND_LOOP");*/
            Thread.Sleep(5000);

            //NetworkConfig.ProfileReportAll();
            //Console.WriteLine("Reliable Count: " + NetworkThread.RELIABLEQUEUE.Count);
            //Console.WriteLine("Ack Count: " + NetworkThread.ACKNOWLEDGED.Count);
            Console.WriteLine("Client StreamPool Count = " + client.protocol.socket.thread.STREAMPOOL.streamCount);
            Console.WriteLine("Server StreamPool Count = " + server.protocol.socket.thread.STREAMPOOL.streamCount);
            Console.WriteLine("Client Receive Cnt: " + client.receiveCnt);
            Console.WriteLine("Server Receive Cnt: " + server.receiveCnt);
            Thread.Sleep(20000);

            Console.WriteLine("Client Receive Cnt: " + client.receiveCnt);
            Console.WriteLine("Server Receive Cnt: " + server.receiveCnt);
            //Thread.Sleep(1000);
            //NetworkConfig.ProfileReportAll(); 
        }
    }
}
