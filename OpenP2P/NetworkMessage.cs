using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenP2P
{
    public class NetworkMessage
    {
        public class Header
        {
            public bool isReliable = false;
            public bool isLittleEndian = true;
            public SendType sendType = 0;
            public MessageChannel messageChannel = MessageChannel.Invalid;
            public ushort sequence = 0;
            public ushort id = 0;
            public NetworkIdentity.PeerIdentity peer = new NetworkIdentity.PeerIdentity();
        }

        
        public MessageChannel messageChannel = MessageChannel.Invalid;

        public event EventHandler<NetworkMessage> OnMessage = null;
        public event EventHandler<NetworkMessage> OnResponse = null;
        
        public virtual void WriteMessage(NetworkPacket packet) { }
        public virtual void WriteResponse(NetworkPacket packet) { }
        public virtual void ReadMessage(NetworkPacket packet) { }
        public virtual void ReadResponse(NetworkPacket packet) { }

        public virtual void InvokeOnRead(NetworkPacket packet)
        {
            switch (packet.header.sendType)
            {
                case SendType.Message:
                    if (OnMessage != null)
                        OnMessage.Invoke(packet, this);
                    break;
                case SendType.Response:
                    if (OnResponse != null)
                        OnResponse.Invoke(packet, this);
                    break;
            }
        }
    }
}
