
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    ///     0 0 0 00000
    ///     Bit 8 = Big Endian Flag
    ///     Bit 7 = Reliable Flag
    ///     Bit 6 = SendType Flag
    /// </summary>
    public class NetworkProtocol : NetworkProtocolBase
    {
        const uint BigEndianFlag = (1 << 7); //bit 8
        const uint ReliableFlag = (1 << 6);  //bit 7
        const uint SendTypeFlag = (1 << 5); //bit 6

        public event EventHandler<NetworkPacket> OnWriteHeader = null;
        public event EventHandler<NetworkPacket> OnReadHeader = null;
        
        public NetworkProtocol(string localIP, int localPort, bool isServer)
        {
            Setup(localIP, localPort, isServer);
        }
        public NetworkProtocol(int localPort, bool isServer)
        {
            string localIP = "127.0.0.1";
            Setup(localIP, localPort, isServer);
        }

        public void Setup(string localIP, int localPort, bool isServer)
        {
            SetupNetworkChannels();

            Console.WriteLine("Binding Socket to: " + localIP + ":" + localPort);
            socket = new NetworkSocket(localIP, localPort);
            AttachSocketListener(socket);
            
            AttachNetworkIdentity();

            if (isServer)
            {
                ident.RegisterServer(socket.sendSocket.LocalEndPoint);
            }
        }
        
        public void AttachNetworkIdentity()
        {
            ident = new NetworkIdentity();
            ident.AttachToProtocol(this);
        }
        

        public NetworkPacket ConnectToServer(IPEndPoint ep, string userName)
        {
            return ident.ConnectToServer(ep, userName);
        }

        public NetworkPacket SendReliableMessage(EndPoint ep, NetworkMessage message)
        {
            IPEndPoint ip = GetIPv6(ep);
            NetworkPacket packet = socket.Prepare(ep);

            message.header.channelType = GetChannelType(message);
            message.header.isReliable = true;
            message.header.sendType = SendType.Message;
            if (packet.retryCount == 0)
                message.header.sequence = ident.local.NextSequence(message);
            message.header.id = ident.local.id;
            Send(packet, message);
            return packet;
        }

        public NetworkPacket SendMessage(EndPoint ep, NetworkMessage message)
        {
            IPEndPoint ip = GetIPv6(ep);
            NetworkPacket packet = socket.Prepare(ep);

            message.header.channelType = GetChannelType(message);
            message.header.isReliable = false;
            message.header.sendType = SendType.Message;
            message.header.sequence = ident.local.NextSequence(message);
            message.header.id = ident.local.id;
            Send(packet, message);
            return packet;
        }

        public NetworkPacket SendResponse(NetworkPacket requestPacket, NetworkMessage message)
        {
            NetworkPacket packet = socket.Prepare(requestPacket.remoteEndPoint);
            if(requestPacket.remoteEndPoint.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                packet.networkIPType = NetworkSocket.NetworkIPType.IPv4;
            else
                packet.networkIPType = NetworkSocket.NetworkIPType.IPv6;

            message.header.channelType = requestPacket.message.header.channelType;
            message.header.isReliable = requestPacket.message.header.isReliable;
            message.header.sendType = SendType.Response;
            message.header.sequence = requestPacket.message.header.sequence;
            message.header.id = requestPacket.message.header.id;
            packet.ackkey = requestPacket.ackkey;
            Send(packet, message);
            return packet;
        }
       
        public void Send(NetworkPacket packet, NetworkMessage message)
        {
            packet.message = message;

            WriteHeader(packet);
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

            switch (message.header.sendType)
            {
                case SendType.Message: message.ReadMessage(packet); break;
                case SendType.Response: message.ReadResponse(packet); break;
            }

            NetworkChannel channel = GetNetworkChannel((uint)message.header.channelType);
            channel.InvokeChannelEvent(packet, message);

            if (message.header.sendType == SendType.Response && message.header.isReliable)
            {
                //Console.WriteLine("Acknowledging: " + packet.ackkey + " -- id:"+ packet.header.id +", seq:"+packet.header.sequence);
                lock (packet.socket.thread.ACKNOWLEDGED)
                {
                    if (!packet.socket.thread.ACKNOWLEDGED.ContainsKey(packet.ackkey))
                    {
                        packet.socket.thread.ACKNOWLEDGED.Add(packet.ackkey, packet);
                    }
                }
            }
            
        }

        public override void OnSend(object sender, NetworkPacket packet)
        {
        }

        public event EventHandler<NetworkPacket> OnErrorConnectToServer;
        public event EventHandler<NetworkPacket> OnErrorReliableFailed;

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

        public override void WriteHeader(NetworkPacket packet)
        {
            uint msgBits = (uint)packet.message.header.channelType;
            if (msgBits < 0 || msgBits >= (uint)ChannelType.LAST)
                msgBits = 0;

            //add sendType to bit 6 
            msgBits |= (uint)packet.message.header.sendType << 5;

            //add reliable to bit 7
            msgBits |= packet.message.header.isReliable ? ReliableFlag : 0;
            
            //add little endian to bit 8
            if (!BitConverter.IsLittleEndian)
            {
                msgBits |= BigEndianFlag;
            }
                

            packet.message.header.isLittleEndian = BitConverter.IsLittleEndian;

            packet.Write((byte)msgBits);
            packet.Write(packet.message.header.sequence);

            OnWriteHeader.Invoke(this, packet);

            if (packet.message.header.isReliable)
            {
                if (packet.message.header.sendType == SendType.Message && packet.retryCount == 0)
                {
                    packet.ackkey = GenerateAckKey(packet);
                }

                packet.Write(packet.ackkey);
            }
        }

        public override NetworkMessage ReadHeader(NetworkPacket packet)
        {
            uint bits = packet.ReadByte();

            bool isLittleEndian = (bits & BigEndianFlag) == 0;
            bool isReliable = (bits & ReliableFlag) > 0;
            SendType sendType = (SendType)((bits & SendTypeFlag) > 0 ? 1 : 0);
           
            //remove response and endian bits
            bits = bits & ~(BigEndianFlag | SendTypeFlag | ReliableFlag);

            if (bits < 0 || bits >= (uint)ChannelType.LAST)
                return CreateMessage((uint)ChannelType.Invalid);

            packet.message = CreateMessage(bits);
            packet.message.header.isReliable = isReliable;
            packet.message.header.isLittleEndian = isLittleEndian;
            packet.message.header.sendType = sendType;
            packet.message.header.channelType = (ChannelType)bits;
            packet.message.header.sequence = packet.ReadUShort();

            OnReadHeader.Invoke(this, packet);
            
            if (packet.message.header.isReliable)
            {
                packet.ackkey = packet.ReadUInt();
            }

            
            return packet.message;
        }


        Random random = new Random();
        public uint GenerateAckKey(NetworkPacket packet)
        {
            /*
            ulong key = 0;
            if (packet.header.id == 0)
            {
                int remoteHash = random.Next(0, 1000000000);// packet.remoteEndPoint.ToString().GetHashCode();
                int localHash = random.Next(0, 1000000000);//packet.socket.sendSocket.LocalEndPoint.ToString().GetHashCode();
                //Console.WriteLine("Remote: " + packet.remoteEndPoint.ToString() + " :: " + remoteHash);
                //Console.WriteLine("Local: " + packet.socket.sendSocket.LocalEndPoint.ToString() + " :: " + localHash);
                key = ((ulong)remoteHash + (ulong)localHash);
                return key;
            }
            key |= (ulong)((ulong)packet.header.messageChannel) << 31;
            key |= (ulong)((ulong)packet.header.id) << 15;
            key |= (ulong)((ulong)packet.header.sequence);
            return key;
            */
            uint sequence = packet.message.header.sequence;
            uint id = packet.message.header.id;

            uint key = sequence | (id << 8);
            return key;

        }
    }
}
