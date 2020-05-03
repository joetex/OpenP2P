using System;
using System.Collections.Generic;
using System.Net;

namespace OpenP2P
{
    public class NetworkIdentity
    {
        public Dictionary<string, NetworkPeer> peersByEndpoint = new Dictionary<string, NetworkPeer>();
        public Dictionary<ushort, NetworkPeer> peersById = new Dictionary<ushort, NetworkPeer>();
        public NetworkPeer local = null;
        public NetworkPeer server = null;
        public NetworkProtocol protocol = null;
        public Random random = new Random();
        public const int MAX_IDENTITIES = 65534;

        public bool hasConnected = false;

        public NetworkIdentity() { }
    
       
        public void AttachToProtocol(NetworkProtocol p)
        {
            local = new NetworkPeer(p);

            protocol = p;
            protocol.OnReadHeader += OnReadHeader;
            protocol.OnWriteHeader += OnWriteHeader;
            protocol.AttachRequestListener(ChannelType.Server, OnRequestConnectToServer);
            protocol.AttachResponseListener(ChannelType.Server, OnResponseConnectToServer);
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

        

        public NetworkMessage ConnectToServer(string userName)
        {
            local.userName = userName;

            //MsgConnectToServer msg = protocol.Create<MsgConnectToServer>();
            MessageServer msg = protocol.Create<MessageServer>();
            msg.method = MessageServer.ServerMethod.CONNECT; 
            msg.request.connect.username = userName;
          
            return msg;
        }

        
        //Server receives message from client
        //Create the peer and send response
        public void OnRequestConnectToServer(object sender, NetworkMessage message)
        {
            NetworkPacket packet = (NetworkPacket)sender;

            NetworkPeer peer;
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
            MessageServer incoming = (MessageServer)message;
            if( !hasConnected )
            {
                hasConnected = true;
                Console.WriteLine(message.header.source.ToString());
                Console.WriteLine(incoming.request.connect.username);

            }

            //MessageServer response = protocol.Create<MessageServer>();// message;
            //response.responseConnected = true;
            //response.responsePeerId = peer.id;
            
            //protocol.SendResponse(message, response);
        }

        //Client receives response from server
        public void OnResponseConnectToServer(object sender, NetworkMessage message)
        {
            NetworkPacket packet = (NetworkPacket)sender;
            MessageServer connectMsg = (MessageServer)message;

            if( local.id == 0 )
                RegisterLocal(connectMsg.response.connect.peerId, protocol.socket.sendSocket.LocalEndPoint);
        }

        public void OnErrorConnectToServer(object sender, NetworkPacket packet)
        {

        }

        public NetworkPeer FindPeer(ushort id)
        {
            if (peersById.ContainsKey(id))
                return peersById[id];
            return null;
        }

        public NetworkPeer RegisterLocal(ushort id, EndPoint ep)
        {
            local = RegisterPeer(id, ep);
            return local;
        }
        
        public NetworkPeer RegisterServer(EndPoint ep)
        {
            ushort id = ServerGeneratePeerId(ep);
            server = RegisterPeer(id, ep);
            return server;
        }

        public NetworkPeer RegisterPeer(EndPoint ep)
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

        public NetworkPeer RegisterPeer(ushort id, EndPoint ep)
        {
            NetworkPeer identity = null;
            

            if (peersById.ContainsKey(id))
            {
                identity = peersById[id];
                return identity;
            }

            string endpoint = ep.ToString();
            if (peersByEndpoint.ContainsKey(endpoint))
                return peersByEndpoint[endpoint];

            identity = new NetworkPeer(protocol);
            identity.id = id;
            identity.AddEndpoint(ep);
            
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