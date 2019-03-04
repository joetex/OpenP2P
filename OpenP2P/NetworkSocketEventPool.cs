using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace OpenP2P
{
    public class NetworkSocketEventPool
    {
        public NetworkBufferPool bufferPool;
        Queue<NetworkSocketEvent> available = new Queue<NetworkSocketEvent>();

        int initialPoolCount = 0;
        int initialBufferSize = 0;
        public int eventCount = 0;

        public NetworkSocketEventPool(int initPoolCount, int initBufferSize)
        {
            initialPoolCount = initPoolCount;
            initialBufferSize = initBufferSize;
            
            bufferPool = new NetworkBufferPool(initPoolCount, initBufferSize);

            for (int i = 0; i < initialPoolCount; i++)
            {
                New();
            }
        }

        /**
         * Add another NetworkBuffer to the Pool
         */
        public void New()
        {
            //Interlocked.Increment(ref eventCount);
            eventCount++;
            NetworkSocketEvent se = new NetworkSocketEvent(0);
            se.SetBuffer(bufferPool.Reserve());
            available.Enqueue(se);
        }

        /**
         * Reserve a NetworkBuffer from this pool.
         */
        public NetworkSocketEvent Reserve()
        {
            NetworkSocketEvent se = null;
            lock (available)
            {
                if (available.Count == 0)
                {
                    New();
                }

                se = available.Dequeue();
            }
            return se;
        }
        
        /**
         * Free a reserved NetworkBuffer from this pool by NetworkBuffer object.
         */
        public void Free(NetworkSocketEvent se)
        {
            lock (available)
            {
                available.Enqueue(se);
            }

        }

        public void Dispose()
        {
            while(available.Count > 0)
            {
                NetworkSocketEvent se = available.Dequeue();
                se.Dispose();
            }
        }
    }
}
