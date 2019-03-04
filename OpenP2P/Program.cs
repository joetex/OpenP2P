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
        static Stopwatch sw;
        public const int MAXSEND = 100000;
        public const int MAXCLIENTS = 1;
        static int receiveCount = 0;
        static int sendCount = 0;
        static Stopwatch sendSW;
        static Stopwatch recvSW;
        static void Main(string[] args)
        {
            NetworkSocket server = new NetworkSocket(9000);
            NetworkSocket client1 = new NetworkSocket("127.0.0.1", 9000);
            NetworkSocket client2 = new NetworkSocket("127.0.0.1", 9000);

            List<NetworkSocket> clients = new List<NetworkSocket>();

            for(int i=0; i< MAXCLIENTS; i++)
            {
                clients.Add(new NetworkSocket("127.0.0.1", 9000, 0));
                clients[i].OnSend += OnSendEvent;
            }

            server.OnReceive += OnReceiveEvent;

            server.Listen(null);


            string test = "12345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890";

            sw = Stopwatch.StartNew();

            Console.WriteLine("Send * Client = " + (MAXSEND * MAXCLIENTS));

            Random rnd = new Random();
            
            for (int i = 0; i < MAXSEND; i++)
                for (int j = 0; j < MAXCLIENTS; j++)
                {
                    NetworkStream stream = clients[j].BeginSend();
                    //stream.WriteHeader(NetworkProtocol.MessageType.SendMessage);
                    //stream.Write(911);
                    //stream.Write((ushort)420);
                    stream.Write(NextFloat(rnd));
                    stream.Write(NextFloat(rnd));
                    stream.Write(NextFloat(rnd));
                    //stream.Write(1.875);
                    //stream.WriteTimestamp();
                    //stream.Write(test);
                    //stream.Write(1.05f);
                    //stream.Write((short)100);
                    //stream.Write("Hello from Texas");
                    clients[j].EndSend(stream);
                }

            
            sw.Stop();
            Console.WriteLine("Finished with " + NetworkSocket.EVENTPOOL.eventCount + " SocketAsyncEventArgs");
            Console.WriteLine("Finished in " + ((float)sw.ElapsedMilliseconds / 1000f) + " seconds");
            
            //Thread.Sleep(10000);
        }

        static float NextFloat(Random random)
        {
            double mantissa = (random.NextDouble() * 2.0) - 1.0;
            // choose -149 instead of -126 to also generate subnormal floats (*)
            double exponent = Math.Pow(2.0, random.Next(-126, 128));
            return (float)(mantissa * exponent);
        }

        static void OnSendEvent(object sender, NetworkSocketEvent se)
        {
            if( sendCount == 0 )
                sendSW = Stopwatch.StartNew();
            sendCount++;
            if (sendCount >= MAXSEND * MAXCLIENTS)
            {
                sendSW.Stop();
                Console.WriteLine("SEND finished in " + ((float)sendSW.ElapsedMilliseconds / 1000f) + " seconds");
            }
                
        }


        static Dictionary<string, bool> endpoints = new Dictionary<string, bool>();

        static void OnReceiveEvent(object sender, NetworkSocketEvent se)
        {
            //Console.WriteLine("Received from: " + e.args.RemoteEndPoint.ToString());
            //endpoints.Add(se.args.RemoteEndPoint.ToString(), true);

            NetworkStream stream = se.stream;

            //Console.WriteLine("stream size: " + stream.byteLength + " B");
            if (receiveCount == 0)
                recvSW = Stopwatch.StartNew();
            receiveCount++;
            
            if (receiveCount >= MAXSEND*MAXCLIENTS)
            {
                recvSW.Stop();
                Console.WriteLine("RECV finished in " + ((float)recvSW.ElapsedMilliseconds / 1000f) + " seconds");
                foreach (KeyValuePair<string, bool> entry in endpoints)
                {
                    //Console.WriteLine("Client: " + entry.Key);
                    // do something with entry.Value or entry.Key
                }
            }
                
           // Console.WriteLine("MessageType: " + stream.ReadHeader().ToString());
            //Console.WriteLine("Timestamp: " + stream.ReadTimestamp());
            //int clientID = stream.ReadUShort();
            //Console.WriteLine("ReadInt: " + stream.ReadInt());
            //Console.WriteLine("ReadUShort: " + stream.ReadUShort());
            //Console.WriteLine("ReadFloat: " + stream.ReadFloat());
            //Console.WriteLine("ReadDouble: " + stream.ReadDouble());
            //Console.WriteLine("clientID: " + clientID);
            //Console.WriteLine("float: " + stream.ReadFloat());
            //Console.WriteLine("short: " + stream.ReadShort());
            //Console.WriteLine("String: " + stream.ReadString());

        }
       
    }
}
