using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace OpenP2P
{
    public class NetworkServer : NetworkProtocol
    {
        //public NetworkProtocol protocol = null;
        public Dictionary<string, string> connections = new Dictionary<string, string>();
        public int receiveCnt = 0;
        static Stopwatch recieveTimer;

        public NetworkServer(int localPort, bool _isServer) : base(localPort, true)
        {
            //protocol = new NetworkProtocol(localIP, localPort, true);
            AttachRequestListener(ChannelType.Server, OnRequestConnectToServer);
            AttachResponseListener(ChannelType.Server, OnResponseConnectToServer);

            //protocol.AttachMessageListener(ChannelType.Heartbeat, OnMessageHeartbeat);
        }
        
        private void OnResponseConnectToServer(object sender, NetworkMessage e)
        {
            PerformanceTest();

            NetworkPacket packet = (NetworkPacket)sender;
        }

        public void OnRequestConnectToServer(object sender, NetworkMessage message)
        {
            PerformanceTest();

            MessageServer msgConnect = (MessageServer)message;
            //Console.WriteLine("Int: " + msgConnect.msgNumber);

            //NetworkPacket packet = (NetworkPacket)sender;
            //string path = Directory.GetCurrentDirectory();
            //path = Path.Combine(path + "/../../ipsum.txt");
            //string text = File.ReadAllText(path);
            //byte[] bytes = Encoding.ASCII.GetBytes(text);

            //MessageStream dataStream = protocol.CreateMessage<MessageStream>();
            //dataStream.SetBuffer(bytes);

            //Console.WriteLine("User Connected, sending ipsum.txt of " + bytes.Length);
            //SendStream(message.header.source, dataStream);

            MessageServer responseMsg = CreateMessage<MessageServer>();
           
            responseMsg.response.method = msgConnect.request.method;
            switch(msgConnect.request.method)
            {
                case MessageServer.ServerMethod.CONNECT:
                    responseMsg.response.connect.connected = true;
                    responseMsg.response.connect.sendRate = NetworkConfig.ThreadSendSleepPacketSizePerFrame;
                    Console.WriteLine("Setting send rate: {0}", responseMsg.response.connect.sendRate);
                    break;
                case MessageServer.ServerMethod.HEARTBEAT:

                    break;
            }
            
            SendResponse(msgConnect, responseMsg);
            
        }


        public void PerformanceTest()
        {
            if (receiveCnt == 0)
                recieveTimer = Stopwatch.StartNew();

            receiveCnt++;

            if (receiveCnt % 10 == 0 || receiveCnt == NetworkConfig.MAXSEND)
            {
                //recieveTimer.Stop();
                Console.WriteLine("SERVER Finished in " + receiveCnt + " packets in " + ((float)recieveTimer.ElapsedMilliseconds / 1000f) + " seconds");
                NetworkConfig.ProfileReportAll();
            }
        }
    }
}
