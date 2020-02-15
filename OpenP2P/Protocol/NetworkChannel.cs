using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace OpenP2P
{
    public enum ChannelType
    {
        Invalid,

        Server,
        Peer,
        //interest mapping data sent to server
        //Peers will be connected together at higher priorities based on the 
        // "interest" mapping to a QuadTree (x, y, width, height) 
        Stream, //used for large data transfer
        STUN,
        Event,
        RPC,
        LAST
    }

    
    public class NetworkChannel
    {
        public Dictionary<uint, Func<NetworkMessage>> constructors = new Dictionary<uint, Func<NetworkMessage>>()
        {
            {(uint)ChannelType.Invalid, Create<MessageInvalid> },
            {(uint)ChannelType.Server, Create<MessageServer>},
            {(uint)ChannelType.Peer, Create<MessageInvalid> },
            {(uint)ChannelType.Stream, Create<MessageStream> },
            {(uint)ChannelType.STUN, Create<MessageSTUN> },
            {(uint)ChannelType.Event, Create<MessageInvalid>},
            {(uint)ChannelType.RPC, Create<MessageInvalid>},
            {(uint)ChannelType.LAST, Create<MessageInvalid>}
        };

        public NetworkMessagePool MESSAGEPOOL = null;

        public Dictionary<Type, ChannelType> messageToChannelType = new Dictionary<Type, ChannelType>();
        public Dictionary<ChannelType, Type> channelTypeToMessage = new Dictionary<ChannelType, Type>();
        public Dictionary<uint, NetworkChannelEvent> channels = new Dictionary<uint, NetworkChannelEvent>();

        public NetworkChannel() {
            SetupNetworkChannels();

            MESSAGEPOOL = new NetworkMessagePool(this, NetworkConfig.MessagePoolInitialCount);
        }
        /// <summary>
        /// Setup Network Channels
        /// Cache channels to a dictionary for fast access
        /// </summary>
        public void SetupNetworkChannels()
        {
            string enumName = "";
            NetworkChannelEvent channelEvent = null;
            for (uint i = 0; i < (uint)ChannelType.LAST; i++)
            {
                //these are used for channel events
                channelEvent = new NetworkChannelEvent();
                channelEvent.channelType = (ChannelType)i;
                channels.Add(i, channelEvent);

                try
                {
                    //these are used to map message object types to channel types
                    enumName = Enum.GetName(typeof(ChannelType), (ChannelType)i);
                    NetworkMessage message = (NetworkMessage)GetInstance("OpenP2P.Message" + enumName);
                    Type t = message.GetType();
                    messageToChannelType.Add(message.GetType(), (ChannelType)i);
                    channelTypeToMessage.Add((ChannelType)i, message.GetType());
                }
                catch (Exception e)
                {
                    //Console.WriteLine(e.ToString());
                }

            }
        }

        public NetworkMessage InstantiateMessage(ChannelType type)
        {
            //return (NetworkMessage)Activator.CreateInstance(channelTypeToMessage[type]);
            return constructors[(uint)type]();
        }

        public INetworkMessage CreateMessage(ChannelType type)
        {
            NetworkMessage message = (NetworkMessage)MESSAGEPOOL.Reserve(type);
            message.header.channelType = type;
            return message;
        }

        public INetworkMessage CreateMessage(uint id)
        {
            NetworkMessage message = (NetworkMessage)MESSAGEPOOL.Reserve((ChannelType)id);
            message.header.channelType = (ChannelType)id;
            return message;
        }

        public INetworkMessage CreateMessage<T>()
        {
            ChannelType ct = messageToChannelType[typeof(T)];
            NetworkMessage message = (NetworkMessage)MESSAGEPOOL.Reserve(ct);
            message.header.channelType = (ChannelType)ct;
            return message;
        }

        public void FreeMessage(NetworkMessage message)
        {
            MESSAGEPOOL.Free(message);
        }

        public ChannelType GetChannelType(NetworkMessage msg)
        {
            Type type = msg.GetType();
            return messageToChannelType[type];
        }

        public object GetInstance(string strFullyQualifiedName)
        {
            Type t = Type.GetType(strFullyQualifiedName);
            return Activator.CreateInstance(t);
        }

        public static class New<T> where T : new()
        {
            public static readonly Func<T> Instance = Expression.Lambda<Func<T>>
                                                      (
                                                       Expression.New(typeof(T))
                                                      ).Compile();
        }

        public static NetworkMessage Create<T>() where T : INetworkMessage, new()
        {
            INetworkMessage obj = New<T>.Instance();
            return (NetworkMessage)obj;
        }

    }
}
