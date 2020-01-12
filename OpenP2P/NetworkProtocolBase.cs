using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace OpenP2P
{
    public class NetworkProtocolBase
    {
        public NetworkChannel channel = null;
        public NetworkSocket socket = null;
        public NetworkIdentity ident = null;

        public NetworkProtocolBase() { }
        
        public virtual void WriteHeader(NetworkPacket packet, NetworkMessage message) { }
        public virtual NetworkMessage ReadHeader(NetworkPacket packet) { return null; }
        public virtual NetworkMessage[] ReadHeaders(NetworkPacket packet) { return null; }
        public virtual void OnReceive(object sender, NetworkPacket packet) { }
        public virtual void OnSend(object sender, NetworkPacket packet) { }
        public virtual void OnError(object sender, NetworkPacket packet) { }
        
        public virtual IPEndPoint GetIPv6(EndPoint ep)
        {
            IPEndPoint ip = (IPEndPoint)ep;
            if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
                return ip;
            //ip = new IPEndPoint(ip.Address.MapToIPv6(), ip.Port);
            return ip;
        }

        public virtual IPEndPoint GetEndPoint(string ip, int port)
        {
            IPAddress address = null;
            if (IPAddress.TryParse(ip, out address))
                return new IPEndPoint(address, port);
            return null;
        }

        public virtual void AttachSocketListener(NetworkSocket _socket)
        {
            socket = _socket;
            socket.OnReceive += OnReceive;
            socket.OnSend += OnSend;
            socket.OnError += OnError;
        }

        public virtual void AttachMessageListener(ChannelType msgType, EventHandler<NetworkMessage> func)
        {
            GetChannelEvent((uint)msgType).OnChannelMessage += func;
        }

        public virtual void AttachResponseListener(ChannelType msgType, EventHandler<NetworkMessage> func)
        {
            GetChannelEvent((uint)msgType).OnChannelResponse += func;
        }

        public virtual void AttachErrorListener(NetworkErrorType errorType, EventHandler<NetworkPacket> func)
        {
           
        }

        public static class New<T> where T : new()
        {
            public static readonly Func<T> Instance = Expression.Lambda<Func<T>>
                                                      (
                                                       Expression.New(typeof(T))
                                                      ).Compile();
        }
        

        public T CreateMessage<T>() where T : INetworkMessage, new()
        {
            return (T)channel.CreateMessage<T>();
        }
        
        public T Create<T>() where T : INetworkMessage, new()
        {
            return (T)channel.CreateMessage<T>();
        }


        public virtual NetworkChannelEvent GetChannelEvent(ChannelType type)
        {
            return GetChannelEvent((uint)type);
        }

        public virtual NetworkChannelEvent GetChannelEvent(uint id)
        {
            if (!channel.channels.ContainsKey(id))
                return channel.channels[(int)ChannelType.Invalid];
            return channel.channels[id];
        }
    }
}
