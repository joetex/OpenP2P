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
        public const int MAXSEND = 1000;
        public const int MAXCLIENTS = 1;
        static long receiveCount = 0;
        static long sendByteCount = 0;
        static long sendCount = 0;
        static Stopwatch sendSW;
        static Stopwatch recvSW;
        static byte[] testBytes;
        static Dictionary<string, bool> endpoints = new Dictionary<string, bool>();
        public static long receiveByteCount = 0;

        static void Main(string[] args)
        {
            NetworkThread.StartNetworkThreads();

            NetworkSocket server = new NetworkSocket(9000);
            server.OnReceive += OnReceiveEvent;
            server.Listen(null);
            //NetworkThread.recvStream = server.Reserve();

            //server.Listen(null);
            //server.Listen(null);
            //server.Listen(null);
            //server.Listen(null);
            List<NetworkSocket> clients = new List<NetworkSocket>();
            for(int i=0; i< MAXCLIENTS; i++)
            {
                clients.Add(new NetworkSocket("127.0.0.1", 9000, 0));
                clients[i].OnSend += OnSendEvent;
            }
            
            string test = "";
            for(int i=0; i<1; i++)
            {
                test += "1234567890";
            }

            testBytes = Encoding.ASCII.GetBytes(test);
            Console.WriteLine("testBytes Count: " + testBytes.Length);

            sw = Stopwatch.StartNew();

            Console.WriteLine("Send * Client = " + (MAXSEND * MAXCLIENTS));

            //Random rnd = new Random();
            NetworkStream[] streams = new NetworkStream[MAXSEND];
            Random rnd = new Random((int)sw.ElapsedTicks);
            for (int i = 0; i < MAXSEND; i++)
            {
                receiveIds[i] = 0;
                for (int j = 0; j < MAXCLIENTS; j++)
                {
                    NetworkStream stream = clients[j].BeginSend();
                    //stream.WriteHeader(NetworkProtocol.MessageType.SendMessage);
                    stream.Write(i);
                    //stream.Write((ushort)420);
                    
                    stream.Write(testBytes);

                    streams[i] = stream;
                    
                    //stream.byteLength += 49000;
                    //stream.Write(1.875);
                    //stream.WriteTimestamp();
                    //stream.Write(test);
                    //stream.Write(1.05f);
                    //stream.Write((short)100);
                    //stream.Write("Hello from Texas");
                }
            }
                

            for (int i = 0; i < MAXSEND; i++)
                for (int j = 0; j < MAXCLIENTS; j++)
                {
                    //NetworkStream stream = clients[j].BeginSend();

                    clients[j].EndSend(streams[i]);
                }


            sw.Stop();
            Console.WriteLine("Finished with " + NetworkThread.STREAMPOOL.streamCount + " SocketAsyncEventArgs");
            Console.WriteLine("Finished in " + ((float)sw.ElapsedMilliseconds / 1000f) + " seconds");

            Thread.Sleep(3000);

            Console.WriteLine("sendCount = " + sendCount);
            Console.WriteLine("sendByteCount = " + sendByteCount);

            Console.WriteLine("receiveCount = " + receiveCount);
            Console.WriteLine("receiveByteCount = " + receiveByteCount);

            int missingCnt = 0;
            for (int i = 0; i<MAXSEND; i++)
            {
                if (receiveIds[i] == 0)
                    missingCnt++;
            }
            Console.WriteLine("Missing Packets: " + missingCnt);
            //Thread.Sleep(5000);
        }


        static void OnSendEvent(object sender, NetworkStream stream)
        {
            if( sendCount == 0 )
                sendSW = Stopwatch.StartNew();
            sendCount++;
            if( stream.byteLength != 4 )
            {
                //Console.WriteLine("SEND Packet oversized: " + stream.byteLength);
            }
            sendByteCount += stream.byteLength;
            //if (stream.byteSent != testBytes.Length)
                //Console.WriteLine("packet size: " + stream.byteLength);
            if (sendCount >= MAXSEND * MAXCLIENTS)
            {
                sendSW.Stop();
                Console.WriteLine("SEND finished in " + ((float)sendSW.ElapsedMilliseconds / 1000f) + " seconds");
            }
                
        }


        static long receiveSum = 0;

        public static int[] receiveIds = new int[MAXSEND];

        static void OnReceiveEvent(object sender, NetworkStream stream)
        {
            //Console.WriteLine("Received from: " + e.args.RemoteEndPoint.ToString());
            //endpoints.Add(se.args.RemoteEndPoint.ToString(), true);
            

            //Console.WriteLine("stream size: " + stream.byteLength + " B");
            if (receiveCount == 0)
                recvSW = Stopwatch.StartNew();
            receiveCount++;
            receiveByteCount += stream.byteLength;

            int id = stream.ReadInt();
            receiveIds[id] = 1;
            receiveSum += id;

            if (stream.byteLength > 4)
            {
                //Console.WriteLine("RECV Packet oversized: " + stream.byteLength);
            }
            //if (receiveCount % 1000 == 0)
            //    Console.WriteLine("ReceiveCount: " + receiveCount);
            if (receiveCount >= MAXSEND*MAXCLIENTS)
            {
                recvSW.Stop();
                Console.WriteLine("RECV finished in " + ((float)recvSW.ElapsedMilliseconds / 1000f) + " seconds");
                Console.WriteLine("Received: " + receiveByteCount + " bytes");
                Console.WriteLine("Received sum: " + receiveSum);
                long expected = (long)MAXSEND * ((long)MAXSEND + (long)1) / (long)2;
                Console.WriteLine("Expected sum: " + expected);
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



        static float NextFloat(Random random)
        {
            double mantissa = (random.NextDouble() * 2.0) - 1.0;
            // choose -149 instead of -126 to also generate subnormal floats (*)
            double exponent = Math.Pow(2.0, random.Next(-126, 128));
            return (float)(mantissa * exponent);
        }

    }
}
