
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

        public event EventHandler<NetworkStream> OnWriteHeader = null;
        public event EventHandler<NetworkStream> OnReadHeader = null;
        
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
            SetupNetworkMessages();

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
        

        public NetworkStream ConnectToServer(IPEndPoint ep, string userName)
        {
            return ident.ConnectToServer(ep, userName);
        }

        public NetworkStream SendReliableMessage(EndPoint ep, NetworkMessage message)
        {
            IPEndPoint ip = GetIPv6(ep);
            NetworkStream stream = socket.Prepare(ep);

            stream.header.messageType = message.messageType;
            stream.header.isReliable = true;
            stream.header.sendType = SendType.Message;
            if (stream.retryCount == 0)
                stream.header.sequence = ident.local.NextSequence(message);
            stream.header.id = ident.local.id;
            Send(stream, message);
            return stream;
        }

        public NetworkStream SendMessage(EndPoint ep, NetworkMessage message)
        {
            IPEndPoint ip = GetIPv6(ep);
            NetworkStream stream = socket.Prepare(ep);

            stream.header.messageType = message.messageType;
            stream.header.isReliable = false;
            stream.header.sendType = SendType.Message;
            stream.header.sequence = ident.local.NextSequence(message);
            stream.header.id = ident.local.id;
            Send(stream, message);
            return stream;
        }

        public NetworkStream SendResponse(NetworkStream requestStream, NetworkMessage message)
        {
            NetworkStream stream = socket.Prepare(requestStream.remoteEndPoint);
            if(requestStream.remoteEndPoint.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                stream.networkIPType = NetworkSocket.NetworkIPType.IPv4;
            else
                stream.networkIPType = NetworkSocket.NetworkIPType.IPv6;

            stream.header.messageType = requestStream.header.messageType;
            stream.header.isReliable = requestStream.header.isReliable;
            stream.header.sendType = SendType.Response;
            stream.header.sequence = requestStream.header.sequence;
            stream.header.id = requestStream.header.id;
            stream.ackkey = requestStream.ackkey;
            Send(stream, message);
            return stream;
        }
       
        public void Send(NetworkStream stream, NetworkMessage message)
        {
            WriteHeader(stream);
            switch(stream.header.sendType)
            {
                case SendType.Message: message.WriteMessage(stream); break;
                case SendType.Response: message.WriteResponse(stream); break;
            }
            
            socket.Send(stream);
        }
        

        public override void OnReceive(object sender, NetworkStream stream)
        {
            //NetworkConfig.ProfileBegin("OnReceive");
            NetworkMessage message = ReadHeader(stream);
            message.InvokeOnRead(stream);
            //NetworkConfig.ProfileEnd("OnReceive");
            if (stream.header.sendType == SendType.Response && stream.header.isReliable)
            {
                //Console.WriteLine("Acknowledging: " + stream.ackkey + " -- id:"+ stream.header.id +", seq:"+stream.header.sequence);
                lock (stream.socket.thread.ACKNOWLEDGED)
                {
                    if (!stream.socket.thread.ACKNOWLEDGED.ContainsKey(stream.ackkey))
                    {
                        stream.socket.thread.ACKNOWLEDGED.Add(stream.ackkey, stream);
                    }
                }
            }
            
        }

        public override void OnSend(object sender, NetworkStream stream)
        {
        }

        public event EventHandler<NetworkStream> OnErrorConnectToServer;
        public event EventHandler<NetworkStream> OnErrorReliableFailed;

        public override void OnError(object sender, NetworkStream stream)
        {
            NetworkErrorType errorType = (NetworkErrorType)sender;
            switch (errorType)
            {
                case NetworkErrorType.ErrorConnectToServer:
                    if (OnErrorConnectToServer != null)
                        OnErrorConnectToServer.Invoke(this, stream);
                    break;
                case NetworkErrorType.ErrorReliableFailed:
                    if( OnErrorReliableFailed != null )
                        OnErrorReliableFailed.Invoke(this, stream);
                    break;
            }
        }

        public override void AttachErrorListener(NetworkErrorType errorType, EventHandler<NetworkStream> func)
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

        public override void WriteHeader(NetworkStream stream)
        {
            uint msgBits = (uint)stream.header.messageType;
            if (msgBits < 0 || msgBits >= (uint)MessageType.LAST)
                msgBits = 0;

            //add sendType to bit 6 
            msgBits |= (uint)stream.header.sendType << 5;

            //add reliable to bit 7
            msgBits |= stream.header.isReliable ? ReliableFlag : 0;
            
            //add little endian to bit 8
            if (!BitConverter.IsLittleEndian)
            {
                msgBits |= BigEndianFlag;
            }
                

            stream.header.isLittleEndian = BitConverter.IsLittleEndian;

            stream.Write((byte)msgBits);
            stream.Write(stream.header.sequence);

            OnWriteHeader.Invoke(this, stream);

            if (stream.header.isReliable)
            {
                if (stream.header.sendType == SendType.Message && stream.retryCount == 0)
                {
                    stream.ackkey = GenerateAckKey(stream);
                }

                stream.Write(stream.ackkey);
            }
        }

        public override NetworkMessage ReadHeader(NetworkStream stream)
        {
            uint bits = stream.ReadByte();

            bool isLittleEndian = (bits & BigEndianFlag) == 0;
            bool isReliable = (bits & ReliableFlag) > 0;
            SendType sendType = (SendType)((bits & SendTypeFlag) > 0 ? 1 : 0);
           
            //remove response and endian bits
            bits = bits & ~(BigEndianFlag | SendTypeFlag | ReliableFlag);

            if (bits < 0 || bits >= (uint)MessageType.LAST)
                return GetMessage((uint)MessageType.Invalid);

            NetworkMessage message = GetMessage(bits);

            stream.header.isReliable = isReliable;
            stream.header.isLittleEndian = isLittleEndian;
            stream.header.sendType = sendType;
            stream.header.messageType = message.messageType;

            stream.header.sequence = stream.ReadUShort();

            OnReadHeader.Invoke(this, stream);
            
            if (stream.header.isReliable)
            {
                stream.ackkey = stream.ReadUInt();
            }

            return message;
        }


        Random random = new Random();
        public uint GenerateAckKey(NetworkStream stream)
        {
            /*
            ulong key = 0;
            if (stream.header.id == 0)
            {
                int remoteHash = random.Next(0, 1000000000);// stream.remoteEndPoint.ToString().GetHashCode();
                int localHash = random.Next(0, 1000000000);//stream.socket.sendSocket.LocalEndPoint.ToString().GetHashCode();
                //Console.WriteLine("Remote: " + stream.remoteEndPoint.ToString() + " :: " + remoteHash);
                //Console.WriteLine("Local: " + stream.socket.sendSocket.LocalEndPoint.ToString() + " :: " + localHash);
                key = ((ulong)remoteHash + (ulong)localHash);
                return key;
            }
            key |= (ulong)((ulong)stream.header.messageType) << 31;
            key |= (ulong)((ulong)stream.header.id) << 15;
            key |= (ulong)((ulong)stream.header.sequence);
            return key;
            */
            uint sequence = stream.header.sequence;
            uint id = stream.header.id;

            uint key = sequence | (id << 8);
            return key;

        }
    }
}
