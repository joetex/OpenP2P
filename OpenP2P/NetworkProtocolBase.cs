using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenP2P
{
    public class NetworkProtocolBase
    {
        public Dictionary<int, NetworkMessage> messages = new Dictionary<int, NetworkMessage>();
        public Dictionary<string, MessageType> awaitingResponse = new Dictionary<string, MessageType>();

        public NetworkSocket socket = null;
        
        public int responseType = 0;
        public bool isLittleEndian = false;
        
        public NetworkProtocolBase() { }

        public virtual void AttachSocketListener(NetworkSocket socket) { }
        public virtual void AttachRequestListener(MessageType msgType, EventHandler<NetworkMessage> func) { }
        public virtual void AttachResponseListener(MessageType msgType, EventHandler<NetworkMessage> func) { }

        public virtual NetworkMessage GetMessage(int id) { return null; }

        public virtual void WriteHeader(NetworkStream stream) { }
        public virtual NetworkMessage ReadHeader(NetworkStream stream) { return null; }

        public virtual void OnReceive(object sender, NetworkStream stream) { }
        public virtual void OnSend(object sender, NetworkStream stream) { }

    }
}
