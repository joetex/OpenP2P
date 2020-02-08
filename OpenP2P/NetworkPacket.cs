
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace OpenP2P
{
    /**
     * Network Packet
     * Read/Write directly to the socket's byte buffer for sending and receiving pipeline.
     * Extensions may be made to support more types.
     */
    public class NetworkPacket : NetworkSerializer
    {
        //public NetworkPacketSerializer serializer = new NetworkPacketSerializer();
        public NetworkSocket socket = null;
        public EndPoint remoteEndPoint;
        public String remoteEndPointStr;
        public EndPoint RemoteEndPoint
        {
            get { return remoteEndPoint; }
            set
            {
                remoteEndPointStr = remoteEndPoint.ToString();
                remoteEndPoint = value;
            }
        }

        public NetworkSocket.NetworkIPType networkIPType = NetworkSocket.NetworkIPType.IPv4;
        public List<NetworkMessage> messages = new List<NetworkMessage>();

        public long retryDelay = NetworkConfig.SocketReliableRetryDelay;

        public NetworkErrorType lastErrorType = NetworkErrorType.None;
        public string lastErrorMessage = "";

        public NetworkPacket(int initBufferSize) : base(initBufferSize)
        {
        }

        public void Complete()
        {
            SetBufferLength(byteLength);
        }

        public void Complete(int bytesTransferred)
        {
            SetBufferLength(bytesTransferred);
        }
        

        public void Dispose()
        {

        }

        public void Error(NetworkErrorType errorType, string errorMsg)
        {
            lastErrorMessage = errorMsg;
            lastErrorType = errorType;
        }
    }
}
