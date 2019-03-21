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
            public MessageType messageType = MessageType.NULL;
            public uint sequence = 0;
        }

        public Header header = new Header();

        public event EventHandler<NetworkMessage> OnRequest = null;
        public event EventHandler<NetworkMessage> OnResponse = null;

        public virtual void WriteRequest(NetworkStream stream) { }
        public virtual void WriteResponse(NetworkStream stream) { }
        public virtual void ReadRequest(NetworkStream stream) { }
        public virtual void ReadResponse(NetworkStream stream) { }

        public virtual void InvokeOnRead(NetworkStream stream)
        {
            switch (header.sendType)
            {
                case SendType.Request:
                    ReadRequest(stream);
                    if (OnRequest != null)
                        OnRequest.Invoke(stream, this);
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
