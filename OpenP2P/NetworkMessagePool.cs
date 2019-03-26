using System;
namespace OpenP2P
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    namespace OpenP2P
    {
        public class NetworkMessagePool
        {
            //Queue<NetworkStream> available = new Queue<NetworkStream>();
            ConcurrentBag<NetworkMessage> available = new ConcurrentBag<NetworkMessage>();
            int initialPoolCount = 0;
            int initialBufferSize = 0;
            public int streamCount = 0;

            public NetworkMessagePool(int initPoolCount)
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
                NetworkMessage message = new NetworkMessage();
                available.Add(stream);
                //available.Enqueue(stream);
            }

            /**
             * Reserve a NetworkBuffer from this pool.
             */
            public NetworkStream Reserve()
            {
                NetworkStream stream = null;
                while (stream == null)
                //lock (available)
                {
                    if (available.Count == 0)
                        New();

                    available.TryTake(out stream);
                    //stream = available.Dequeue();
                }

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
                stream.message.header.isReliable = false;

                //lock (available)
                {
                    available.Add(stream);
                    //available.Enqueue(stream);
                }

            }

            public void Dispose()
            {
                while (available.Count > 0)
                {
                    //NetworkStream stream = available.Dequeue();
                    NetworkStream stream = null;
                    available.TryTake(out stream);
                    stream.Dispose();
                }
            }
        }
    }

}
