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
    

    public class NetworkProtocol 
    {
        public NetworkManager net;

        public NetworkMessageFactory messageFactory = null;
        //public NetworkMessageFactory channel = null;
        public NetworkSocket socket = null;
        public NetworkIdentity ident = null;

        public bool isClient = false;
        public bool isServer = false;

        public NetworkProtocol(NetworkManager networkManager)
        {
            net = networkManager;
            messageFactory = new NetworkMessageFactory();
        }

        public static class New<T> where T : new()
        {
            public static readonly Func<T> Instance = Expression.Lambda<Func<T>>
                                                      (Expression.New(typeof(T))).Compile();
        }
        
        public T CreateMessage<T>() where T : INetworkMessage, new()
        {
            return (T)messageFactory.CreateMessage<T>();
        }

        public T Create<T>() where T : INetworkMessage, new()
        {
            return (T)messageFactory.CreateMessage<T>();
        }

        public INetworkMessage Create(MessageType ct)
        {
            return messageFactory.CreateMessage(ct);
        }


        public virtual NetworkMessageEvent GetMessageEvent(MessageType type)
        {
            return GetMessageEvent((uint)type);
        }

        public virtual NetworkMessageEvent GetMessageEvent(uint id)
        {
            if (!messageFactory.messageEvents.ContainsKey(id))
                return messageFactory.messageEvents[(int)MessageType.Invalid];
            return messageFactory.messageEvents[id];
        }


        public virtual void Send(NetworkPacket packet)
        {
            net.SendPacket(packet);
        }

        public virtual void Send(NetworkPacket packet, string policy)
        {

        }

        public virtual void OnSocketSend(NetworkPacket packet)
        {

        }

        public virtual void OnSocketReliable(NetworkPacket packet)
        {

        }

        public virtual void OnSocketError(NetworkPacket packet)
        {

        }

        public virtual void OnSocketReceive(NetworkPacket packet)
        {

        }



        //public virtual void WriteHeader(NetworkPacket packet, NetworkMessage message) { }
        //public virtual NetworkMessage ReadHeader(NetworkPacket packet) { return null; }
        //public virtual NetworkMessage[] ReadHeaders(NetworkPacket packet) { return null; }
        //public virtual void OnReceive(object sender, NetworkPacket packet) { }
        //public virtual void OnSend(object sender, NetworkPacket packet) { }
        //public virtual void OnError(object sender, NetworkPacket packet) { }
        //public virtual void WriteRequest(NetworkPacket packet, NetworkMessage message) { }
        //public virtual void WriteResponse(NetworkPacket packet, NetworkMessage message) { }
        //public Dictionary<uint, MessageStream> cachedStreams = new Dictionary<uint, MessageStream>();
        //public List<byte[]> cachedStreams = new List<byte[]>();



        //public virtual void AttachSocketListener(NetworkSocket _socket)
        //{
        //    socket = _socket;
        //    socket.OnReceive += OnReceive;
        //    //socket.OnSend += OnSend;
        //    socket.OnError += OnError;
        //}

        //public virtual void AttachStreamListener(MessageType msgType, EventHandler<NetworkMessage> func)
        //{
        //    GetChannelEvent((uint)msgType).OnChannelStream += func;
        //}

        //public virtual void AttachRequestListener(MessageType msgType, EventHandler<NetworkMessage> func)
        //{
        //    GetChannelEvent((uint)msgType).OnChannelRequest += func;
        //}

        //public virtual void AttachResponseListener(MessageType msgType, EventHandler<NetworkMessage> func)
        //{
        //    GetChannelEvent((uint)msgType).OnChannelResponse += func;
        //}

        //public virtual void AttachErrorListener(NetworkErrorType errorType, EventHandler<NetworkPacket> func)
        //{

        //}


    }
}
