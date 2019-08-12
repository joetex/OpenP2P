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
        //interest mapping data sent to server
        //Peers will be connected together at higher priorities based on the 
        // "interest" mapping to a QuadTree (x, y, width, height) 
        Heartbeat,
        Raw,
        Event,
        RPC,
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

        };

        public static NetworkMessagePool MESSAGEPOOL = new NetworkMessagePool(NetworkConfig.MessagePoolInitialCount);

        public ChannelType channelType = ChannelType.Invalid;

        public event EventHandler<NetworkMessage> OnChannelMessage = null;
        public event EventHandler<NetworkMessage> OnChannelResponse = null;


        public static NetworkMessage CreateMessage(ChannelType type)
        {
            return MESSAGEPOOL.Reserve(type);
            //return constructors[(uint)type]();
        }

        public static NetworkMessage CreateMessage(uint type)
        {
            return MESSAGEPOOL.Reserve((ChannelType)type);
            //return constructors[type]();
        }

        public static void FreeMessage(NetworkMessage message)
        {
            MESSAGEPOOL.Free(message);
        }

        public virtual void InvokeEvent(NetworkPacket packet, NetworkMessage message)
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
    }
}
