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
            public List<int> messageSequence = new List<int>();

            public void AddEndpoint(string endpoint, EndPoint ep)
            {
                endpoints.Add(endpoint, ep);
            }

            public EndPoint FindEndpoint(string endpoint)
            {
                return endpoints[endpoint];
            }
        }

        public NetworkIdentity()
        {
        }

        public Dictionary<string, PeerIdentity> peersByEndpoint = new Dictionary<string, PeerIdentity>();
        public Dictionary<ushort, PeerIdentity> peersById = new Dictionary<ushort, PeerIdentity>();

        public PeerIdentity RegisterPeer(ushort id, EndPoint ep)
        {
            PeerIdentity identity = null;

            if (peersById.ContainsKey(id))
            {
                return peersById[id];
            }


            string endpoint = ep.ToString();
            if (peersByEndpoint.ContainsKey(endpoint))
                return peersByEndpoint[endpoint];

            identity = new PeerIdentity();
            identity.AddEndpoint(endpoint, ep);

            peersById.Add(id, identity);
            peersByEndpoint.Add(endpoint, identity);

            return identity;
        }

        public void OnWriteHeader(object sender, NetworkMessage message)
        {
            NetworkStream stream = (NetworkStream)sender;

            if (message.header.sendType == SendType.Request )
            {
                if (message.header.messageType != MessageType.ConnectToServer)
                    AddSignature(stream);
            }
            else
            {
                if (message.header.messageType != MessageType.ConnectToServer)
                    AddSignature(stream);
            }

        }

        public void AddSignature(NetworkStream stream)
        {

        }

        public void OnReadHeader(object sender, NetworkMessage message)
        {

        }

        public void OnConnectToServerRequest(object sender, NetworkMessage message)
        {

        }
        public void OnConnectToServerResponse(object sender, NetworkMessage message)
        {

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
    }
}