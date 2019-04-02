
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


        public static int receiveCnt = 0;
        static Stopwatch recieveTimer;
        public NetworkClient(string remoteHost, int remotePort, int localPort)
        {
            
            protocol = new NetworkProtocol(localPort);
            serverHost = protocol.GetEndPoint(remoteHost, remotePort);
            protocol.AttachResponseListener(MessageType.ConnectToServer, OnResponseConnectToServer);
            //protocol.Listen();
        }
        
        public void ConnectToServer(string userName)
        {
            protocol.ConnectToServer(serverHost, userName);
            /*MsgConnectToServer msg = protocol.Create<MsgConnectToServer>();
            msg.requestUsername = userName;
            protocol.SendReliableRequest(serverHost, msg);*/
            PerformanceTest();
        }

        public void SendHeartbeat()
        {
            MsgHeartbeat msg = protocol.Create<MsgHeartbeat>();
            msg.timestamp = NetworkTime.Milliseconds();
            protocol.SendReliableRequest(serverHost, msg);
        }
        
        public void OnResponseConnectToServer(object sender, NetworkMessage message)
        {
            

            //MsgConnectToServer connectMsg = (MsgConnectToServer)message;
        }

        public void PerformanceTest()
        {
            if (receiveCnt == 0)
                recieveTimer = Stopwatch.StartNew();

            //Interlocked.Increment(ref receiveCnt);
            receiveCnt++;

            if (receiveCnt == Program.MAXSEND)
            {
                recieveTimer.Stop();
                Console.WriteLine("CLIENT Finished in " + ((float)recieveTimer.ElapsedMilliseconds / 1000f) + " seconds");
            }
        }
    }
}
