
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

        public Stopwatch profiler = new Stopwatch();

        public NetworkProtocol(int localPort)
        {

            profiler.Start();
            socket = new NetworkSocket(localPort);
            profiler.Stop();
            //Console.WriteLine("new Socket took: " + ((float)profiler.ElapsedMilliseconds / 1000f) + " seconds");

            profiler.Reset();
            profiler.Start();
            AttachSocketListener(socket);
            profiler.Stop();
            //Console.WriteLine("AttachSocketListener took: " + ((float)profiler.ElapsedMilliseconds / 1000f) + " seconds");

            profiler.Reset();
            profiler.Start();
            BuildMessages();
            profiler.Stop();
            //Console.WriteLine("BuildMessages took: " + ((float)profiler.ElapsedMilliseconds / 1000f) + " seconds");

            profiler.Reset();
            profiler.Start();
            AttachNetworkIdentity();
            profiler.Stop();
            //Console.WriteLine("AttachNetworkIdentity took: " + ((float)profiler.ElapsedMilliseconds / 1000f) + " seconds");
            
            AttachThreads();
        }

        public void AttachThreads()
        {
            //threads = new NetworkThread(this);
            //socket.AttachThreads(threads);

            //threads.StartNetworkThreads();

            
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
        
        public IPEndPoint GetIPv6(EndPoint ep)
        {
            IPEndPoint ip = (IPEndPoint)ep;
            if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
                return ip; 
            ip = new IPEndPoint(ip.Address.MapToIPv6(), ip.Port);
            return ip;
        }

        public IPEndPoint GetEndPoint(string ip, int port)
        {
            IPAddress address = null;
            if (IPAddress.TryParse(ip, out address))
                return new IPEndPoint(address, port);
            return null;
        }
        
        /// <summary>
        /// Bind Messages to our Message Dictionary
        /// This uses reflection to map our Enum to a Message class
        /// </summary>
        public void BuildMessages()
        {
            string enumName = "";
            NetworkMessage message = null;
            for (int i=0; i<(int)MessageType.LAST; i++)
            {
                enumName = Enum.GetName(typeof(MessageType), (MessageType)i);
                try
                {
                    message = (NetworkMessage)GetInstance("OpenP2P.Msg" + enumName);
                    messagesContainer.AddService(message.GetType(), message);
                    message.messageType = (MessageType)i;
                }
                catch (Exception e)
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

        public T Create<T>()
        {
            return (T)messagesContainer.GetService(typeof(T));
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
            //IPEndPoint ip = GetIPv6(requestStream.remoteEndPoint);
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

            if (stream.header.sendType == SendType.Response)
            {
                //Console.WriteLine("Acknowledging: " + stream.ackkey + " -- id:"+ stream.header.id +", seq:"+stream.header.sequence);
                lock (NetworkThread.ACKNOWLEDGED)
                {
                    if (NetworkThread.ACKNOWLEDGED.ContainsKey(stream.ackkey))
                        Console.WriteLine("Already exists:" + stream.ackkey);
                    NetworkThread.ACKNOWLEDGED.Add(stream.ackkey, stream);
                }

                stream.acknowledged = true;
            }

            message.InvokeOnRead(stream);
        }

        public override void OnSend(object sender, NetworkStream stream)
        {
            
        }

        public override void WriteHeader(NetworkStream stream)
        {
            //NetworkMessage message = stream.message;

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


        public object GetInstance(string strFullyQualifiedName)
        {
            Type t = Type.GetType(strFullyQualifiedName);
            return Activator.CreateInstance(t);
        }

        public override NetworkMessage GetMessage(int id)
        {
            if (!messages.ContainsKey(id))
                return messages[(int)MessageType.Invalid];
            return messages[id];
        }
    }
}
