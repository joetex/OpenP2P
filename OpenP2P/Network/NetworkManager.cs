
using System;
using System.Net;
using System.Collections.Generic;
using System.Linq.Expressions;

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
    public class NetworkManager
    {
        //const uint S
        //const uint ProtocolTypeFlag = (1 << 7); //bit 8
        //const uint StreamFlag = (1 << 6); //bit 7
        //const uint ReliableFlag = (1 << 5);  //bit 6
        //const uint SendTypeFlag = (1 << 4); //bit 5

        //public event EventHandler<NetworkMessage> OnWriteHeader = null;
        //public event EventHandler<NetworkMessage> OnReadHeader = null;
        //public event EventHandler<NetworkPacket> OnErrorConnectToServer;
        //public event EventHandler<NetworkPacket> OnErrorReliableFailed;
        //public event EventHandler<NetworkPacket> OnErrorSTUNFailed;

        
        public NetworkSocket socket = null;
        public NetworkIdentity ident = null;
        public Dictionary<string, NetworkProtocol> protocols = new Dictionary<string, NetworkProtocol>();


        Random random = new Random();

        public bool isClient = false;
        public bool isServer = false;


        public NetworkManager(bool _isServer)
        {
            Setup(0, _isServer);
        }

        public NetworkManager(int localPort, bool _isServer)
        {
            Setup(localPort, _isServer);
        }

        public void DefaultProtocols()
        {
            RegisterProtocol("FSG", new ProtocolFSG(this));
            RegisterProtocol("STUN", new ProtocolSTUN(this));
        }

        public void Setup(int localPort, bool _isServer)
        {
            string localIP = "127.0.0.1";
           
            socket = new NetworkSocket(localIP, localPort);

            Console.WriteLine("Binding Socket to: " + localIP + ":" + localPort);
            Console.WriteLine("Binded to: " + socket.socket4.LocalEndPoint.ToString());
            
            isClient = !isServer;
            isServer = _isServer;

            AttachSocketListener(socket);
            //AttachNetworkIdentity();
        }

        public void AttachSocketListener(NetworkSocket _socket)
        {
            socket = _socket;
            socket.OnReceive += OnReceive;
            socket.OnSend += OnSend;
            socket.OnError += OnError;
        }

        public void SendPacket(NetworkPacket packet)
        {
            socket.Send(packet);
        }

        public void RegisterProtocol(string name, NetworkProtocol protocol)
        {
            protocols.Add(name, protocol);
        }

        public NetworkProtocol GetProtocol(string name)
        {
            if( !protocols.ContainsKey(name) )
            {
                return null;
            }

            return protocols[name];
        }

        public void OnReceive(object sender, NetworkPacket packet)
        {
            EndPoint ep = packet.RemoteEndPoint;
            NetworkPeer peer = ident.peersByEndpoint[ep];

            peer.protocol.OnSocketReceive(packet);

            //NetworkMessage message = ReadHeader(packet);

            //packet.messages.Add(message);
            //message.header.source = packet.remoteEndPoint;
        }

        public void OnSend(object sender, NetworkPacket packet)
        {

        }

        public void OnError(object sender, NetworkPacket packet)
        {
            NetworkErrorType errorType = (NetworkErrorType)sender;
            
        }


        

        //public void AttachNetworkIdentity()
        //{
        //    AttachNetworkIdentity(new NetworkIdentity());
        //}

        //public void AttachNetworkIdentity(NetworkIdentity ni)
        //{
        //    ident = ni;
        //    ident.AttachToProtocol(this);

        //    if (isServer)
        //    {
        //        ident.RegisterServer(socket.sendSocket.LocalEndPoint);
        //    }
        //}


        //public virtual MessageServer ConnectToServer(string userName)
        //{
        //    return (MessageServer)ident.ConnectToServer(userName);
        //}


       


        //public override void OnReceive(object sender, NetworkPacket packet)
        //{
        //    NetworkMessage message = ReadHeader(packet);

        //    packet.messages.Add(message);
        //    message.header.source = packet.remoteEndPoint;

        //    if( message.header.isStream )
        //    {

        //        HandleReceiveStream(message, packet);
        //    }
        //    else
        //    {
        //        HandleReceiveMessage(message, packet);
        //    }



        //}


        //public override void OnSend(object sender, NetworkPacket packet)
        //{
        //}


        //public override void OnError(object sender, NetworkPacket packet)
        //{
        //    NetworkErrorType errorType = (NetworkErrorType)sender;
        //    switch (errorType)
        //    {
        //        case NetworkErrorType.ErrorConnectToServer:
        //            if (OnErrorConnectToServer != null)
        //                OnErrorConnectToServer.Invoke(this, packet);
        //            break;
        //        case NetworkErrorType.ErrorReliableFailed:
        //            if( OnErrorReliableFailed != null )
        //                OnErrorReliableFailed.Invoke(this, packet);
        //            break;
        //        case NetworkErrorType.ErrorNoResponseSTUN:
        //            if (OnErrorSTUNFailed != null)
        //                OnErrorSTUNFailed.Invoke(this, packet);
        //            break;
        //    }
        //}


        //public override void AttachErrorListener(NetworkErrorType errorType, EventHandler<NetworkPacket> func)
        //{
        //    switch (errorType)
        //    {
        //        case NetworkErrorType.ErrorConnectToServer:
        //            OnErrorConnectToServer += func;
        //            break;
        //        case NetworkErrorType.ErrorReliableFailed:
        //            OnErrorReliableFailed += func;
        //            break;
        //        case NetworkErrorType.ErrorNoResponseSTUN:
        //            OnErrorSTUNFailed += func;
        //            break;
        //    }
        //}

        //public override NetworkMessage[] ReadHeaders(NetworkPacket packet)
        //{
        //    uint bits = packet.ReadByte();
        //    //remove flag bits to reveal channel type
        //    bits = bits & ~(StreamFlag | SendTypeFlag | ReliableFlag | ProtocolTypeFlag);

        //    if (bits < 0 || bits >= (uint)MessageType.LAST)
        //    {
        //        NetworkMessage[] msgFailList = new NetworkMessage[1];
        //        msgFailList[0] = (NetworkMessage)channel.CreateMessage(MessageType.Invalid);
        //        return msgFailList;
        //    }

        //    uint msgCount = packet.ReadByte();
        //    NetworkMessage[] msg = new NetworkMessage[msgCount];
        //    for (int i=0; i<msgCount; i++)
        //    {
        //        msg[i] = ReadHeader(packet);
        //    }
        //    return msg;
        //}

       
        
    }
}
