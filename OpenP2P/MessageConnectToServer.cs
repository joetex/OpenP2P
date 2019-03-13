using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenP2P
{
    class MessageConnectToServer : INetworkMessage
    {
        public string userName = "";



        public void Request(NetworkStream stream)
        {
            //stream.WriteHeader(Message.ConnectToServer, false);
        }

        public void Response(NetworkStream stream)
        {
            //stream.WriteHeader(Message.ConnectToServer, true);
        }

        public void OnReceive(NetworkStream stream)
        {
           
        }

        public void Write(NetworkStream stream)
        {
            
        }
    }
}
