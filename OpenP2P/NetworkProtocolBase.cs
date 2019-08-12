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

        
        
        public Dictionary<uint, NetworkChannel> channels = new Dictionary<uint, NetworkChannel>();
        public Dictionary<uint, uint> messageSequences = new Dictionary<uint, uint>();
        
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
        public virtual void SetupNetworkChannels()
        {
            string enumName = "";
            NetworkChannel channel = null;
            for (uint i = 0; i < (uint)ChannelType.LAST; i++)
            {
                enumName = Enum.GetName(typeof(ChannelType), (ChannelType)i);
                //try
                //{
                channel = new NetworkChannel();// (NetworkChannel)GetInstance("OpenP2P.Msg" + enumName);
                channel.channelType = (ChannelType)i;
                    //channelContainer.AddService(channel.GetType(), channel);
                    //channel.channelType = (ChannelType)i;
                //}
                //catch (Exception e)
                //{
                    //Console.WriteLine(e.ToString());
                    //channel = new MsgInvalid();
                //}
                //messageConstructors.Add(i, New<channel.GetType()>.Instance() )
                channels.Add(i, channel);
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
            GetNetworkChannel((uint)msgType).OnChannelMessage += func;
        }
        public virtual void AttachResponseListener(ChannelType msgType, EventHandler<NetworkMessage> func)
        {
            GetNetworkChannel((uint)msgType).OnChannelResponse += func;
        }
        public virtual void AttachErrorListener(NetworkErrorType errorType, EventHandler<NetworkPacket> func)
        {
           
        }
        
        public virtual T Create<T>()
        {
            return (T)channelContainer.GetService(typeof(T));
        }

        public virtual object GetInstance(string strFullyQualifiedName)
        {
            Type t = Type.GetType(strFullyQualifiedName);
            return Activator.CreateInstance(t);
        }

        public virtual NetworkChannel GetNetworkChannel(uint id)
        {
            if (!channels.ContainsKey(id))
                return channels[(int)ChannelType.Invalid];
            return channels[id];
        }

        public virtual NetworkMessage GetMessage(uint id)
        {
            NetworkMessage message = NetworkChannel.constructors[id]();// messageConstructors[id].Invoke();
            return message;
            /*
            if (!channels.ContainsKey(id))
                return channels[(int)MessageChannel.Invalid];
            return channels[id];*/
        }
    }
}
