
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
        
        public NetworkClient(string remoteHost, int remotePort)
        {
            Setup(remoteHost, remotePort, 0);
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
            protocol.AttachMessageListener(Message.ConnectToServer, OnConnectToServer);
            protocol.Listen();
        }


        public void ConnectToServer(string userName)
        {
            MsgConnectToServer msg = (MsgConnectToServer)protocol.Begin(Message.ConnectToServer);
            msg.userName = userName;

            protocol.Send(msg);
        }

        public void OnConnectToServer(object sender, NetworkMessage message)
        {
            MsgConnectToServer connectMsg = (MsgConnectToServer)message;
            Console.WriteLine("Received message from client:");
            Console.WriteLine(connectMsg.userName);
        }


    }
}
