
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace OpenP2P
{
    public class NetworkClient
    {
        public NetworkProtocol protocol = null;

        public IPEndPoint serverHost = null;


        public int receiveCnt = 0;
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
            MsgConnectToServer msg = protocol.Create<MsgConnectToServer>();
            msg.requestUsername = userName;
            protocol.SendReliableRequest(serverHost, msg);
        }

        public void SendHeartbeat()
        {
            MsgHeartbeat msg = protocol.Create<MsgHeartbeat>();
            msg.timestamp = NetworkTime.Milliseconds();
            protocol.SendReliableRequest(serverHost, msg);
        }
        
        public void OnResponseConnectToServer(object sender, NetworkMessage message)
        {
            PerformanceTest();

            MsgConnectToServer connectMsg = (MsgConnectToServer)message;
        }

        public void PerformanceTest()
        {
            if (receiveCnt == 0)
                recieveTimer = Stopwatch.StartNew();

            receiveCnt++;

            if (receiveCnt == Program.MAXSEND)
            {
                recieveTimer.Stop();
                Console.WriteLine("CLIENT Finished in " + ((float)recieveTimer.ElapsedMilliseconds / 1000f) + " seconds");
            }
        }
    }
}
