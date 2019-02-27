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
        private readonly object poolLock = new object();

        public NetworkBufferPool bufferPool;

        Queue<NetworkSocketEvent> available = new Queue<NetworkSocketEvent>();
        Dictionary<int, NetworkSocketEvent> used = new Dictionary<int, NetworkSocketEvent>();

        int initialPoolCount = 0;
        int initialBufferSize = 0;
        int eventCount = 0;

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
            Interlocked.Increment(ref eventCount);
            NetworkSocketEvent socketEvent = new NetworkSocketEvent(eventCount);
            available.Enqueue(socketEvent);
        }

        /**
         * Reserve a NetworkBuffer from this pool.
         */
        public NetworkSocketEvent Reserve(byte[] data)
        {
            NetworkSocketEvent socketEvent = null;
            lock (available)
            {
                if (available.Count == 0)
                {
                    New();
                }

                socketEvent = available.Dequeue();
                used.Add(socketEvent.id, socketEvent);
            }
            return socketEvent;

        }
        public NetworkSocketEvent Reserve(bool withBuffer)
        {
            NetworkSocketEvent socketEvent = null;
            lock (available)
            {


                if (available.Count == 0)
                {
                    New();
                }

                socketEvent = available.Dequeue();
                if (withBuffer)
                {
                    NetworkBuffer buffer = bufferPool.Reserve();
                    socketEvent.SetBuffer(buffer);
                }
                else
                {
                    socketEvent.SetBuffer(null);
                }
                Console.WriteLine("Reserving Socket Event: " + socketEvent.id);

                used.Add(socketEvent.id, socketEvent);
            }
            return socketEvent;
        }

        

        /**
         * Free a reserved NetworkBuffer from this pool by NetworkBuffer object.
         */
        public void Free(NetworkSocketEvent socketEvent)
        {
            lock (available)
            {
                if (socketEvent.buffer != null)
                    bufferPool.Free(socketEvent.buffer);

                Console.WriteLine("Freeing Socket Event: " + socketEvent.id);

                //if( used.ContainsKey(socketEvent.id))
                {
                    used.Remove(socketEvent.id);
                    available.Enqueue(socketEvent);
                }
            }

        }

        /**
         * Free a reserved NetworkBuffer from this pool by id.
         */
        public void Free(int id)
        {
            NetworkSocketEvent socketEvent = used[id];
            Free(socketEvent);
        }

        public void Dispose()
        {
            while(available.Count > 0)
            {
                NetworkSocketEvent socketEvent = available.Dequeue();
                socketEvent.Dispose();
            }
        }
    }
}
