using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OpenP2P
{
    public class NetworkBufferPool
    {
        Queue<NetworkBuffer> available = new Queue<NetworkBuffer>();
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
            //Interlocked.Increment(ref bufferCount);
            NetworkBuffer buffer = new NetworkBuffer(0, initialBufferLength);
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
            return buffer;
        }

        /**
         * Free a reserved NetworkBuffer from this pool by NetworkBuffer object.
         */
        public void Free(NetworkBuffer buffer)
        {
            available.Enqueue(buffer);
        }
        
    }
}
