using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenP2P
{
    public class NetworkStreamPool
    {
        Queue<NetworkStream> available = new Queue<NetworkStream>();
        //ConcurrentBag<NetworkStream> available = new ConcurrentBag<NetworkStream>();
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
            //available.Add(stream);
            available.Enqueue(stream);
        }

        /**
         * Reserve a NetworkBuffer from this pool.
         */
        public NetworkStream Reserve()
        {
            NetworkStream stream = null;
            //while (stream == null)
            lock (available)
            {
                if (available.Count == 0)
                    New();

                //available.TryTake(out stream);
                stream = available.Dequeue();
            }

            if (stream == null)
                return Reserve();

            stream.header.isReliable = false;
            stream.header.sendType = SendType.Request;
            stream.ackkey = 0;
            stream.retryCount = 0;
            stream.sentTime = 0;
            stream.acknowledged = false;

            return stream;
        }

        /**
         * Free a reserved NetworkBuffer from this pool by NetworkBuffer object.
         */
        public void Free(NetworkStream stream)
        {
            stream.header.isReliable = false;

            lock (available)
            {
                //available.Add(stream);
                available.Enqueue(stream);
            }

        }

        public void Dispose()
        {
            while (available.Count > 0)
            {
                NetworkStream stream = available.Dequeue();
                //NetworkStream stream = null;
                //available.TryTake(out stream);
                stream.Dispose();
            }
        }
    }
}
