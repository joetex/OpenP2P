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

        public NetworkBuffer buffer = null;

        //the reserved space for our network data
        public SocketAsyncEventArgs args;

        public NetworkSocketEvent(int _id)
        {
            id = _id;
            args = new SocketAsyncEventArgs();
        }

        public void SetBuffer(NetworkBuffer _buffer)
        {
            buffer = _buffer;
            if (buffer == null)
                return;
            args.SetBuffer(buffer.buffer, 0, buffer.buffer.Length);
        }
        
        public void SetBuffer(byte[] data)
        {
            buffer = null;
            args.SetBuffer(data, 0, data.Length);
        }

        public void Dispose()
        {
            args.Dispose();
        }
    }
}
