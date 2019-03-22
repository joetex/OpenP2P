
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

        public int receiveCnt = 0;
        static Stopwatch recieveTimer;
        public NetworkClient(string remoteHost, int remotePort, int localPort)
        {
            Setup(remoteHost, remotePort, localPort);
        }
        
        /**
         * Setup the connection credentials and socket configuration
         */
        public void Setup(string remoteHost, int remotePort, int localPort)
        {
            protocol = new NetworkProtocol(remoteHost, remotePort, localPort);
            AttachListeners();
        }

        public void AttachListeners()
        {
            protocol.AttachResponseListener(MessageType.ConnectToServer, OnResponseConnectToServer);
            protocol.Listen();
        }


        public void ConnectToServer(string userName)
        {
            MsgConnectToServer msg = (MsgConnectToServer)protocol.Create(MessageType.ConnectToServer);
            msg.header.isReliable = true;
            msg.requestUsername = userName;
            //Console.WriteLine("Sending Request: ");
            //Console.WriteLine(userName);
            protocol.SendRequest(protocol.socket.remote, msg);
        }

        
        public void OnResponseConnectToServer(object sender, NetworkMessage message)
        {
            PerformanceTest();
            
            MsgConnectToServer connectMsg = (MsgConnectToServer)message;
            Console.WriteLine("Received Response:");
            Console.WriteLine(connectMsg.responseConnected);
            Console.WriteLine(connectMsg.responsePeerId);

            protocol.localIdentity = protocol.ident.RegisterPeer(connectMsg.responsePeerId, protocol.socket.local);
        }

        public void PerformanceTest()
        {
            if (receiveCnt == 0)
                recieveTimer = Stopwatch.StartNew();

            receiveCnt++;

            if (receiveCnt >= Program.MAXSEND)
            {
                recieveTimer.Stop();
                Console.WriteLine("CLIENT Finished in " + ((float)recieveTimer.ElapsedMilliseconds / 1000f) + " seconds");
            }
        }
    }
}
