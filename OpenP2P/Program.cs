
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
        public const int MAXCLIENTS = 1;
        public const int MAXSEND = 100;

        public static string connectToAddress = "127.0.0.1";

        static void Main(string[] args)
        {
            bool isServer = false;
          
            for (int i = 0; i < args.Length; i++)
            {
                if( args[i].ToLower().Equals("--server") )
                {
                    isServer = true;
                }
                if( args[i].ToLower().StartsWith("--connectto") )
                {
                    if( i < args.Length - 1 )
                    {
                        connectToAddress = args[++i];
                    }
                }
                Console.WriteLine("Arg[{0}] = [{1}]", i, args[i]);
            }


            
            if(isServer )
            {
                RunServer();
            }
            else
            {
                RunClient();
            }
            
            //Thread t = new Thread(Test1);
            //t.Start();
            //t = new Thread(Test2);
            //t.Start();

            //Thread.Sleep(3000);
        }
        static void Test1()
        {
            int c = 1000000;
            var sw = new Stopwatch();
            sw.Start();
            long sum = 0;
            for (var i = 0; i < c; i++)
            {
                sum += Environment.TickCount;
            }
            sw.Stop();
            Console.WriteLine("Test1 " + c + ": " + sw.ElapsedMilliseconds + "   (ignore: " + sum + ")");
        }

        static void Test2()
        {
            int c = 1000000;
            var sw = new Stopwatch();
            sw.Start();
            long sum = 0;
            for (var i = 0; i < c; i++)
            {
                sum += DateTime.Now.Ticks;
            }
            sw.Stop();
            Console.WriteLine("Test2 " + c + ": " + sw.ElapsedMilliseconds + "   (ignore: " + sum + ")");
        }
        public static void RunServer()
        {
            string localIP = NetworkConfig.GetPublicIP();
            Console.WriteLine("IPAddress = " + localIP);
            
            NetworkServer server = new NetworkServer("127.0.0.1", 9000);
        }

        public static void RunClient()
        {
            List<NetworkClient> clients = new List<NetworkClient>();
            NetworkClient client = null;// new NetworkClient("127.0.0.1", 9000, 9002);

            //Stopwatch createClient = new Stopwatch();
            //createClient.Start();
            for (int i=0; i< MAXCLIENTS; i++)
            {
                client = new NetworkClient(connectToAddress, 9000, 0);

                clients.Add(client);
            }


            NetworkConfig.ProfileEnable();


            //createClient.Stop();
            //Console.WriteLine("Clients created in " + ((float)createClient.ElapsedMilliseconds / 1000f) + " seconds");

            //Thread.Sleep(100);
            //int i = 0;
            char[] letters = "abcdefghijklmnopqrstuvwxyz0123456789".ToCharArray();
            Random random = new Random();
            string username = "";

            for(int x=0; x<MAXCLIENTS; x++)
            {
                for (int i = 0; i < MAXSEND; i++)
                {
                    //username = "123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890";
                    username = "JoeOfTex";
                    //for (int j = 0; j < random.Next(10, 32); j++)
                    {
                        //username += letters[random.Next(0, letters.Length - 1)];
                    }
                    clients[x].ConnectToServer(username);
                    //if (i % 500 == 0)
                    //    Thread.Sleep(1);
                }
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
            Thread.Sleep(6000);

            //NetworkConfig.ProfileReportAll();
            //Console.WriteLine("Reliable Count: " + NetworkThread.RELIABLEQUEUE.Count);
            //Console.WriteLine("Ack Count: " + NetworkThread.ACKNOWLEDGED.Count);
            for(int i=0; i<MAXCLIENTS; i++)
            {
                Console.WriteLine("Client PacketPool Count = " + clients[i].protocol.socket.thread.PACKETPOOL.packetCount);
                //Console.WriteLine("Server PacketPool Count = " + server.protocol.socket.thread.PACKETPOOL.packetCount);
                Console.WriteLine("Client Receive Cnt: " + clients[i].receiveCnt);
                //Console.WriteLine("Server Receive Cnt: " + server.receiveCnt);
                //Thread.Sleep(20000);

                //Console.WriteLine("Client Receive Cnt: " + client.receiveCnt);
            }
            
            //Console.WriteLine("Server Receive Cnt: " + server.receiveCnt);
            //Thread.Sleep(1000);
            //NetworkConfig.ProfileReportAll(); 
        }
    }
}
