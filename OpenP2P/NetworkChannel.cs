using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenP2P
{
    public enum ChannelType
    {
        Invalid,

        ConnectToServer,
        ConnectToPeer,
        DisconnectFromServer,
        DisconnectFromPeer,
        //DisconnectFromServer,

        //interest mapping data sent to server
        //Peers will be connected together at higher priorities based on the 
        // "interest" mapping to a QuadTree (x, y, width, height) 
        Heartbeat,

        Raw,
        Event,
        RPC,

        //GetPeers,
        //ConnectTo,
        LAST
    }

    

    public class NetworkChannel
    {
        

        public static Dictionary<uint, Func<NetworkMessage>> constructors = new Dictionary<uint, Func<NetworkMessage>>()
        {
            {(uint)ChannelType.Invalid, () => new MsgInvalid()},
            {(uint)ChannelType.ConnectToServer, () => new MsgConnectToServer()},
            {(uint)ChannelType.ConnectToPeer, () => new MsgInvalid()},
            {(uint)ChannelType.DisconnectFromServer, () => new MsgInvalid()},
            {(uint)ChannelType.DisconnectFromPeer, () => new MsgInvalid()},
            {(uint)ChannelType.Heartbeat, () => new MsgHeartbeat()},
            {(uint)ChannelType.Raw, () => new MsgInvalid()},
            {(uint)ChannelType.Event, () => new MsgInvalid()},
            {(uint)ChannelType.RPC, () => new MsgInvalid()},
            {(uint)ChannelType.LAST, () => new MsgInvalid()},
            //{2, () => new ClassB()}
            // more of the same
        };

        public ChannelType channelType = ChannelType.Invalid;

        public event EventHandler<NetworkMessage> OnChannelMessage = null;
        public event EventHandler<NetworkMessage> OnChannelResponse = null;

        public virtual void InvokeChannelEvent(NetworkPacket packet, NetworkMessage message)
        {
            switch (message.header.sendType)
            {
                case SendType.Message:
                    if (OnChannelMessage != null)
                        OnChannelMessage.Invoke(packet, message);
                    break;
                case SendType.Response:
                    if (OnChannelResponse != null)
                        OnChannelResponse.Invoke(packet, message);
                    break;
            }
        }

        public static NetworkMessage CreateMessage(ChannelType type)
        {
            NetworkMessage message = null;
            


            return message;
        }
    }
}
