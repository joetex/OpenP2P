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
        public NetworkMessageFactory channel = null;
        public NetworkSocket socket = null;
        public NetworkIdentity ident = null;

        public NetworkProtocolBase() { }
        
        public virtual void WriteHeader(NetworkPacket packet, NetworkMessage message) { }
        public virtual NetworkMessage ReadHeader(NetworkPacket packet) { return null; }
        public virtual NetworkMessage[] ReadHeaders(NetworkPacket packet) { return null; }
        public virtual void OnReceive(object sender, NetworkPacket packet) { }
        public virtual void OnSend(object sender, NetworkPacket packet) { }
        public virtual void OnError(object sender, NetworkPacket packet) { }
        public virtual void WriteRequest(NetworkPacket packet, NetworkMessage message) { }
        public virtual void WriteResponse(NetworkPacket packet, NetworkMessage message) { }
        public Dictionary<uint, MessageStream> cachedStreams = new Dictionary<uint, MessageStream>();
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
