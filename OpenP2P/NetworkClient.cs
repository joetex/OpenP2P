
using System;
using System.Collections.Generic;
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

        public NetworkClient(string remoteHost, int remotePort)
        {
            Setup(remoteHost, remotePort, 9001);
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
            protocol.AttachResponseListener(Message.ConnectToServer, OnResponseConnectToServer);
            protocol.Listen();
        }


        public void ConnectToServer(string userName)
        {
            MsgConnectToServer msg = (MsgConnectToServer)protocol.Create(Message.ConnectToServer);
            msg.requestUsername = userName;
            Console.WriteLine("Sending Request: ");
            Console.WriteLine(userName);

            protocol.SendRequest(protocol.socket.remote, msg);
        }

        public void OnResponseConnectToServer(object sender, NetworkMessage message)
        {
            receiveCnt++;
            MsgConnectToServer connectMsg = (MsgConnectToServer)message;
            Console.WriteLine("Received Response:");
            Console.WriteLine(connectMsg.responseConnected);
        }


    }
}
