﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenP2P
{
    public class NetworkStreamPool
    {
        Queue<NetworkStream> available = new Queue<NetworkStream>();

        int initialPoolCount = 0;
        int initialBufferSize = 0;
        public int streamCount = 0;

        public NetworkStreamPool(int initPoolCount, int initBufferSize)
        {
            initialPoolCount = initPoolCount;
            initialBufferSize = initBufferSize;
            
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
            streamCount++;
            NetworkStream stream = new NetworkStream(initialBufferSize);
            available.Enqueue(stream);
        }

        /**
         * Reserve a NetworkBuffer from this pool.
         */
        public NetworkStream Reserve()
        {
            NetworkStream stream = null;
            lock (available)
            {
                if (available.Count == 0)
                    New();

                stream = available.Dequeue();
            }
            return stream;
        }

        /**
         * Free a reserved NetworkBuffer from this pool by NetworkBuffer object.
         */
        public void Free(NetworkStream stream)
        {
            lock (available)
            {
                available.Enqueue(stream);
            }

        }

        public void Dispose()
        {
            while (available.Count > 0)
            {
                NetworkStream stream = available.Dequeue();
                stream.Dispose();
            }
        }
    }
}