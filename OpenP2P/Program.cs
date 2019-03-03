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

        static void Main(string[] args)
        {
            NetworkSocket server = new NetworkSocket(9000);
            NetworkSocket client1 = new NetworkSocket("127.0.0.1", 9000);
            NetworkSocket client2 = new NetworkSocket("127.0.0.1", 9000);

            List<NetworkSocket> clients = new List<NetworkSocket>();

            for(int i=0; i<10000; i++)
            {
                clients.Add(new NetworkSocket("127.0.0.1", 9000, 9000 + i + 1));
            }

            server.OnReceive += OnReceiveEvent;
            server.OnSend += OnSendEvent;
            server.Listen();


            string test = "123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890";

            sw = Stopwatch.StartNew();

            for (int i = 0; i < clients.Count; i++)
            {
                NetworkSocket client = clients[i];
                client.stream.BeginWrite();

                //client1.stream.WriteHeader(NetworkProtocol.MessageType.SendMessage);
                //client1.stream.WriteTimestamp();

                client.stream.Write(i);
                //client1.stream.Write(1.05f);
                //client1.stream.Write((short)100);

                //client1.stream.Write("Hello from Texas");
                client.stream.EndWrite();
                

                //client1.Send("c1: " + i + " " + test);
                //Thread.Sleep(1);
                //client2.Send("c2: " + i + " " + test);
                //Thread.Sleep(1);
            }


            //Console.WriteLine(test.Length);
            //client.Listen();
            //server.Send()

            sw.Stop();
            Console.WriteLine("Finished with " + NetworkSocket.EVENTPOOL.eventCount + " SocketAsyncEventArgs");
            Console.WriteLine("Finished in " + ((float)sw.ElapsedMilliseconds / 1000f) + " seconds");

            Thread.Sleep(10000);
            //Console.ReadLine();
        }

        static void OnSendEvent(object sender, NetworkSocketEvent e)
        {

        }

        static void OnReceiveEvent(object sender, NetworkSocketEvent e)
        {
            //Console.WriteLine("Received from: " + e.args.RemoteEndPoint.ToString());

            NetworkStream stream = e.stream;
            int clientID = stream.ReadInt();
            //if( run < 96000)
            //    Console.WriteLine("clientID: " + clientID);
            
                
            //stream.BeginRead();
            /*
            Console.WriteLine("MessageType: " + stream.ReadHeader().ToString());
            Console.WriteLine("Timestamp: " + stream.ReadTimestamp());
            
            Console.WriteLine("float: " + stream.ReadFloat());
            Console.WriteLine("short: " + stream.ReadShort());
            Console.WriteLine("String: " + stream.ReadString());*/
            //Console.WriteLine("Received message: " + System.Text.Encoding.ASCII.GetString(e.args.Buffer, 0, e.args.BytesTransferred) + " [" + NetworkSocket.EVENTPOOL.eventCount + "]");
        }
       
    }
}
