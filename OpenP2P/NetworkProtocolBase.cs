using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenP2P
{
    class NetworkProtocolBase
    {
        public Dictionary<int, INetworkMessage> messages = new Dictionary<int, INetworkMessage>();

        public NetworkProtocolBase()
        {
        }

        public void OnReceive(object sender, NetworkStream stream)
        {
            //Message msg = stream.ReadHeader();
            //messages[msg].OnReceive(stream);
        }

        public void OnSend(object sender, NetworkStream stream)
        {
            //messages[msg].OnReceive(stream);
        }
    }
}
