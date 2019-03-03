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

        public NetworkStream stream = new NetworkStream(null);
        public NetworkSocket socket = null;
        NetworkBuffer buffer = null;
        public NetworkBuffer Buffer { get { return buffer; } }
        public byte[] GetByteBuffer() { return buffer.buffer; }

        public int bufferLength = 0;

        //the reserved space for our network data
        public SocketAsyncEventArgs args;

        public NetworkSocketEvent(int _id)
        {
            id = _id;
            args = new SocketAsyncEventArgs();
        }

        public void InitStream()
        {
            stream.Attach(this);
        }

        public void SetBuffer(NetworkBuffer _buffer)
        {
            buffer = _buffer;
            if (buffer == null)
                return;
            args.SetBuffer(buffer.buffer, 0, buffer.buffer.Length);
        }
        
        public void SetBufferBytes(byte[] data)
        {
            for(int i=0; i<data.Length; i++)
            {
                args.Buffer[i] = data[i];
            }
            //buffer = null;
            //args.SetBuffer(data, 0, data.Length);
        }

        public void SetBufferLength(int length)
        {
            bufferLength = length;
            args.SetBuffer(0, length);

            stream.SetBufferLength(length);
        }

        public void Dispose()
        {
            args.Dispose();
        }
    }
}
