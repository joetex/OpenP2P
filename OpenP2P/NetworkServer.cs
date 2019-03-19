using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenP2P
{
    public class NetworkServer
    {
        public NetworkProtocol protocol = null;
        public Dictionary<string, string> connections = new Dictionary<string, string>();
        public int receiveCnt = 0;

        public NetworkServer(int localPort)
        {
            Setup(localPort);
        }

        public void Setup(int localPort)
        {
            protocol = new NetworkProtocol("127.0.0.1", 0, localPort);
            AttachListeners();
        }

        public void AttachListeners()
        {
            protocol.AttachMessageListener(Message.ConnectToServer, OnConnectToServer);
            protocol.Listen();
        }
    

        public void OnConnectToServer(object sender, NetworkMessage message)
        {
            receiveCnt++;
            MsgConnectToServer connectMsg = (MsgConnectToServer)message;
            //Console.WriteLine("Received message from client:");
            //Console.WriteLine(connectMsg.userName);
        }
    }
}
