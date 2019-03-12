using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenP2P
{
    class MessageHeartbeat : IMessage
    {
        public void Request(NetworkStream stream)
        {
            stream.WriteHeader(Message.Heartbeat, true);
        }
        
        public void Response(NetworkStream stream)
        {
            stream.WriteHeader(Message.Heartbeat, false);
        }

        public void OnReceive(NetworkStream stream)
        {
            
        }

        public void Write(NetworkStream stream)
        {
        }
    }
}
