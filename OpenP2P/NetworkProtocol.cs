
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
        const int BigEndianFlag = (1 << 7); //bit 8
        const int ReliableFlag = (1 << 6);  //bit 7
        const int SendTypeFlag = (1 << 5); //bit 6

        public event EventHandler<NetworkStream> OnWriteHeader = null;
        public event EventHandler<NetworkStream> OnReadHeader = null;
        
        public NetworkProtocol(int localPort)
        {
            socket = new NetworkSocket(localPort);
            AttachSocketListener(socket);
            BuildMessages();
            AttachNetworkIdentity();
        }
        
        public void AttachNetworkIdentity()
        {
            ident = new NetworkIdentity();
            ident.AttachToProtocol(this);
        }

        public void RegisterAsServer()
        {
            ident.RegisterServer(socket.sendSocket.LocalEndPoint);
        }

        public void ConnectToServer(IPEndPoint ep, string userName)
        {
            ident.ConnectToServer(ep, userName);
        }

        public void Listen()
        {
            NetworkStream stream = socket.Reserve();
            socket.Listen(stream);
        }

        public void SendReliableRequest(EndPoint ep, NetworkMessage message)
        {
            IPEndPoint ip = GetIPv6(ep);
            NetworkStream stream = socket.Prepare(ep);

            stream.header.messageType = message.messageType;
            stream.header.isReliable = true;
            stream.header.sendType = SendType.Request;
            stream.header.sequence = ident.local.messageSequence[(int)message.messageType]++;
            stream.header.id = ident.local.id;
            Send(stream, message);
        }

        public void SendRequest(EndPoint ep, NetworkMessage message)
        {
            IPEndPoint ip = GetIPv6(ep);
            NetworkStream stream = socket.Prepare(ep);

            stream.header.messageType = message.messageType;
            stream.header.isReliable = false;
            stream.header.sendType = SendType.Request;
            stream.header.sequence = ident.local.messageSequence[(int)message.messageType]++;
            stream.header.id = ident.local.id;
            Send(stream, message);
        }

        public void SendResponse(NetworkStream requestStream, NetworkMessage message)
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
            Send(stream, message);
        }
       
        public void Send(NetworkStream stream, NetworkMessage message)
        {
            WriteHeader(stream);
            switch(stream.header.sendType)
            {
                case SendType.Request: message.WriteRequest(stream); break;
                case SendType.Response: message.WriteResponse(stream); break;
            }
            
            socket.Send(stream);
        }
        

        public override void OnReceive(object sender, NetworkStream stream)
        {
            NetworkMessage message = ReadHeader(stream);
            message.InvokeOnRead(stream);

            if (stream.header.sendType == SendType.Response && stream.header.isReliable)
            {
                //Console.WriteLine("Acknowledging: " + stream.ackkey + " -- id:"+ stream.header.id +", seq:"+stream.header.sequence);
                //lock (NetworkThread.ACKNOWLEDGED)
                {
                    if (NetworkThread.ACKNOWLEDGED.ContainsKey(stream.ackkey))
                        Console.WriteLine("Already exists:" + stream.ackkey);
                    NetworkThread.ACKNOWLEDGED.TryAdd(stream.ackkey, stream);
                }
                stream.acknowledged = true;
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
                    OnErrorConnectToServer.Invoke(this, stream);
                    break;
                case NetworkErrorType.ErrorReliableFailed:
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
            int msgBits = (int)stream.header.messageType;
            if (msgBits < 0 || msgBits >= (int)MessageType.LAST)
                msgBits = 0;

            //add sendType to bit 6 
            msgBits |= (int)stream.header.sendType << 5;

            //add reliable to bit 7
            msgBits |= stream.header.isReliable ? ReliableFlag : 0;
            
            //add little endian to bit 8
            if (!BitConverter.IsLittleEndian)
                msgBits |= BigEndianFlag;

            stream.header.isLittleEndian = BitConverter.IsLittleEndian;

            stream.Write((byte)msgBits);

            OnWriteHeader.Invoke(this, stream);
        }

        public override NetworkMessage ReadHeader(NetworkStream stream)
        {
            int bits = stream.ReadByte();

            bool isLittleEndian = (bits & BigEndianFlag) == 0;
            bool isReliable = (bits & ReliableFlag) > 0;
            SendType sendType = (SendType)((bits & SendTypeFlag) > 0 ? 1 : 0);
           
            //remove response and endian bits
            bits = bits & ~(BigEndianFlag | SendTypeFlag | ReliableFlag);

            if (bits < 0 || bits >= (int)MessageType.LAST)
                return GetMessage((int)MessageType.Invalid);

            NetworkMessage message = GetMessage(bits);

            stream.header.isReliable = isReliable;
            stream.header.isLittleEndian = isLittleEndian;
            stream.header.sendType = sendType;
            stream.header.messageType = message.messageType;

            OnReadHeader.Invoke(this, stream);
            
            return message;
        }
    }
}
