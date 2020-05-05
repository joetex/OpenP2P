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
    public partial class ProtocolFSG : NetworkProtocol
    {
        //const uint S
        const uint ProtocolTypeFlag = (1 << 7); //bit 8
        const uint StreamFlag = (1 << 6); //bit 7
        const uint ReliableFlag = (1 << 5);  //bit 6
        const uint SendTypeFlag = (1 << 4); //bit 5
        
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
        MessageFSG tempSendMessage = null;

        public ProtocolFSG(NetworkManager _manager) : base(_manager)
        {
        }

        
        //public void AttachStreamListener(MessageType msgType, EventHandler<NetworkMessage> func)
        //{
        //    GetChannelEvent((uint)msgType).OnChannelStream += func;
        //}

        //public void AttachRequestListener(MessageType msgType, EventHandler<NetworkMessage> func)
        //{
        //    GetChannelEvent((uint)msgType).OnChannelRequest += func;
        //}

        //public void AttachResponseListener(MessageType msgType, EventHandler<NetworkMessage> func)
        //{
        //    GetChannelEvent((uint)msgType).OnChannelResponse += func;
        //}

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
            ident.AttachToProtocol(net);

            if (isServer)
            {
                ident.RegisterServer(socket.sendSocket.LocalEndPoint);
            }
        }


        

        public void HandleReceiveStream(MessageFSG message, NetworkPacket packet)
        {
            MessageStream stream = (MessageStream)message;
            uint streamID = ((uint)stream.header.id << 8) | (uint)stream.header.sequence;

            MessageStream response = (MessageStream)messageFactory.CreateMessage(stream.header.channelType);

            if (message.header.sendType == SendType.Response)
            {
                if (message.header.isReliable)
                {
                    lock (socket.thread.ACKNOWLEDGED)
                    {
                        if (!socket.thread.ACKNOWLEDGED.ContainsKey(message.header.ackkey))
                            socket.thread.ACKNOWLEDGED.Add(message.header.ackkey, packet);
                    }
                }

                message.ReadResponse(packet);
                NetworkMessageEvent messageEvent = GetMessageEvent(message.header.channelType);
                messageEvent.InvokeEvent(packet, message);
            }
            else if (message.header.sendType == SendType.Message)
            {
                //send acknowledgement

                MessageStream first = stream;
                if (cachedStreams.ContainsKey(streamID))
                {
                    first = cachedStreams[streamID];
                }
                else
                {
                    cachedStreams.Add(streamID, first);
                }

                stream.ReadRequest(packet);

                first.SetBuffer(stream.byteData, stream.startPos);

                if (stream.startPos > 0
                    && first.byteData.Length == (stream.startPos + stream.byteData.Length))
                {
                    NetworkMessageEvent messageEvent = GetMessageEvent(first.header.channelType);
                    messageEvent.InvokeEvent(packet, first);

                    messageFactory.FreeMessage(first);
                }

                if (first != stream)
                {
                    messageFactory.FreeMessage(stream);
                }
            }
        }

        public void HandleReceiveMessage(MessageFSG message, NetworkPacket packet)
        {
            switch (message.header.sendType)
            {
                case SendType.Message: message.ReadRequest(packet); break;
                case SendType.Response: message.ReadResponse(packet); break;
            }

            if ((message.header.sendType == SendType.Response)
                && message.header.isReliable)
            {
                lock (socket.thread.ACKNOWLEDGED)
                {
                    if (!socket.thread.ACKNOWLEDGED.ContainsKey(message.header.ackkey))
                        socket.thread.ACKNOWLEDGED.Add(message.header.ackkey, packet);
                }
            }

            NetworkMessageEvent messageEvent = GetMessageEvent(message.header.channelType);
            messageEvent.InvokeEvent(packet, message);

            messageFactory.FreeMessage(message);
        }

        public void WriteHeader(NetworkPacket packet, MessageFSG message)
        {
            if (message.header.isSTUN)
                return;

            uint msgBits = (uint)message.header.channelType;
            if (msgBits < 0 || msgBits >= (uint)MessageType.LAST)
                msgBits = 0;

            //add sendType to bit 5 
            if (message.header.sendType == SendType.Response)
                msgBits |= SendTypeFlag;

            //add reliable to bit 6
            if (message.header.isReliable)
                msgBits |= ReliableFlag;

            //add little endian to bit 8
            if (message.header.isStream)
                msgBits |= StreamFlag;

            msgBits |= ProtocolTypeFlag;

            packet.Write((byte)msgBits);
            packet.Write(message.header.sequence);

            WriteIdentity(packet, message);
            //OnWriteHeader.Invoke(packet, message);

            if (message.header.isReliable)
            {
                if (message.header.sendType == SendType.Message && message.header.retryCount == 0)
                {
                    message.header.ackkey = GenerateAckKey(packet, message);
                }
            }
        }


        public MessageFSG ReadHeader(NetworkPacket packet)
        {
            uint bits = packet.ReadByte();

            bool isSTUN = (bits & ProtocolTypeFlag) == 0;
            if (isSTUN)
            {
                packet.bytePos = 0;
                MessageFSG msg = (MessageFSG)messageFactory.CreateMessage(MessageType.STUN);
                msg.header.isSTUN = true;
                msg.header.isReliable = true;
                msg.header.sendType = SendType.Response;
                return msg;
            }

            bool isStream = (bits & StreamFlag) > 0;
            bool isReliable = (bits & ReliableFlag) > 0;
            SendType sendType = (SendType)((bits & SendTypeFlag) > 0 ? 1 : 0);

            //remove flag bits to reveal channel type
            bits = bits & ~(StreamFlag | SendTypeFlag | ReliableFlag | ProtocolTypeFlag);

            if (bits < 0 || bits >= (uint)MessageType.LAST)
                return (MessageFSG)messageFactory.CreateMessage(MessageType.Invalid);

            MessageFSG message = (MessageFSG)messageFactory.CreateMessage(bits);
            message.header.isReliable = isReliable;
            message.header.isStream = isStream;
            message.header.sendType = sendType;
            message.header.channelType = (MessageType)bits;
            message.header.sequence = packet.ReadUShort();

            ReadIdentity(packet, message);
            //OnReadHeader.Invoke(packet, message);

            if (message.header.isReliable)
            {
                message.header.ackkey = GenerateAckKey(packet, message);
            }

            return message;
        }

   
        public void OnSocketSend(object sender, NetworkPacket packet)
        {
            //check for reliable messages
            bool hasReliable = false;
            for (int i = 0; i < packet.messages.Count; i++)
            {
                tempSendMessage = (MessageFSG)packet.messages[i];
                if (tempSendMessage.header.sendType == SendType.Message
                    && tempSendMessage.header.isReliable)
                {
                    tempSendMessage.header.sentTime = NetworkTime.Milliseconds();
                    tempSendMessage.header.retryCount++;

                    lock (socket.thread.RELIABLEQUEUE)
                    {
                        socket.thread.RELIABLEQUEUE.Enqueue(packet);
                    }

                    hasReliable = true;
                }
            }

            if (!hasReliable)
            {
                messageFactory.FreeMessage(packet.messages[0]);
                socket.Free(packet);
            }
        }

        public void OnSocketReceive(object sender, NetworkPacket packet)
        {
            MessageFSG message = ReadHeader(packet);
            packet.messages.Add(message);

            message.header.source = packet.remoteEndPoint;

            if (message.header.isStream)
            {

                HandleReceiveStream(message, packet);
            }
            else
            {
                HandleReceiveMessage(message, packet);
            }
        }


        public void WriteIdentity(NetworkPacket packet, MessageFSG message)
        {
            packet.Write(message.header.id);
        }

        public void ReadIdentity(NetworkPacket packet, MessageFSG message)
        {
            message.header.id = packet.ReadUShort();
            message.header.peer = net.ident.FindPeer(message.header.id);
        }

    }
}
