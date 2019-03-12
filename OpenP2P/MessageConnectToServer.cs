using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenP2P
{
    class MessageConnectToServer : IMessage
    {
        public void Request(NetworkStream stream)
        {
            stream.WriteHeader(Message.ConnectToServer, false);
        }

        public void Response(NetworkStream stream)
        {
            stream.WriteHeader(Message.ConnectToServer, true);
        }

        public void OnReceive(NetworkStream stream)
        {
            throw new NotImplementedException();
        }

        public void Write(NetworkStream stream)
        {
            throw new NotImplementedException();
        }
    }
}
