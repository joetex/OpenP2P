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
            AttachRequestListener(ChannelType.Server, OnRequestServer);
           
        }
        
     

        public void OnRequestServer(object sender, NetworkMessage message)
        {
            PerformanceTest();

            MessageServer requestMsg = (MessageServer)message;
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
           
            responseMsg.method = requestMsg.method;
            switch(requestMsg.method)
            {
                case MessageServer.ServerMethod.CONNECT:
                    responseMsg.response.connect.connected = true;
                    responseMsg.response.connect.sendRate = NetworkConfig.ThreadSendSleepPacketSizePerFrame;
                    Console.WriteLine("Setting send rate: {0}", responseMsg.response.connect.sendRate);
                    break;
                case MessageServer.ServerMethod.HEARTBEAT:

                    break;
            }
            
            SendResponse(requestMsg, responseMsg);
            
        }


        public void PerformanceTest()
        {
            if (receiveCnt == 0)
                recieveTimer = Stopwatch.StartNew();

            receiveCnt++;

            if (receiveCnt % 1 == 0 || receiveCnt == NetworkConfig.MAXSEND)
            {
                //recieveTimer.Stop();
                Console.WriteLine("SERVER Finished " + receiveCnt + " packets in " + ((float)recieveTimer.ElapsedMilliseconds / 1000f) + " seconds");
                NetworkConfig.ProfileReportAll();
            }
        }
    }
}
