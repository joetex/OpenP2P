
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
    public partial class NetworkPacket
    {
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

        public NetworkMessage message = null;
        public NetworkMessage.Header header = new NetworkMessage.Header();
        public uint ackkey = 0;
        public long sentTime = 0;
        public int retryCount = 0;
        public bool acknowledged = false;
        
        public byte[] buffer;
        public byte[] ByteBuffer { get { return buffer; } }
        public int byteLength = 0; //total size of data 
        public int bytePos = 0; //current read position
        public int byteSent = 0;

        public NetworkErrorType lastErrorType = NetworkErrorType.None;
        public string lastErrorMessage = "";

        
        public NetworkPacket(int initBufferSize)
        {
            buffer = new byte[initBufferSize];
        }
        
        public void SetBufferLength(int length)
        {
            byteLength = length;
            bytePos = 0;
        }

        public void Reset()
        {
            remoteEndPoint = socket.anyHost4;
            acknowledged = false;
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
