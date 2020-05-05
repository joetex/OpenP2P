using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenP2P
{
    public class NetworkMessageEvent
    {
        public MessageType messageType = MessageType.Invalid;

        public event EventHandler<NetworkMessage> OnMessageRequest = null;
        public event EventHandler<NetworkMessage> OnMessageResponse = null;
        public event EventHandler<NetworkMessage> OnMessageStream = null;
        public NetworkMessageEvent()
        {
        }

        public virtual void InvokeEvent(NetworkPacket packet, NetworkMessage message)
        {
            switch (message.header.sendType)
            {
                case SendType.Message:
                    if (message.header.isStream)
                    {
                        if (OnMessageStream != null)
                            OnMessageStream.Invoke(packet, message);
                    }
                    else
                    {
                        if (OnMessageRequest != null)
                            OnMessageRequest.Invoke(packet, message);
                    }
                    break;
                case SendType.Response:
                    if (OnMessageResponse != null)
                        OnMessageResponse.Invoke(packet, message);
                    break;
            }
        }

    }


}
