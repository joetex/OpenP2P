using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace OpenP2P
{
    public class NetworkPeer
    {
        public ushort id = 0;
        public string userName = "";

        //public Dictionary<string, EndPoint> endpoints = new Dictionary<string, EndPoint>();
        public EndPoint endpoint = null;
        public List<ushort> messageSequence = new List<ushort>((int)ChannelType.LAST);

        public Queue<NetworkMessage> outgoing = new Queue<NetworkMessage>();
        public Queue<NetworkMessage> incoming = new Queue<NetworkMessage>();

        public NetworkProtocol protocol;

        public NetworkPeer(NetworkProtocol p)
        {
            protocol = p;
            for (int i = 0; i < (int)ChannelType.LAST; i++)
            {
                messageSequence.Add(0);
            }
        }

        public void Send(NetworkMessage message)
        {
            lock(outgoing)
            {
                outgoing.Enqueue(message);
            }
        }

        public void FreeMessage(NetworkMessage message)
        {

        }

        public void ProcessOutgoing()
        {

        }

        public void ForwardIncoming(NetworkMessage message)
        {

        }

        public void AddEndpoint(EndPoint ep)
        {
            endpoint = ep;
            //endpoints.Add(endpoint, ep);
        }

        public EndPoint GetEndpoint()
        {
            return endpoint;
            //return endpoints[endpoint];
        }

        public ushort NextSequence(NetworkMessage message)
        {
            int index = (int)message.header.channelType;
            uint iSequence = ((uint)messageSequence[index] + 1) % 65534;
            messageSequence[index] = (ushort)iSequence;
            return messageSequence[index];
        }

        

    }
}
