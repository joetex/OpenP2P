using System;
using System.Collections.Generic;
using System.Net;

namespace OpenP2P
{
    public class NetworkIdentity
    {
        public class PeerIdentity
        {
            public Dictionary<string, EndPoint> endpoints = new Dictionary<string, EndPoint>();
            public ushort id = 0;
            public string userName = "";
            public List<ushort> messageSequence = new List<ushort>((int)MessageType.LAST);

            public PeerIdentity()
            {
                for(int i=0; i<(int)MessageType.LAST; i++)
                {
                    messageSequence.Add(1);
                }
            }
            public void AddEndpoint(string endpoint, EndPoint ep)
            {
                endpoints.Add(endpoint, ep);
            }

            public EndPoint FindEndpoint(string endpoint)
            {
                return endpoints[endpoint];
            }
        }

        public Dictionary<string, PeerIdentity> peersByEndpoint = new Dictionary<string, PeerIdentity>();
        public Dictionary<ushort, PeerIdentity> peersById = new Dictionary<ushort, PeerIdentity>();
        public PeerIdentity local = new PeerIdentity();
        public NetworkProtocol protocol = null;

        public NetworkIdentity() { }
    
       
        public void AttachToProtocol(NetworkProtocol p)
        {
            protocol = p;
            protocol.OnReadHeader += OnReadHeader;
            protocol.OnWriteHeader += OnWriteHeader;
            protocol.AttachRequestListener(MessageType.ConnectToServer, OnConnectToServerRequest);
            protocol.AttachRequestListener(MessageType.ConnectToServer, OnConnectToServerResponse);
            protocol.AttachErrorListener(NetworkErrorType.ErrorConnectToServer, OnErrorConnectToServer);

            //local.id = 0;// ServerGeneratePeerId(protocol.socket.sendSocket.LocalEndPoint);
        }

        

        public void OnWriteHeader(object sender, NetworkStream stream)
        {
            //NetworkStream stream = (NetworkStream)sender;
            stream.Write(stream.header.id);
            stream.Write(stream.header.sequence);
           
            if ( stream.header.isReliable )
            {
                //NetworkConfig.ProfileBegin("GenerateAckKey");
                if ( stream.header.sendType == SendType.Request && stream.retryCount == 0 )
                {
                    stream.ackkey = GenerateAckKey(stream);
                    stream.Write(stream.ackkey);
                }
                else
                {
                    stream.Write(stream.ackkey);
                }
                //NetworkConfig.ProfileEnd("GenerateAckKey");
            }
            

            //Console.WriteLine("WriteHeader AckKey: " + stream.ackkey);
        }
    
        public void OnReadHeader(object sender, NetworkStream stream)
        {
            //NetworkStream stream = (NetworkStream)sender;
            stream.header.id = stream.ReadUShort();
            stream.header.sequence = stream.ReadUShort();
            if (stream.header.isReliable)
                stream.ackkey = stream.ReadULong();
            //Console.WriteLine("ReadHeader AckKey: " + stream.ackkey);
        }

        public NetworkStream ConnectToServer(IPEndPoint ep, string userName)
        {
            local.userName = userName;

            MsgConnectToServer msg = protocol.Create<MsgConnectToServer>();
            msg.requestUsername = userName;
            return protocol.SendReliableRequest(ep, msg);
        }

        //Server receives request from client
        //Create the peer and send response
        public void OnConnectToServerRequest(object sender, NetworkMessage message)
        {
            NetworkStream stream = (NetworkStream)sender;
            PeerIdentity peer = RegisterPeer(stream.remoteEndPoint);
            
            MsgConnectToServer connectMsg = (MsgConnectToServer)message;
            connectMsg.responseConnected = true;
            connectMsg.responsePeerId = peer.id;

            //protocol.SendResponse(stream, connectMsg);
        }

        //Client receives response from server
        public void OnConnectToServerResponse(object sender, NetworkMessage message)
        {
            NetworkStream stream = (NetworkStream)sender;
            MsgConnectToServer connectMsg = (MsgConnectToServer)message;
            RegisterLocal(connectMsg.responsePeerId, stream.socket.sendSocket.LocalEndPoint);
        }

        public void OnErrorConnectToServer(object sender, NetworkStream stream)
        {

        }

        public PeerIdentity RegisterLocal(ushort id, EndPoint ep)
        {
            local = RegisterPeer(id, ep);
            return local;
        }

        public PeerIdentity RegisterServer(EndPoint ep)
        {
            local = RegisterPeer(ep);
            return local;
        }

        public PeerIdentity RegisterPeer(EndPoint ep)
        {
            string endpoint = ep.ToString();
            if (peersByEndpoint.ContainsKey(endpoint))
                return peersByEndpoint[endpoint];
            ushort id = ServerGeneratePeerId(ep);
            return RegisterPeer(id, ep);
        }

        public PeerIdentity RegisterPeer(ushort id, EndPoint ep)
        {
            PeerIdentity identity = null;

            if (peersById.ContainsKey(id))
                return peersById[id];

            string endpoint = ep.ToString();
            if (peersByEndpoint.ContainsKey(endpoint))
                return peersByEndpoint[endpoint];

            identity = new PeerIdentity();
            identity.id = id;
            identity.AddEndpoint(endpoint, ep);
            
            peersById.Add(id, identity);
            peersByEndpoint.Add(endpoint, identity);

            return identity;
        }

        public ushort ServerGeneratePeerId(EndPoint ep)
        {
            string endpoint = ep.ToString();
            if (peersByEndpoint.ContainsKey(endpoint))
                return peersByEndpoint[endpoint].id;

            ushort id = 0;
            int signedid = endpoint.GetHashCode();
            if (signedid < 0)
                signedid = signedid * -1;

            id = (ushort)signedid;

            while( peersById.ContainsKey(id) )
            {
                id += 1;
            }

            return id;
        }

        public bool IdentityExists(ushort id)
        {
            if (id == 0)
                return false;
            if (!peersById.ContainsKey(id))
                return false;
            return true;
        }

        public bool IdentityExists(string endpoint)
        {
            if (endpoint.Length == 0)
                return false;
            if (!peersByEndpoint.ContainsKey(endpoint))
                return false;
            return true;
        }

        Random random = new Random();
        public ulong GenerateAckKey(NetworkStream stream)
        {
            ulong key = 0;
            if( stream.header.id == 0 )
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
        }
    }
}