using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenP2P
{
    public class NetworkChannelEvent
    {
        public ChannelType channelType = ChannelType.Invalid;

        public event EventHandler<NetworkMessage> OnChannelMessage = null;
        public event EventHandler<NetworkMessage> OnChannelResponse = null;
        public event EventHandler<NetworkMessage> OnChannelStream = null;
        public NetworkChannelEvent()
        {
        }

        public virtual void InvokeEvent(NetworkPacket packet, NetworkMessage message)
        {
            switch (message.header.sendType)
            {
                case SendType.Message:
                    if (message.header.isStream)
                    {
                        if (OnChannelStream != null)
                            OnChannelStream.Invoke(packet, message);
                    }
                    else
                    {
                        if (OnChannelMessage != null)
                            OnChannelMessage.Invoke(packet, message);
                    }
                    break;
                case SendType.Response:
                    if (OnChannelResponse != null)
                        OnChannelResponse.Invoke(packet, message);
                    break;
            }
        }

    }


}
