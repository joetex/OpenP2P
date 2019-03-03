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
        public const int SERVERID = 0;

        public Dictionary<int, NetworkSocket> sockets = new Dictionary<int, NetworkSocket>();

        public NetworkClient(string serverHost)
        {
            NetworkSocket toServer = new NetworkSocket(serverHost, 9000);

            sockets.Add(SERVERID, toServer);
        }

        public NetworkSocket server { get { return sockets[SERVERID]; } }

        public void ConnectToServer()
        {
            //NetworkSocketEvent se = server.PrepareSend();
            //NetworkProtocol.TURN.Register(se);
        }


    }
}
