using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace OpenP2P
{
    public class NetworkProtocolBase
    {
        public ServiceContainer messagesContainer = new ServiceContainer();

        public Dictionary<uint, NetworkMessage> messages = new Dictionary<uint, NetworkMessage>();
        public Dictionary<uint, uint> messageSequences = new Dictionary<uint, uint>();

        public Dictionary<string, MessageChannel> awaitingResponse = new Dictionary<string, MessageChannel>();

        public NetworkSocket socket = null;
        public NetworkIdentity ident = null;
        public NetworkIdentity.PeerIdentity localIdentity = new NetworkIdentity.PeerIdentity();
        public int responseType = 0;
        public bool isLittleEndian = false;

        //public NetworkThread threads = null;

        public NetworkProtocolBase() { }
        
        public virtual void WriteHeader(NetworkPacket packet) { }
        public virtual NetworkMessage ReadHeader(NetworkPacket packet) { return null; }

        public virtual void OnReceive(object sender, NetworkPacket packet) { }
        public virtual void OnSend(object sender, NetworkPacket packet) { }
        public virtual void OnError(object sender, NetworkPacket packet) { }

        /// <summary>
        /// Bind Messages to our Message Dictionary
        /// This uses reflection to map our Enum to a Message class
        /// </summary>
        public virtual void SetupNetworkMessages()
        {
            string enumName = "";
            NetworkMessage message = null;
            for (uint i = 0; i < (uint)MessageChannel.LAST; i++)
            {
                enumName = Enum.GetName(typeof(MessageChannel), (MessageChannel)i);
                try
                {
                    message = (NetworkMessage)GetInstance("OpenP2P.Msg" + enumName);
                    messagesContainer.AddService(message.GetType(), message);
                    message.messageChannel = (MessageChannel)i;
                }
                catch (Exception e)
                {
                    //Console.WriteLine(e.ToString());
                    message = new MsgInvalid();
                }

                messages.Add(i, message);
            }
        }

        public virtual IPEndPoint GetIPv6(EndPoint ep)
        {
            IPEndPoint ip = (IPEndPoint)ep;
            if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
                return ip;
            ip = new IPEndPoint(ip.Address.MapToIPv6(), ip.Port);
            return ip;
        }

        public virtual IPEndPoint GetEndPoint(string ip, int port)
        {
            IPAddress address = null;
            if (IPAddress.TryParse(ip, out address))
                return new IPEndPoint(address, port);
            return null;
        }



        public virtual void AttachSocketListener(NetworkSocket _socket)
        {
            socket = _socket;
            socket.OnReceive += OnReceive;
            socket.OnSend += OnSend;
            socket.OnError += OnError;
        }

        public virtual void AttachMessageListener(MessageChannel msgType, EventHandler<NetworkMessage> func)
        {
            GetMessage((uint)msgType).OnMessage += func;
        }
        public virtual void AttachResponseListener(MessageChannel msgType, EventHandler<NetworkMessage> func)
        {
            GetMessage((uint)msgType).OnResponse += func;
        }
        public virtual void AttachErrorListener(NetworkErrorType errorType, EventHandler<NetworkPacket> func)
        {
           
        }
        

        public virtual T Create<T>()
        {
            return (T)messagesContainer.GetService(typeof(T));
        }

        public virtual object GetInstance(string strFullyQualifiedName)
        {
            Type t = Type.GetType(strFullyQualifiedName);
            return Activator.CreateInstance(t);
        }

        public virtual NetworkMessage GetMessage(uint id)
        {
            if (!messages.ContainsKey(id))
                return messages[(int)MessageChannel.Invalid];
            return messages[id];
        }
    }
}
