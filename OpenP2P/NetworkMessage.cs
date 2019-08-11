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
            public MessageType messageType = MessageType.Invalid;
            public ushort sequence = 0;
            public ushort id = 0;
        }

        public NetworkIdentity.PeerIdentity peer = new NetworkIdentity.PeerIdentity();
        public MessageType messageType = MessageType.Invalid;

        public event EventHandler<NetworkMessage> OnMessage = null;
        public event EventHandler<NetworkMessage> OnResponse = null;

        public virtual void WriteMessage(NetworkStream stream) { }
        public virtual void WriteResponse(NetworkStream stream) { }
        public virtual void ReadMessage(NetworkStream stream) { }
        public virtual void ReadResponse(NetworkStream stream) { }

        public virtual void InvokeOnRead(NetworkStream stream)
        {
            switch (stream.header.sendType)
            {
                case SendType.Message:
                    ReadMessage(stream);
                    if (OnMessage != null)
                        OnMessage.Invoke(stream, this);
                    break;
                case SendType.Response:
                    ReadResponse(stream);
                    if (OnResponse != null)
                        OnResponse.Invoke(stream, this);
                    break;
            }
        }
    }
}
