using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace OpenP2P
{
    public interface INetworkMessage { }
    public interface IMessageHeader { }


    public class MessageHeader
    {
        //unique identifier for a particular peer, controlled by custom protocols
        public ushort id = 0;

        //reliable key for tracking response packets
        public uint ackkey = 0;

        //packet endpoints
        public EndPoint source = null;
        public EndPoint destination = null;
        public NetworkPeer peer = null;
    }

    public class NetworkMessage : INetworkMessage
    {
        //public Header header = new Header();

        public virtual void WriteRequest(NetworkPacket packet) { }
        public virtual void WriteResponse(NetworkPacket packet) { }
        public virtual void ReadRequest(NetworkPacket packet) { }
        public virtual void ReadResponse(NetworkPacket packet) { }

        public virtual void StreamMessage(NetworkPacket packet) {}

    }
}
