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
            protocol.AttachRequestListener(Message.ConnectToServer, OnRequestConnectToServer);
            protocol.Listen();
        }
    

        public void OnRequestConnectToServer(object sender, NetworkMessage message)
        {
            receiveCnt++;
            NetworkStream stream = (NetworkStream)sender;
         
            MsgConnectToServer connectMsg = (MsgConnectToServer)message;
            Console.WriteLine("Received Request:");
            Console.WriteLine(connectMsg.requestUsername);

            connectMsg.responseConnected = true;

            Console.WriteLine("Sending Response: True");
            protocol.SendResponse(stream.remoteEndPoint, connectMsg);
        }
    }
}
