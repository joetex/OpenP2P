using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenP2P
{
    public class NetworkProtocol
    {
        public enum MessageType
        {
            //interest mapping data sent to server
            //Peers will be connected together at higher priorities based on the 
            // "interest" mapping to a QuadTree (x, y, width, height) 
            Heartbeat,

            //keep connection open with peers
            PeerHeartbeat,

            //Server specific
            RegisterAsPeer,
            
            //NAT Traversal Protocol between Local/Server/Peers
            RequestPeers,       //Request peers immediately
            RequestPeerRetry,   //ip:port combo failed, request a correction
            ReceivePeer,        //create connection to ip/port received by server
            ReceivePeerRequest, //peer sent new ip/port 

            //Used for realtime data, i.e. Position, Rotation, etc.
            SendRaw,
            ReceiveRaw,

            //Used for event messages, i.e. "Player 1 died".
            SendMessage,
            ReceiveMessage,

            //Used for executing RPC functions, i.e. [RPC] PlayerSpawn()
            SendRPC, 
            ReceiveRPC
        }
        
        public NetworkProtocol() { }
        
        public void Write_Heartbeat(NetworkStream stream)
        {
            stream.WriteHeader(MessageType.Heartbeat);
        }

        public void Read_Heartbeat(NetworkStream stream)
        {
            MessageType mt = stream.ReadHeader();
        }
    }
}
