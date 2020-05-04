using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace OpenP2P
{

    /// <summary>
    /// Network Protocol for header defining the type of message
    /// 
    /// Single Byte Format:
    ///     0 0 0 0 0000
    ///     Bit 8    => ProtocolType Flag
    ///     Bit 7    => Big Endian Flag
    ///     Bit 6    => Reliable Flag
    ///     Bit 5    => SendType Flag
    ///     Bits 4-1 => Channel Type
    /// </summary>
    /// 
    public class ProtocolFSG : INetworkProtocol
    {
        ProtocolFSG()
        {

        }
        //const uint S
        const uint ProtocolTypeFlag = (1 << 7); //bit 8
        const uint StreamFlag = (1 << 6); //bit 7
        const uint ReliableFlag = (1 << 5);  //bit 6
        const uint SendTypeFlag = (1 << 4); //bit 5

        public bool isClient = false;
        public bool isServer = false;


        public class Header : IMessageHeader
        {
            //encoded into packet
            public bool isReliable = false;
            public bool isStream = false;
            public bool isSTUN = false;
            public SendType sendType = 0;
            public MessageType channelType = MessageType.Invalid;
            public ushort sequence = 0;
            public ushort id = 0;

            //packet status
            public uint ackkey = 0;
            public long sentTime = 0;
            public int retryCount = 0;

            //packet endpoints
            public EndPoint source = null;
            public EndPoint destination = null;
            public NetworkPeer peer = null;
        }

        public Header header = new Header();
        public Dictionary<uint, MessageStream> cachedStreams = new Dictionary<uint, MessageStream>();

        public void OnWriteHeader(object sender, NetworkMessage message)
        {
            NetworkPacket packet = (NetworkPacket)sender;
            packet.Write(message.header.id);
        }

        public void OnReadHeader(object sender, NetworkMessage message)
        {
            NetworkPacket packet = (NetworkPacket)sender;
            message.header.id = packet.ReadUShort();
            message.header.peer = FindPeer(message.header.id);
        }

        public void AttachStreamListener(MessageType msgType, EventHandler<NetworkMessage> func)
        {
            GetChannelEvent((uint)msgType).OnChannelStream += func;
        }

        public void AttachRequestListener(MessageType msgType, EventHandler<NetworkMessage> func)
        {
            GetChannelEvent((uint)msgType).OnChannelRequest += func;
        }

        public void AttachResponseListener(MessageType msgType, EventHandler<NetworkMessage> func)
        {
            GetChannelEvent((uint)msgType).OnChannelResponse += func;
        }

        public void AttachErrorListener(NetworkErrorType errorType, EventHandler<NetworkPacket> func)
        {

        }

        public void AttachNetworkIdentity()
        {
            AttachNetworkIdentity(new NetworkIdentity());
        }

        public void AttachNetworkIdentity(NetworkIdentity ni)
        {
            ident = ni;
            ident.AttachToProtocol(this);

            if (isServer)
            {
                ident.RegisterServer(socket.sendSocket.LocalEndPoint);
            }
        }


        public virtual MessageServer ConnectToServer(string userName)
        {
            return (MessageServer)ident.ConnectToServer(userName);
        }


        public NetworkPacket SendReliableMessage(EndPoint ep, NetworkMessage message)
        {
            IPEndPoint ip = GetIPv6(ep);
            NetworkPacket packet = socket.Prepare(ep);

            message.header.destination = ep;
            message.header.channelType = channel.GetChannelType(message);
            message.header.isReliable = true;
            message.header.sendType = SendType.Message;
            message.header.id = ident.local.id;

            if (message.header.retryCount == 0)
                message.header.sequence = ident.local.NextSequence(message);

            Send(packet, message);

            return packet;
        }

        public List<NetworkPacket> SendStream(EndPoint ep, NetworkMessage message)
        {
            IPEndPoint ip = GetIPv6(ep);
            MessageStream stream = (MessageStream)message;
            List<NetworkPacket> packets = new List<NetworkPacket>();

            stream.header.channelType = channel.GetChannelType(stream);
            stream.header.isReliable = true;
            stream.header.isStream = true;
            stream.header.sendType = SendType.Message;
            stream.header.sequence = ident.local.NextSequence(stream);
            stream.header.id = ident.local.id;

            while (stream.segmentLen > 0 && stream.startPos < stream.byteData.Length)
            {
                NetworkPacket packet = socket.Prepare(ep);
                packet.messages.Add(stream);

                WriteHeader(packet, stream);
                WriteRequest(packet, stream);

                socket.Send(packet);
                Console.WriteLine("Sent " + (stream.segmentLen) + " bytes");
            }

            return packets;
        }

        public NetworkPacket SendMessage(EndPoint ep, NetworkMessage message)
        {
            IPEndPoint ip = GetIPv6(ep);
            NetworkPacket packet = socket.Prepare(ep);

            message.header.channelType = channel.GetChannelType(message);
            message.header.isReliable = false;
            message.header.sendType = SendType.Message;
            message.header.sequence = ident.local.NextSequence(message);
            message.header.id = ident.local.id;

            Send(packet, message);

            return packet;
        }


        public NetworkPacket SendResponse(NetworkMessage requestMessage, NetworkMessage response)
        {
            NetworkPacket packet = socket.Prepare(requestMessage.header.source);

            if (requestMessage.header.source.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                packet.networkIPType = NetworkSocket.NetworkIPType.IPv4;
            else
                packet.networkIPType = NetworkSocket.NetworkIPType.IPv6;

            response.header.channelType = requestMessage.header.channelType;
            response.header.isReliable = requestMessage.header.isReliable;
            response.header.sendType = SendType.Response;
            response.header.sequence = requestMessage.header.sequence;
            response.header.id = requestMessage.header.id;
            response.header.ackkey = requestMessage.header.ackkey;

            Send(packet, response);

            return packet;
        }


        public void Send(NetworkPacket packet, NetworkMessage message)
        {
            packet.messages.Add(message);// = message;
            WriteHeader(packet, message);
            switch (message.header.sendType)
            {
                case SendType.Message: WriteRequest(packet, message); break;
                case SendType.Response: WriteResponse(packet, message); break;
            }

            socket.Send(packet);
        }

        public virtual void WriteRequest(NetworkPacket packet) { }
        public virtual void WriteResponse(NetworkPacket packet) { }
        public virtual void ReadRequest(NetworkPacket packet) { }
        public virtual void ReadResponse(NetworkPacket packet) { }

        public virtual void StreamMessage(NetworkPacket packet) { }

        public void WriteHeader(NetworkPacket packet, NetworkMessage message)
        {
            throw new NotImplementedException();
        }

        public NetworkMessage ReadHeader(NetworkPacket packet)
        {
            throw new NotImplementedException();
        }

        public void WriteMessage(NetworkPacket packet, NetworkMessage message)
        {
            throw new NotImplementedException();
        }

        public void ReadMessage(NetworkPacket packet, NetworkMessage message)
        {
            throw new NotImplementedException();
        }

        public void BeginWrite(NetworkPacket packet, NetworkMessage message)
        {
            throw new NotImplementedException();
        }

        public void BeginRead(NetworkPacket packet, NetworkMessage message)
        {
            throw new NotImplementedException();
        }

        public void OnSocketSend(object sender, NetworkPacket packet)
        {
            throw new NotImplementedException();
        }

        public void OnSocketReceive(object sender, NetworkPacket packet)
        {
            throw new NotImplementedException();
        }
    }
}
