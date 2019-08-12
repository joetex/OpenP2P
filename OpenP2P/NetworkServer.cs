using System;
using System.Collections.Generic;
using System.Diagnostics;
using static OpenP2P.NetworkIdentity;

namespace OpenP2P
{
    public class NetworkServer
    {
        public NetworkProtocol protocol = null;
        public Dictionary<string, string> connections = new Dictionary<string, string>();
        public int receiveCnt = 0;
        static Stopwatch recieveTimer;

        public NetworkServer(String localIP, int localPort)
        {
            protocol = new NetworkProtocol(localIP, localPort, true);
            protocol.AttachMessageListener(ChannelType.ConnectToServer, OnMessageConnectToServer);
            protocol.AttachMessageListener(ChannelType.Heartbeat, OnMessageHeartbeat);
        }

        

        public void OnMessageHeartbeat(object sender, NetworkMessage message)
        {
            

            MsgHeartbeat heartbeat = (MsgHeartbeat)message;
            //Console.WriteLine("Received Heartbeat from ("+ message.peer.id +") :");
            //Console.WriteLine(heartbeat.timestamp);
        }
        
        public void OnMessageConnectToServer(object sender, NetworkMessage message)
        {
            PerformanceTest();
            
            NetworkPacket packet = (NetworkPacket)sender;
        }


        public void PerformanceTest()
        {
            if (receiveCnt == 0)
                recieveTimer = Stopwatch.StartNew();

            receiveCnt++;

            if (receiveCnt == Program.MAXSEND)
            {
                recieveTimer.Stop();
                Console.WriteLine("SERVER Finished in " + ((float)recieveTimer.ElapsedMilliseconds / 1000f) + " seconds");
                NetworkConfig.ProfileReportAll();
            }
        }
    }
}
