using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenP2P
{
    public class NetworkBufferPool
    {
        Queue<NetworkBuffer> available = new Queue<NetworkBuffer>();
        Dictionary<int, NetworkBuffer> used = new Dictionary<int, NetworkBuffer>();

        private int initialPoolCount = 0;
        private int initialBufferLength = 0;
        private int bufferCount = 0;

        public NetworkBufferPool(int initPoolCount, int initBufferLength)
        {
            initialPoolCount = initPoolCount;
            initialBufferLength = initBufferLength;

            for (int i=0; i< initialPoolCount; i++)
            {
                New();
            }
        }

        /**
         * Add another NetworkBuffer to the Pool
         */
        public void New()
        {
            NetworkBuffer buffer = new NetworkBuffer(++bufferCount, initialBufferLength);
            available.Enqueue(buffer);
        }

        /**
         * Reserve a NetworkBuffer from this pool.
         */
        public NetworkBuffer Reserve()
        {
            if( available.Count == 0 )
            {
                New();
            }

            NetworkBuffer buffer = available.Dequeue();
            used.Add(buffer.id, buffer);

            return buffer;
        }

        /**
         * Free a reserved NetworkBuffer from this pool by NetworkBuffer object.
         */
        public void Free(NetworkBuffer buffer)
        {
            used.Remove(buffer.id);
            available.Enqueue(buffer);
        }

        /**
         * Free a reserved NetworkBuffer from this pool by id.
         */
        public void Free(int id)
        {
            NetworkBuffer buffer = used[id];
            Free(buffer);
        }
    }
}
