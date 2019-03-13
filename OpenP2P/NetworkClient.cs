
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
        public NetworkSocket socket = null;
        public NetworkProtocol protocol = new NetworkProtocol();

        public NetworkClient(string remoteHost, int remotePort, int localPort)
        {
            Setup(remoteHost, remotePort, localPort);
        }

        public NetworkClient(string remoteHost, int remotePort)
        {
            Setup(remoteHost, remotePort, 0);
        }

        public NetworkClient(int localPort)
        {
            Setup("127.0.0.1", 0, localPort);
        }

        /**
         * Setup the connection credentials and socket configuration
         */
        public void Setup(string remoteHost, int remotePort, int localPort)
        {
            socket = new NetworkSocket(remoteHost, remotePort, localPort);
            socket.OnReceive += protocol.OnReceive;
        }


        public void ConnectToServer(string userName)
        {
            
        }

        public void OnConnectToServer(NetworkStream stream)
        {

        }

    }
}
