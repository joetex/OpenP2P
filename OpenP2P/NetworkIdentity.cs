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

            public ushort NextSequence(NetworkMessage message)
            {
                int index = (int)message.messageType;
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

        public NetworkIdentity() { }
    
       
        public void AttachToProtocol(NetworkProtocol p)
        {
            protocol = p;
            protocol.OnReadHeader += OnReadHeader;
            protocol.OnWriteHeader += OnWriteHeader;
            protocol.AttachMessageListener(MessageType.ConnectToServer, OnMessageConnectToServer);
            protocol.AttachMessageListener(MessageType.ConnectToServer, OnResponseConnectToServer);
            protocol.AttachErrorListener(NetworkErrorType.ErrorConnectToServer, OnErrorConnectToServer);

            //local.id = 0;// ServerGeneratePeerId(protocol.socket.sendSocket.LocalEndPoint);
        }

       
        public void OnWriteHeader(object sender, NetworkStream stream)
        {
            stream.Write(stream.header.id);
        }
    
        public void OnReadHeader(object sender, NetworkStream stream)
        {
            stream.header.id = stream.ReadUShort();
        }

        public NetworkStream ConnectToServer(IPEndPoint ep, string userName)
        {
            local.userName = userName;

            MsgConnectToServer msg = protocol.Create<MsgConnectToServer>();
            msg.msgUsername = userName;
            return protocol.SendReliableMessage(ep, msg);
        }

        //Server receives message from client
        //Create the peer and send response
        public void OnMessageConnectToServer(object sender, NetworkMessage message)
        {
            NetworkStream stream = (NetworkStream)sender;
            PeerIdentity peer = RegisterPeer(stream.remoteEndPoint);
            if( peer == null )
            {
                stream.socket.Failed(NetworkErrorType.ErrorMaxIdentitiesReached, "Peer identity unable to be created.", stream);
                return;
            }

            MsgConnectToServer connectMsg = (MsgConnectToServer)message;
            connectMsg.responseConnected = true;
            connectMsg.responsePeerId = peer.id;
            
            protocol.SendResponse(stream, connectMsg);

            //protocol.SendResponse(stream, connectMsg);
        }

        //Client receives response from server
        public void OnResponseConnectToServer(object sender, NetworkMessage message)
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

        public const int MAX_IDENTITIES = 65534;
        /// <summary>
        /// Server Generate Peer Identity
        /// Generates a random ushort number in range [1, 65534] to identify a user.
        /// Prevent infinite loop by locking tests to 65534 attempts;
        /// </summary>
        /// <param name="ep">Endpoint of User</param>
        /// <returns></returns>
        public ushort ServerGeneratePeerId(EndPoint ep)
        {
            /*string endpoint = ep.ToString();
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
            */
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