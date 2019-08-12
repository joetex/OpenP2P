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
        public ServiceContainer channelContainer = new ServiceContainer();


        public Dictionary<Type, ChannelType> messageTypes = new Dictionary<Type, ChannelType>();
        public Dictionary<uint, NetworkChannel> channels = new Dictionary<uint, NetworkChannel>();
        public Dictionary<uint, uint> messageSequences = new Dictionary<uint, uint>();
        
        public NetworkSocket socket = null;
        public NetworkIdentity ident = null;
        public NetworkIdentity.PeerIdentity localIdentity = new NetworkIdentity.PeerIdentity();
        public int responseType = 0;
        public bool isLittleEndian = false;

        //public NetworkThread threads = null;

        public NetworkProtocolBase() { }
        
        public virtual void WriteHeader(NetworkPacket packet, NetworkMessage message) { }
        public virtual NetworkMessage ReadHeader(NetworkPacket packet) { return null; }

        public virtual void OnReceive(object sender, NetworkPacket packet) { }
        public virtual void OnSend(object sender, NetworkPacket packet) { }
        public virtual void OnError(object sender, NetworkPacket packet) { }
        
        /// <summary>
        /// Setup Network Channels
        /// Cache channels to a dictionary for fast access
        /// </summary>
        public virtual void SetupNetworkChannels()
        {
            string enumName = "";
            NetworkChannel channel = null;
            for (uint i = 0; i < (uint)ChannelType.LAST; i++)
            {
                
              
                channel = new NetworkChannel();
                channel.channelType = (ChannelType)i;
                channels.Add(i, channel);

                try
                {
                    enumName = Enum.GetName(typeof(ChannelType), (ChannelType)i);
                    NetworkMessage message = (NetworkMessage)GetInstance("OpenP2P.Msg" + enumName);
                    messageTypes.Add(message.GetType(), (ChannelType)i);
                }
                catch(Exception e)
                {
                    //Console.WriteLine(e.ToString());

                }
                
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

        public virtual void AttachMessageListener(ChannelType msgType, EventHandler<NetworkMessage> func)
        {
            GetChannel((uint)msgType).OnChannelMessage += func;
        }
        public virtual void AttachResponseListener(ChannelType msgType, EventHandler<NetworkMessage> func)
        {
            GetChannel((uint)msgType).OnChannelResponse += func;
        }
        public virtual void AttachErrorListener(NetworkErrorType errorType, EventHandler<NetworkPacket> func)
        {
           
        }

        public static class New<T> where T : new()
        {
            public static readonly Func<T> Instance = Expression.Lambda<Func<T>>
                                                      (
                                                       Expression.New(typeof(T))
                                                      ).Compile();
        }

        public T Create<T>() where T : new()
        {
            T obj = New<T>.Instance();
            return obj;
        }

        public virtual object GetInstance(string strFullyQualifiedName)
        {
            Type t = Type.GetType(strFullyQualifiedName);
            return Activator.CreateInstance(t);
        }

        public virtual NetworkChannel GetChannel(ChannelType type)
        {
            return GetChannel((uint)type);
        }

        public virtual NetworkChannel GetChannel(uint id)
        {
            if (!channels.ContainsKey(id))
                return channels[(int)ChannelType.Invalid];
            return channels[id];
        }

        public virtual NetworkMessage CreateMessage(ChannelType type)
        {
            NetworkMessage message = NetworkChannel.CreateMessage(type);// messageConstructors[id].Invoke();
            message.header.channelType = type;
            return message;
        }

        public virtual NetworkMessage CreateMessage(uint id)
        {
            NetworkMessage message = NetworkChannel.CreateMessage(id);// messageConstructors[id].Invoke();
            message.header.channelType = (ChannelType)id;
            return message;
        }
        
        public ChannelType GetChannelType(NetworkMessage msg)
        {
            Type type = msg.GetType();
            return messageTypes[type];
        }
    }
}
