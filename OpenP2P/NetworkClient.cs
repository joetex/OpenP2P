
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
            AttachResponseListener(ChannelType.Server, OnResponseServer);
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

     
        public void OnErrorReliableFailed(object sender, NetworkPacket packet)
        {
            //Console.WriteLine("[ERROR] " + packet.lastErrorType.ToString() + ": " + packet.lastErrorMessage);
        }

        public void AddServer(string remoteHost, int remotePort)
        {
            IPEndPoint serverHost = GetEndPoint(remoteHost, remotePort);
            server = new NetworkPeer(this);
            server.AddEndpoint(serverHost);
        }

        public override MessageServer ConnectToServer(string userName)
        {
            MessageServer message = base.ConnectToServer(userName);
           
            //SendMessage(server.GetEndpoint(), message);
            latencyStartTime = NetworkTime.Milliseconds();

            SendReliableMessage(server.GetEndpoint(), message);
            return message;
        }

        public void ConnectToSTUN()
        {
            stun.ConnectSTUN(true);
            //stun.ConnectTURN(null, true);
        }

        

        public void OnResponseServer(object sender, NetworkMessage message)
        {
            NetworkPacket packet = (NetworkPacket)sender;
            PerformanceTest();

            MessageServer msgServer = (MessageServer)message;

            switch(msgServer.method)
            {
                case MessageServer.ServerMethod.CONNECT:
                    Console.WriteLine("Server SendRate (BytesPerFrame) = " + msgServer.response.connect.sendRate);

                    for(int i=0; i<NetworkConfig.MAXSEND; i++)
                    {
                        SendHeartbeat();
                    }
                    
                    break;
                case MessageServer.ServerMethod.HEARTBEAT:

                    break;
            }
           
            CalculateLatency();
        }

        public void CalculateLatency()
        {
            latency = NetworkTime.Milliseconds() - latencyStartTime;
           // Console.WriteLine("Ping = " + (latency) + " ms");
            latencyStartTime = NetworkTime.Milliseconds();
        }

        public long latencyStartTime = 0;
        public long latency = 0;
        
        public void SendHeartbeat()
        {
            MessageServer msg = CreateMessage<MessageServer>();
            msg.method = MessageServer.ServerMethod.HEARTBEAT;
            latencyStartTime = NetworkTime.Milliseconds();
            SendMessage(server.GetEndpoint(), msg);
        }
        /*
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
