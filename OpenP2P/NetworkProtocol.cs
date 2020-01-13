
using System;
using System.Net;

namespace OpenP2P
{
    /// <summary>
    /// Network Protocol for header defining the type of message
    /// 
    /// Single Byte Format:
    ///     0 0 0 0 0000
    ///     Bit 8    => Redirect Flag
    ///     Bit 7    => Big Endian Flag
    ///     Bit 6    => Reliable Flag
    ///     Bit 5    => SendType Flag
    ///     Bits 4-1 => Channel Type
    /// </summary>
    public class NetworkProtocol : NetworkProtocolBase
    {
        const uint RedirectFlag = (1 << 8); //bit 8
        const uint StreamFlag = (1 << 7); //bit 7
        const uint ReliableFlag = (1 << 6);  //bit 6
        const uint SendTypeFlag = (1 << 5); //bit 5
        
        public event EventHandler<NetworkMessage> OnWriteHeader = null;
        public event EventHandler<NetworkMessage> OnReadHeader = null;
        public event EventHandler<NetworkPacket> OnErrorConnectToServer;
        public event EventHandler<NetworkPacket> OnErrorReliableFailed;

        Random random = new Random();

        public bool isClient = false;
        public bool isServer = false;


        public NetworkProtocol(string localIP, int localPort, bool _isServer)
        {
            Setup(localIP, localPort, _isServer);
        }


        public NetworkProtocol(int localPort, bool _isServer)
        {
            string localIP = "127.0.0.1";
            Setup(localIP, localPort, _isServer);
        }


        public void Setup(string localIP, int localPort, bool _isServer)
        {
            Console.WriteLine("Binding Socket to: " + localIP + ":" + localPort);
            channel = new NetworkChannel();
            socket = new NetworkSocket(localIP, localPort);
            AttachSocketListener(socket);
            
            AttachNetworkIdentity();
            
            if (isServer)
            {
                ident.RegisterServer(socket.sendSocket.LocalEndPoint);
            }
           
            isClient = !isServer;
            isServer = _isServer;
        }
        

        public void AttachNetworkIdentity()
        {
            ident = new NetworkIdentity();
            ident.AttachToProtocol(this);
        }
        

