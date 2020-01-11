
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
    public partial class NetworkClient
    {
        public NetworkProtocol protocol = null;

        public IPEndPoint serverHost = null;
        public NetworkPeer server = null;
        public Thread mainThread = null;
        

        public int receiveCnt = 0;
        Stopwatch timer;
        Dictionary<uint, Stopwatch> recieveTimer = new Dictionary<uint, Stopwatch>();
        public NetworkClient(string remoteHost, int remotePort, int localPort)
        {
            protocol = new NetworkProtocol(localPort, false);
            protocol.AttachMessageListener(ChannelType.ConnectToServer, OnMessageConnectToServer);
            protocol.AttachResponseListener(ChannelType.ConnectToServer, OnResponseConnectToServer);
            protocol.AttachResponseListener(ChannelType.Heartbeat, OnResponseHeartbeat);
            protocol.AttachErrorListener(NetworkErrorType.ErrorReliableFailed, OnErrorReliableFailed);
            
            IPEndPoint serverHost = protocol.GetEndPoint(remoteHost, remotePort);
            server = new NetworkPeer(protocol);
            server.AddEndpoint(serverHost);

            
        }

        private void OnMessageConnectToServer(object sender, NetworkMessage e)
        {
            NetworkPacket packet = (NetworkPacket)sender;
            //recieveTimer[message.header.ackkey].Stop();
            //long end = recieveTimer[message.header.ackkey].ElapsedMilliseconds;
            //Console.WriteLine("Ping took: " + end + " milliseconds");
            PerformanceTest();

            mainThread = new Thread(MainThread);
            mainThread.Start();
        }

        public void OnErrorReliableFailed(object sender, NetworkPacket packet)
        {
            Console.WriteLine("[ERROR] " + packet.lastErrorType.ToString() + ": " + packet.lastErrorMessage);
        }

        public void ConnectToServer(string userName)
        {
            MsgConnectToServer message = protocol.ConnectToServer(userName);
            message.msgNumber = 10;
            message.msgShort = 20;
            message.msgBool = true;

            protocol.SendReliableMessage(server.GetEndpoint(), message);
        }
        public void OnResponseConnectToServer(object sender, NetworkMessage message)
        {
            NetworkPacket packet = (NetworkPacket)sender;
            //recieveTimer[message.header.ackkey].Stop();
            //long end = recieveTimer[message.header.ackkey].ElapsedMilliseconds;
            //Console.WriteLine("Ping took: " + end + " milliseconds");
            PerformanceTest();
            mainThread = new Thread(MainThread);
            mainThread.Start();
            //MsgConnectToServer connectMsg = (MsgConnectToServer)message;
        }


        public long latencyStartTime = 0;
        public long latency = 0;

        public void SendHeartbeat()
        {
            MsgHeartbeat msg = protocol.CreateMessage<MsgHeartbeat>();
            latencyStartTime = NetworkTime.Milliseconds();
            msg.timestamp = latencyStartTime;
            protocol.SendReliableMessage(server.GetEndpoint(), msg);
        }
        
        public void OnResponseHeartbeat(object sender, NetworkMessage message)
        {
            MsgHeartbeat msg = (MsgHeartbeat)message;

            latency = NetworkTime.Milliseconds() - msg.responseTimestamp;
            Console.WriteLine("SentTime ["+msg.header.peer+"] Ping = " + (latency) + " ms");
            //NetworkConfig.SocketReliableRetryDelay = Math.Max(100, latency * 2);
            //Random r = new Random();
            //string username = "0123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789";
            //string username = "JoeOfTexJoeOfTexJoeOfTexJoeOfTexJoeOfTexJoeOfTexJoeOfTexJoeOfTexJoeOfTexJoeOfTexJoeOfTexJoeOfTexJoeOfTexJoeOfTexJoeOfTex";
            //string username = "JoeOfTex";
            //for (int x = 0; x < MAXCLIENTS; x++)
            {
                //for (int i = 0; i < NetworkConfig.MAXSEND; i++)
                {
                    //username += "JoeOfTex" + r.Next(1000, 100000) + r.Next(1000, 100000) + r.Next(1000, 100000);
                    //ConnectToServer(username);
                }
            }
        }
        
        public void PerformanceTest()
        {
            if (receiveCnt == 0)
                timer = Stopwatch.StartNew();

            //Interlocked.Increment(ref receiveCnt);
            receiveCnt++;

            if (receiveCnt % 500 == 0 || receiveCnt == NetworkConfig.MAXSEND)
            {
                //timer.Stop();
                Console.WriteLine("CLIENT Finished " + receiveCnt + " packets in " + ((float)timer.ElapsedMilliseconds / 1000f) + " seconds");
            }
        }
    }
}
