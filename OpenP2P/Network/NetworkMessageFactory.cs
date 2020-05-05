using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace OpenP2P
{
    
    public partial class NetworkMessageFactory
    {
        public NetworkMessagePool MESSAGEPOOL = null;

        public Dictionary<Type, MessageType> messageToMessageType = new Dictionary<Type, MessageType>();
        public Dictionary<MessageType, Type> messageTypeToMessage = new Dictionary<MessageType, Type>();
        public Dictionary<uint, NetworkMessageEvent> messageEvents = new Dictionary<uint, NetworkMessageEvent>();

        public NetworkMessageFactory() {
            SetupMessageTypes();

            MESSAGEPOOL = new NetworkMessagePool(this, NetworkConfig.MessagePoolInitialCount);
        }
        /// <summary>
        /// Setup Network Channels
        /// Cache channels to a dictionary for fast access
        /// </summary>
        public void SetupMessageTypes()
        {
            string enumName = "";
            NetworkMessageEvent messageEvent = null;
            for (uint i = 0; i < (uint)MessageType.LAST; i++)
            {
                //these are used for channel events
                messageEvent = new NetworkMessageEvent();
                messageEvent.messageType = (MessageType)i;
                messageEvents.Add(i, messageEvent);

                try
                {
                    //these are used to map message object types to channel types
                    enumName = Enum.GetName(typeof(MessageType), (MessageType)i);
                    NetworkMessage message = (NetworkMessage)GetInstance("OpenP2P.Message" + enumName);
                    Type t = message.GetType();
                    messageToMessageType.Add(message.GetType(), (MessageType)i);
                    messageTypeToMessage.Add((MessageType)i, message.GetType());
                }
                catch (Exception e)
                {
                    //Console.WriteLine(e.ToString());
                }

            }
        }

        public NetworkMessage InstantiateMessage(MessageType type)
        {
            //return (NetworkMessage)Activator.CreateInstance(channelTypeToMessage[type]);
            return constructors[(uint)type]();
        }

        public INetworkMessage CreateMessage(MessageType type)
        {
            NetworkMessage message = (NetworkMessage)MESSAGEPOOL.Reserve(type);
            message.header.channelType = type;
            return message;
        }

        public INetworkMessage CreateMessage(uint id)
        {
            NetworkMessage message = (NetworkMessage)MESSAGEPOOL.Reserve((MessageType)id);
            message.header.channelType = (MessageType)id;
            return message;
        }

        public INetworkMessage CreateMessage<T>()
        {
            MessageType ct = messageToMessageType[typeof(T)];
            NetworkMessage message = (NetworkMessage)MESSAGEPOOL.Reserve(ct);
            message.header.channelType = (MessageType)ct;
            return message;
        }

        public void FreeMessage(NetworkMessage message)
        {
            MESSAGEPOOL.Free(message);
        }

        public MessageType GetMessageType(NetworkMessage msg)
        {
            Type type = msg.GetType();
            return messageToMessageType[type];
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