        public MsgConnectToServer ConnectToServer(string userName)
        {
            return (MsgConnectToServer)ident.ConnectToServer(userName);
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

        public NetworkPacket[] SendStream(EndPoint ep, NetworkMessage message)
        {
            IPEndPoint ip = GetIPv6(ep);

            MsgDataContent msg = (MsgDataContent)message;

            int packetCount = msg.packetCount;
            NetworkPacket[] packets = new NetworkPacket[packetCount];

            message.header.channelType = channel.GetChannelType(message);
            message.header.isReliable = true;
            message.header.isStream = true;
            message.header.sendType = SendType.Message;
            

            message.header.id = ident.local.id;
            for (int i=0; i<packetCount; i++)
            {
                NetworkPacket packet = socket.Prepare(ep);
                packets[i] = packet;
                packet.messages.Add(message);// = message;

                message.header.sequence = ident.local.NextSequence(message);

                WriteHeader(packet, message);
                message.WriteMessage(packet); 

                socket.Send(packet);
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

            if(requestMessage.header.source.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
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
            switch(message.header.sendType)
            {
                case SendType.Message: message.WriteMessage(packet); break;
                case SendType.Response: message.WriteResponse(packet); break;
            }
            
            socket.Send(packet);
        }
        

        public override void OnReceive(object sender, NetworkPacket packet)
        {
            NetworkMessage message = ReadHeader(packet);

            packet.messages.Add(message);
            message.header.source = packet.remoteEndPoint;

            if (message.header.sendType == SendType.Response
                && message.header.isReliable)
            {
                lock (socket.thread.ACKNOWLEDGED)
                {
                    if (!socket.thread.ACKNOWLEDGED.ContainsKey(message.header.ackkey))
                    {
                        socket.thread.ACKNOWLEDGED.Add(message.header.ackkey, packet);
                    }
                }

            }

            if (message.header.isStream)
            {

            }

            switch (message.header.sendType)
            {
                case SendType.Message: message.ReadMessage(packet); break;
                case SendType.Response: message.ReadResponse(packet); break;
            }
            
            NetworkChannelEvent channelEvent = GetChannelEvent(message.header.channelType);
            channelEvent.InvokeEvent(packet, message);
        }
        


        public override void OnSend(object sender, NetworkPacket packet)
        {
        }


        public override void OnError(object sender, NetworkPacket packet)
        {
            NetworkErrorType errorType = (NetworkErrorType)sender;
            switch (errorType)
            {
                case NetworkErrorType.ErrorConnectToServer:
                    if (OnErrorConnectToServer != null)
                        OnErrorConnectToServer.Invoke(this, packet);
                    break;
                case NetworkErrorType.ErrorReliableFailed:
                    if( OnErrorReliableFailed != null )
                        OnErrorReliableFailed.Invoke(this, packet);
                    break;
            }
        }


        public override void AttachErrorListener(NetworkErrorType errorType, EventHandler<NetworkPacket> func)
        {
            switch (errorType)
            {
                case NetworkErrorType.ErrorConnectToServer:
                    OnErrorConnectToServer += func;
                    break;
                case NetworkErrorType.ErrorReliableFailed:
                    OnErrorReliableFailed += func;
                    break;
            }
        }

        // 0000 0000
        // bits 1-4 => Channel Type (up to 16 channels)
        // bits 5 => Send Type
        // bits 6 => Reliable Flag
        // bits 7 => Endian Flag
        // bits 8 => Redirect Flag
        public override void WriteHeader(NetworkPacket packet, NetworkMessage message)
        {
            uint msgBits = (uint)message.header.channelType;
            if (msgBits < 0 || msgBits >= (uint)ChannelType.LAST)
                msgBits = 0;

            //add sendType to bit 5 
            if( message.header.sendType == SendType.Response )
                msgBits |= SendTypeFlag;

            //add reliable to bit 6
            if( message.header.isReliable )
                msgBits |= ReliableFlag;
           
            //add little endian to bit 8
            if ( message.header.isStream )
                msgBits |= StreamFlag;

            if( message.header.isRedirect )
                msgBits |= RedirectFlag;
                
            message.header.isStream = BitConverter.IsLittleEndian;

            packet.Write((byte)msgBits);
            packet.Write(message.header.sequence);

            OnWriteHeader.Invoke(packet, message);

            if (message.header.isReliable)
            {
                if (message.header.sendType == SendType.Message && message.header.retryCount == 0)
                {
                    message.header.ackkey = GenerateAckKey(packet, message);
                }
            }
        }


        public override NetworkMessage ReadHeader(NetworkPacket packet)
        {
            uint bits = packet.ReadByte();

            bool isRedirect = (bits & RedirectFlag) > 0;
            bool isStream = (bits & StreamFlag) > 0;
            bool isReliable = (bits & ReliableFlag) > 0;
            SendType sendType = (SendType)((bits & SendTypeFlag) > 0 ? 1 : 0);
           
            //remove flag bits to reveal channel type
            bits = bits & ~(StreamFlag | SendTypeFlag | ReliableFlag | RedirectFlag);

            if (bits < 0 || bits >= (uint)ChannelType.LAST)
                return (NetworkMessage)channel.CreateMessage(ChannelType.Invalid);

            NetworkMessage message = (NetworkMessage)channel.CreateMessage(bits);
            message.header.isReliable = isReliable;
            message.header.isStream = isStream;
            message.header.sendType = sendType;
            message.header.channelType = (ChannelType)bits;
            message.header.sequence = packet.ReadUShort();

            OnReadHeader.Invoke(packet, message);
            
            if (message.header.isReliable)
            {
                message.header.ackkey = GenerateAckKey(packet, message);
            }
            
            return message;
        }

        public override NetworkMessage[] ReadHeaders(NetworkPacket packet)
        {
            uint bits = packet.ReadByte();
            //remove flag bits to reveal channel type
            bits = bits & ~(StreamFlag | SendTypeFlag | ReliableFlag | RedirectFlag);

            if (bits < 0 || bits >= (uint)ChannelType.LAST)
            {
                NetworkMessage[] msgFailList = new NetworkMessage[1];
                msgFailList[0] = (NetworkMessage)channel.CreateMessage(ChannelType.Invalid);
                return msgFailList;
            }

            uint msgCount = packet.ReadByte();
            NetworkMessage[] msg = new NetworkMessage[msgCount];
            for (int i=0; i<msgCount; i++)
            {
                msg[i] = ReadHeader(packet);
            }
            return msg;
        }
        

        public uint GenerateAckKey(NetworkPacket packet, NetworkMessage message)
        {
            uint sequence = message.header.sequence;
            uint id = message.header.id;

            uint key = sequence | (id << 16);
            return key;
        }
    }
}
