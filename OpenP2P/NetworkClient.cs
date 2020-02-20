
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
    public partial class NetworkClient : NetworkProtocol
    {
        //public NetworkProtocol protocol = null;

        public IPEndPoint serverHost = null;
        public NetworkPeer server = null;
        public Thread mainThread = null;

        
        public STUNClient stun = null;
        //public TURNClient turn = null;

        public int receiveCnt = 0;
        Stopwatch timer;
        Dictionary<uint, Stopwatch> recieveTimer = new Dictionary<uint, Stopwatch>();
        public NetworkClient() : base(false)
        {
            //protocol = new NetworkProtocol(localPort, false);
            AttachResponseListener(ChannelType.Server, OnResponseConnectToServer);
            //protocol.AttachMessageListener(ChannelType.DataContent, OnStreamDataContent);
            //protocol.AttachResponseListener(ChannelType.Heartbeat, OnResponseHeartbeat);
            AttachStreamListener(ChannelType.Stream, OnStreamDataContent);
           // protocol.AttachResponseListener(ChannelType.DataContent, OnResponseDataContent);
            AttachErrorListener(NetworkErrorType.ErrorReliableFailed, OnErrorReliableFailed);

            
            stun = new STUNClient(this);
            //turn = new TURNClient(protocol);

            
        }

        private void OnStreamDataContent(object sender, NetworkMessage e)
        {
            NetworkMessageStream stream = (NetworkMessageStream)e;

            Console.WriteLine("Command: " + stream.command);
            string result = Encoding.UTF8.GetString(stream.byteData);
            latency = NetworkTime.Milliseconds() - latencyStartTime;
            Console.WriteLine("Stream took " + (latency) + " ms");
            Console.WriteLine("Text: " + result);
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
            //Console.WriteLine("[ERROR] " + packet.lastErrorType.ToString() + ": " + packet.lastErrorMessage);
        }

        public void Connect(string remoteHost, int remotePort, string userName)
        {
            IPEndPoint serverHost = GetEndPoint(remoteHost, remotePort);
            server = new NetworkPeer(this);
            server.AddEndpoint(serverHost);

            MessageServer message = base.ConnectToServer(userName);
            message.msgNumber = 10;
            message.msgShort = 20;
            message.msgBool = true;

            SendReliableMessage(server.GetEndpoint(), message);

            latencyStartTime = NetworkTime.Milliseconds();
        }

        public void ConnectToSTUN()
        {
            stun.ConnectSTUN(true);
            //stun.ConnectTURN(null, true);
        }

        

        public void OnResponseConnectToServer(object sender, NetworkMessage message)
        {
            NetworkPacket packet = (NetworkPacket)sender;
            //recieveTimer[message.header.ackkey].Stop();
            //long end = recieveTimer[message.header.ackkey].ElapsedMilliseconds;
            //Console.WriteLine("Ping took: " + end + " milliseconds");
            PerformanceTest();
            //mainThread = new Thread(MainThread);
            //mainThread.Start();

            latency = NetworkTime.Milliseconds() - latencyStartTime;
            //Console.WriteLine("Ping = " + (latency) + " ms");

            latencyStartTime = NetworkTime.Milliseconds();
            //MsgConnectToServer connectMsg = (MsgConnectToServer)message;
        }


        public long latencyStartTime = 0;
        public long latency = 0;
        /*
        public void SendHeartbeat()
        {
            MessageHeartbeat msg = protocol.CreateMessage<MessageHeartbeat>();
            latencyStartTime = NetworkTime.Milliseconds();
            msg.timestamp = latencyStartTime;
            protocol.SendReliableMessage(server.GetEndpoint(), msg);
        }
        
        public void OnResponseHeartbeat(object sender, NetworkMessage message)
        {
            MessageHeartbeat msg = (MessageHeartbeat)message;

            latency = NetworkTime.Milliseconds() - msg.responseTimestamp;
            Console.WriteLine("SentTime ["+msg.header.sequence+"] Ping = " + (latency) + " ms");
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
        */
        public void PerformanceTest()
        {
            if (receiveCnt == 0)
                timer = Stopwatch.StartNew();

            //Interlocked.Increment(ref receiveCnt);
            receiveCnt++;

            if (receiveCnt % 10000 == 0 || receiveCnt == NetworkConfig.MAXSEND)
            {
                //timer.Stop();
                Console.WriteLine("CLIENT Finished " + receiveCnt + " packets in " + ((float)timer.ElapsedMilliseconds / 1000f) + " seconds");
            }
        }
    }
}
