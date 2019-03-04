using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace OpenP2P
{
    public class NetworkSocketEvent
    {
        //used to index/find in NetworkBufferPool
        public int id = 0;

        public NetworkSocket socket;
        public NetworkStream stream;
        
        //the reserved space for our network data
        public SocketAsyncEventArgs args;

        public NetworkSocketEvent(int _id)
        {
            id = _id;
            args = new SocketAsyncEventArgs();
            stream = new NetworkStream(this); 
        }
        
        public void SetBuffer(NetworkBuffer _buffer)
        {
            stream.SetBuffer(_buffer);
            args.SetBuffer(_buffer.buffer, 0, _buffer.buffer.Length);
        }
        
        public void SetBufferLength(int length)
        {
            stream.SetBufferLength(length);
            args.SetBuffer(0, length);
        }

        public void Dispose()
        {
            args.Dispose();
        }
    }
}
