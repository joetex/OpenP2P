
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

        public event EventHandler<NetworkMessage> OnWriteHeader = null;
        public event EventHandler<NetworkMessage> OnReadHeader = null;

        public NetworkProtocol(string remoteHost, int remotePort, int localPort)
        {
            socket = new NetworkSocket(remoteHost, remotePort, localPort);
            AttachSocketListener(socket);
            BindMessages();

            AttachNetworkIdentity();
        }

        public void AttachNetworkIdentity()
        {
            ident = new NetworkIdentity();
            ident.AttachToProtocol(this);
        }

        public void RegisterAsServer()
        {
            ident.RegisterServer(socket.local);
        }
        
        public IPEndPoint GetIPv6(EndPoint ep)
        {
            IPEndPoint ip = (IPEndPoint)ep;
            if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
                return ip; 
            ip = new IPEndPoint(ip.Address.MapToIPv6(), ip.Port);
            return ip;
        }
        
        /// <summary>
        /// Bind Messages to our Message Dictionary
        /// This uses reflection to map our Enum to a Message class
        /// </summary>
        public void BindMessages()
        {
            string enumName = "";
            NetworkMessage message = null;
            for (int i=0; i<(int)MessageType.LAST; i++)
            {
                enumName = Enum.GetName(typeof(MessageType), (MessageType)i);
                try
                {
                    message = (NetworkMessage)GetInstance("OpenP2P.Msg" + enumName);
                    message.header.messageType = (MessageType)i;
                }
                catch(Exception e)
                {
                    //Console.WriteLine(e.ToString());
                    message = new MsgInvalid();
                }
                
                messages.Add(i, message);
            }
        }

        public override void AttachSocketListener(NetworkSocket _socket)
        {
            socket = _socket;
            socket.OnReceive += OnReceive;
            socket.OnSend += OnSend;
        }

        public override void AttachRequestListener(MessageType msgType, EventHandler<NetworkMessage> func)
        {
            GetMessage((int)msgType).OnRequest += func;
        }
        public override void AttachResponseListener(MessageType msgType, EventHandler<NetworkMessage> func)
        {
            GetMessage((int)msgType).OnResponse += func;
        }

        public NetworkMessage Create(MessageType _msgType)
        {
            NetworkMessage message = GetMessage((int)_msgType);
            return message;
        }
        
        public void Listen()
        {
            socket.Listen(null);
        }

        public void SendReliableRequest(EndPoint ep, NetworkMessage message)
        {
            message.header.isReliable = true;
            message.header.sendType = SendType.Request;
            message.header.sequence = ident.local.messageSequence[(int)message.header.messageType]++;
            message.header.id = ident.local.id;
            Send(ep, message);
        }
       

        public void SendRequest(EndPoint ep, NetworkMessage message)
        {
            message.header.isReliable = false;
            message.header.sendType = SendType.Request;
            message.header.sequence = ident.local.messageSequence[(int)message.header.messageType]++;
            message.header.id = ident.local.id;
            Send(ep, message);
        }

        public void SendResponse(EndPoint ep, NetworkMessage message)
        {
            message.header.isReliable = true;
            message.header.sendType = SendType.Response;
            Send(ep, message);
        }
       
        public void Send(EndPoint ep, NetworkMessage message)
        {
            IPEndPoint ip = GetIPv6(ep);
            NetworkStream stream = socket.Prepare(ip);
            stream.message = message;
            
            WriteHeader(stream);
            switch(message.header.sendType)
            {
                case SendType.Request: message.WriteRequest(stream); break;
                case SendType.Response: message.WriteResponse(stream); break;
            }
            
            socket.Send(stream);
        }
        

        public override void OnReceive(object sender, NetworkStream stream)
        {
            NetworkMessage message = ReadHeader(stream);

            if (message.header.sendType == SendType.Response)
            {
                //Console.WriteLine("Acknowledging: " + key);
                lock (NetworkThread.ACKNOWLEDGED)
                {
                    if (NetworkThread.ACKNOWLEDGED.ContainsKey(stream.ackkey))
                        Console.WriteLine("ALready exists:" + stream.ackkey);
                    NetworkThread.ACKNOWLEDGED.Add(stream.ackkey, stream);
                }
            }

            message.InvokeOnRead(stream);
        }

        public override void OnSend(object sender, NetworkStream stream)
        {
            if (stream.message.header.sendType == SendType.Request && stream.message.header.isReliable)
            {
                lock (NetworkThread.RELIABLEQUEUE)
                {
                    //Console.WriteLine("Adding Reliable: " + stream.ackkey);
                    stream.sentTime = NetworkTime.Milliseconds();
                    stream.retryCount++;
                    NetworkThread.RELIABLEQUEUE.Enqueue(stream);
                }
            }
            else
            {
                socket.Free(stream);
            }
        }

        public override void WriteHeader(NetworkStream stream)
        {
            NetworkMessage message = stream.message;

            int msgBits = (int)message.header.messageType;
            if (msgBits < 0 || msgBits >= (int)MessageType.LAST)
                msgBits = 0;

            //add sendType to bit 6 
            msgBits |= (int)message.header.sendType << 5;

            //add reliable to bit 7
            msgBits |= message.header.isReliable ? ReliableFlag : 0;
            
            //add little endian to bit 8
            if (!BitConverter.IsLittleEndian)
                msgBits |= BigEndianFlag;
            
            message.header.isLittleEndian = BitConverter.IsLittleEndian;

            stream.Write((byte)msgBits);

            OnWriteHeader.Invoke(stream, message);
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
                return GetMessage((int)MessageType.NULL);

            NetworkMessage message = GetMessage(bits);
            message.header.isReliable = isReliable;
            message.header.isLittleEndian = isLittleEndian;
            message.header.sendType = sendType;
            stream.message = message;

            OnReadHeader.Invoke(stream, message);

            return message;
        }


        public object GetInstance(string strFullyQualifiedName)
        {
            Type t = Type.GetType(strFullyQualifiedName);
            return Activator.CreateInstance(t);
        }

        public override NetworkMessage GetMessage(int id)
        {
            if (!messages.ContainsKey(id))
                return messages[(int)MessageType.NULL];
            return messages[id];
        }
    }
}
