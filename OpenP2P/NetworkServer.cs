using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static OpenP2P.NetworkIdentity;

namespace OpenP2P
{
    public class NetworkServer
    {
        public NetworkProtocol protocol = null;
        public Dictionary<string, string> connections = new Dictionary<string, string>();
        public int receiveCnt = 0;
        static Stopwatch recieveTimer;

        public NetworkServer(int localPort)
        {
            protocol = new NetworkProtocol(localPort);
            protocol.RegisterAsServer();
            protocol.AttachRequestListener(MessageType.ConnectToServer, OnRequestConnectToServer);
            protocol.AttachRequestListener(MessageType.Heartbeat, OnRequestHeartbeat);
            //protocol.Listen();
        }

        public void OnRequestHeartbeat(object sender, NetworkMessage message)
        {
            

            MsgHeartbeat heartbeat = (MsgHeartbeat)message;
            //Console.WriteLine("Received Heartbeat from ("+ message.peer.id +") :");
            //Console.WriteLine(heartbeat.timestamp);
        }
        
        public void OnRequestConnectToServer(object sender, NetworkMessage message)
        {
            PerformanceTest();

            NetworkStream stream = (NetworkStream)sender;
            PeerIdentity peer = protocol.ident.RegisterPeer(stream.remoteEndPoint);

            MsgConnectToServer connectMsg = (MsgConnectToServer)message;
            connectMsg.responseConnected = true;
            connectMsg.responsePeerId = peer.id;
            
            protocol.SendResponse(stream, connectMsg);
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
            }
        }
    }
}
