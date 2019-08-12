using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace OpenP2P
{
    class NetworkMessageFactory<T>
    {

        public Dictionary<Type, ChannelType> messageTypes = new Dictionary<Type, ChannelType>();

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

        public static class New<T> where T : new()
        {
            public static readonly Func<T> Instance = Expression.Lambda<Func<T>>
                                                      (
                                                       Expression.New(typeof(T))
                                                      ).Compile();
        }

        public T Create<T>() where T : new()
        {
            T obj = New<T>.Instance();
            return obj;
        }

        public NetworkMessage CreateMessage<T>()
        {
            ChannelType type = messageTypes[typeof(T)];
            NetworkMessage obj = constructors[(uint)type]();
            return obj;
        }
        
    }
}
