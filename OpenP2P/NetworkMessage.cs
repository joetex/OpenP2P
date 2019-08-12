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
            public ChannelType channelType = ChannelType.Invalid;
            public ushort sequence = 0;
            public ushort id = 0;
            public NetworkIdentity.PeerIdentity peer = new NetworkIdentity.PeerIdentity();
        }

        public Header header = new Header();
        public ChannelType channelType = ChannelType.Invalid;

        public virtual void WriteMessage(NetworkPacket packet) { }
        public virtual void WriteResponse(NetworkPacket packet) { }
        public virtual void ReadMessage(NetworkPacket packet) { }
        public virtual void ReadResponse(NetworkPacket packet) { }
    }
}
