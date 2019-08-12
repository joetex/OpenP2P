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
            public List<ushort> messageSequence = new List<ushort>((int)ChannelType.LAST);

            public PeerIdentity()
            {
                for(int i=0; i<(int)ChannelType.LAST; i++)
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

            public ushort NextSequence(NetworkMessage message)
            {
                int index = (int)message.header.channelType;
                uint iSequence = ((uint)messageSequence[index] + 1) % 65534;
                messageSequence[index] = (ushort)iSequence;
                return messageSequence[index];
            }
        }

        public Dictionary<string, PeerIdentity> peersByEndpoint = new Dictionary<string, PeerIdentity>();
        public Dictionary<ushort, PeerIdentity> peersById = new Dictionary<ushort, PeerIdentity>();
        public PeerIdentity local = new PeerIdentity();
        public PeerIdentity server = null;
        public NetworkProtocol protocol = null;
        public Random random = new Random();
        public const int MAX_IDENTITIES = 65534;

        public NetworkIdentity() { }
    
       
        public void AttachToProtocol(NetworkProtocol p)
        {
            protocol = p;
            protocol.OnReadHeader += OnReadHeader;
            protocol.OnWriteHeader += OnWriteHeader;
            protocol.AttachMessageListener(ChannelType.ConnectToServer, OnMessageConnectToServer);
            protocol.AttachResponseListener(ChannelType.ConnectToServer, OnResponseConnectToServer);
            protocol.AttachErrorListener(NetworkErrorType.ErrorConnectToServer, OnErrorConnectToServer);

            //local.id = 0;// ServerGeneratePeerId(protocol.socket.sendSocket.LocalEndPoint);
        }

       
        public void OnWriteHeader(object sender, NetworkMessage message)
        {
            NetworkPacket packet = (NetworkPacket)sender;
            packet.Write(message.header.id);
        }
    
        public void OnReadHeader(object sender, NetworkMessage message)
        {
            NetworkPacket packet = (NetworkPacket)sender;
            message.header.id = packet.ReadUShort();
            message.header.peer = FindPeer(message.header.id);
        }

        public PeerIdentity FindPeer(ushort id)
        {
            if (peersById.ContainsKey(id))
                return peersById[id];
            return null;
        }

        public NetworkPacket ConnectToServer(IPEndPoint ep, string userName)
        {
            local.userName = userName;

            //MsgConnectToServer msg = protocol.Create<MsgConnectToServer>();
            MsgConnectToServer msg = protocol.Create<MsgConnectToServer>();
            msg.msgUsername = userName;
            return protocol.SendReliableMessage(ep, msg);
        }

        //Server receives message from client
        //Create the peer and send response
        public void OnMessageConnectToServer(object sender, NetworkMessage message)
        {
            NetworkPacket packet = (NetworkPacket)sender;

            PeerIdentity peer;
            if( message.header.id == 0 )
            {
                peer = RegisterPeer(message.header.source);
            }
            else
            {
                peer = FindPeer(message.header.id);
            }
            
            if( peer == null )
            {
                protocol.socket.Failed(NetworkErrorType.ErrorMaxIdentitiesReached, "Peer identity unable to be created.", packet);
                return;
            }

            MsgConnectToServer response = protocol.Create<MsgConnectToServer>();// message;
            response.responseConnected = true;
            response.responsePeerId = peer.id;
            
            protocol.SendResponse(message, response);
        }

        //Client receives response from server
        public void OnResponseConnectToServer(object sender, NetworkMessage message)
        {
            NetworkPacket packet = (NetworkPacket)sender;
            MsgConnectToServer connectMsg = (MsgConnectToServer)message;
            RegisterLocal(connectMsg.responsePeerId, protocol.socket.sendSocket.LocalEndPoint);
        }

        public void OnErrorConnectToServer(object sender, NetworkPacket packet)
        {

        }

        public PeerIdentity RegisterLocal(ushort id, EndPoint ep)
        {
            local = RegisterPeer(id, ep);
            return local;
        }
        
        public PeerIdentity RegisterServer(EndPoint ep)
        {
            ushort id = ServerGeneratePeerId(ep);
            server = RegisterPeer(id, ep);
            return server;
        }

        public PeerIdentity RegisterPeer(EndPoint ep)
        {
            //string endpoint = ep.ToString();
            //if (peersByEndpoint.ContainsKey(endpoint))
            //    return peersByEndpoint[endpoint];
            ushort id = ServerGeneratePeerId(ep);
            if( id == 0 )
            {
                return null;
            }
            return RegisterPeer(id, ep);
        }

        public PeerIdentity RegisterPeer(ushort id, EndPoint ep)
        {
            PeerIdentity identity = null;
            string endpoint = ep.ToString();

            if (peersById.ContainsKey(id))
            {
                identity = peersById[id];
                if (!peersByEndpoint.ContainsKey(endpoint))
                    peersByEndpoint.Add(endpoint, identity);
                return identity;
            }
                
            if (peersByEndpoint.ContainsKey(endpoint))
                return peersByEndpoint[endpoint];

            identity = new PeerIdentity();
            identity.id = id;
            identity.AddEndpoint(endpoint, ep);
            
            peersById.Add(id, identity);
            peersByEndpoint.Add(endpoint, identity);

            return identity;
        }

        
        /// <summary>
        /// Server Generate Peer Identity
        /// Generates a random ushort number in range [1, 65534] to identify a user.
        /// Prevent infinite loop by locking tests to 65534 attempts;
        /// </summary>
        /// <param name="ep">Endpoint of User</param>
        /// <returns></returns>
        public ushort ServerGeneratePeerId(EndPoint ep)
        {
            int id = random.Next(1, MAX_IDENTITIES);
            int testId = id;
            int increment = 0;
            while (peersById.ContainsKey((ushort)testId))
            {
                testId = (id + (++increment)) % MAX_IDENTITIES;
                if (increment > MAX_IDENTITIES)
                    return 0;
            }
            return (ushort)id;
        }

        public bool IdentityExists(ushort id)
        {
            if (id == 0)
                return false;
            if (!peersById.ContainsKey(id))
                return false;
            return true;
        }

      
    }
}