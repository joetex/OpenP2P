using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
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
            protocol.AttachMessageListener(ChannelType.Server, OnMessageConnectToServer);
            protocol.AttachResponseListener(ChannelType.Server, OnResponseConnectToServer);

            protocol.AttachMessageListener(ChannelType.Heartbeat, OnMessageHeartbeat);
        }

        

        public void OnMessageHeartbeat(object sender, NetworkMessage message)
        {
            MessageHeartbeat heartbeat = (MessageHeartbeat)message;
            MessageHeartbeat response = protocol.CreateMessage<MessageHeartbeat>();
            response.responseTimestamp = heartbeat.timestamp;
            protocol.SendResponse(heartbeat, response);
            Console.WriteLine("Received Heartbeat from ("+ heartbeat.header.peer.id +") :");
            //Console.WriteLine(heartbeat.timestamp);

            /*for (int i = 0; i < NetworkConfig.MAXSEND; i++)
            {
                //username += "JoeOfTex" + r.Next(1000, 100000) + r.Next(1000, 100000) + r.Next(1000, 100000);
                MsgConnectToServer tmp = protocol.Create<MsgConnectToServer>();
                tmp.msgUsername = "0123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789";
                tmp.msgNumber = 10;
                tmp.msgShort = 20;
                tmp.msgBool = true;

                protocol.SendReliableMessage(message.header.source, tmp);
            }*/
        }

        private void OnResponseConnectToServer(object sender, NetworkMessage e)
        {
            PerformanceTest();

            NetworkPacket packet = (NetworkPacket)sender;
        }

        public void OnMessageConnectToServer(object sender, NetworkMessage message)
        {
            PerformanceTest();

            MessageServer msgConnect = (MessageServer)message;
            Console.WriteLine("Int: " + msgConnect.msgNumber);
            
            NetworkPacket packet = (NetworkPacket)sender;
            string path = Directory.GetCurrentDirectory();
            path = Path.Combine(path + "/../../ipsum.txt");
            string text = File.ReadAllText(path);
            byte[] bytes = Encoding.ASCII.GetBytes(text);

            MessageStream dataStream = protocol.CreateMessage<MessageStream>();
            dataStream.SetBuffer(bytes);

            Console.WriteLine("User Connected, sending ipsum.txt of " + bytes.Length);
            protocol.SendStream(message.header.source, dataStream);
        }


        public void PerformanceTest()
        {
            if (receiveCnt == 0)
                recieveTimer = Stopwatch.StartNew();

            receiveCnt++;

            if (receiveCnt % 500 == 0 || receiveCnt == NetworkConfig.MAXSEND)
            {
                //recieveTimer.Stop();
                Console.WriteLine("SERVER Finished in " + receiveCnt + " packets in " + ((float)recieveTimer.ElapsedMilliseconds / 1000f) + " seconds");
                NetworkConfig.ProfileReportAll();
            }
        }
    }
}
