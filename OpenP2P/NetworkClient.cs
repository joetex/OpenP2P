
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
        Stopwatch timer;
        Dictionary<uint, Stopwatch> recieveTimer = new Dictionary<uint, Stopwatch>();
        public NetworkClient(string remoteHost, int remotePort, int localPort)
        {
            
            protocol = new NetworkProtocol(localPort, false);
            serverHost = protocol.GetEndPoint(remoteHost, remotePort);
            protocol.AttachResponseListener(MessageChannel.ConnectToServer, OnResponseConnectToServer);
            protocol.AttachErrorListener(NetworkErrorType.ErrorReliableFailed, OnErrorReliableFailed);
            //protocol.Listen();
        }
        
        public void OnErrorReliableFailed(object sender, NetworkPacket packet)
        {
            Console.WriteLine("[ERROR] " + packet.lastErrorType.ToString() + ": " + packet.lastErrorMessage);
        }

        public void ConnectToServer(string userName)
        {
            NetworkPacket packet = protocol.ConnectToServer(serverHost, userName);

            Stopwatch sw = new Stopwatch();
            
            recieveTimer.Add(packet.ackkey, sw);
            sw.Start();
        }

        public void SendHeartbeat()
        {
            MsgHeartbeat msg = protocol.Create<MsgHeartbeat>();
            msg.timestamp = NetworkTime.Milliseconds();
            protocol.SendMessage(serverHost, msg);
        }
        
        public void OnResponseConnectToServer(object sender, NetworkMessage message)
        {
            NetworkPacket packet = (NetworkPacket)sender;
            recieveTimer[packet.ackkey].Stop();
            long end = recieveTimer[packet.ackkey].ElapsedMilliseconds;
            //Console.WriteLine("Ping took: " + end + " milliseconds");
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
