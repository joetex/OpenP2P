
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OpenP2P
{
    public class NetworkClient
    {
        public NetworkProtocol protocol = null;

        public IPEndPoint serverHost = null;


        public int receiveCnt = 0;
        static Stopwatch timer;
        static Dictionary<ulong, Stopwatch> recieveTimer = new Dictionary<ulong, Stopwatch>();
        public NetworkClient(string remoteHost, int remotePort, int localPort)
        {
            
            protocol = new NetworkProtocol(localPort, false);
            serverHost = protocol.GetEndPoint(remoteHost, remotePort);
            protocol.AttachResponseListener(MessageType.ConnectToServer, OnResponseConnectToServer);
            //protocol.Listen();
        }
        
        public void ConnectToServer(string userName)
        {
            
            NetworkStream stream = protocol.ConnectToServer(serverHost, userName);

            Stopwatch sw = new Stopwatch();
            
            recieveTimer.Add(stream.ackkey, sw);
            sw.Start();
            /*MsgConnectToServer msg = protocol.Create<MsgConnectToServer>();
            msg.requestUsername = userName;
            protocol.SendReliableRequest(serverHost, msg);*/

        }

        public void SendHeartbeat()
        {
            MsgHeartbeat msg = protocol.Create<MsgHeartbeat>();
            msg.timestamp = NetworkTime.Milliseconds();
            protocol.SendRequest(serverHost, msg);
        }
        
        public void OnResponseConnectToServer(object sender, NetworkMessage message)
        {
            NetworkStream stream = (NetworkStream)sender;
            recieveTimer[stream.ackkey].Stop();
            long end = recieveTimer[stream.ackkey].ElapsedMilliseconds;
            Console.WriteLine("Ping took: " + end + " milliseconds");
            PerformanceTest();
            //MsgConnectToServer connectMsg = (MsgConnectToServer)message;
        }

        public void PerformanceTest()
        {
            if (receiveCnt == 0)
                timer = Stopwatch.StartNew();

            //Interlocked.Increment(ref receiveCnt);
            receiveCnt++;

            if (receiveCnt == Program.MAXSEND)
            {
                timer.Stop();
                Console.WriteLine("CLIENT Finished in " + ((float)timer.ElapsedMilliseconds / 1000f) + " seconds");
            }
        }
    }
}
