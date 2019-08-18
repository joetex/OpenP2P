using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenP2P
{
    public class NetworkPacketPool
    {
        Queue<NetworkPacket> available = new Queue<NetworkPacket>();
        //ConcurrentBag<NetworkPacket> available = new ConcurrentBag<NetworkPacket>();
        int initialPoolCount = 0;
        int initialBufferSize = 0;
        public int packetCount = 0;

        public NetworkPacketPool(int initPoolCount, int initBufferSize)
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
            NetworkPacket packet = new NetworkPacket(initialBufferSize);
            //lock(available)
            {
                packetCount++;
                available.Enqueue(packet);
            }
               
        }

        /**
         * Reserve a NetworkBuffer from this pool.
         */
        public NetworkPacket Reserve()
        {
            NetworkPacket packet = null;
            //while (packet == null)

            int count = 0;
            lock(available)
            {
                count = available.Count;
            //}

                if (count == 0)
                    New();

            //lock (available)
            //{
                packet = available.Dequeue();
            }

            if (packet == null)
                return Reserve();
            
            packet.messages.Clear();

            return packet;
        }

        /**
         * Free a reserved NetworkBuffer from this pool by NetworkBuffer object.
         */
        public void Free(NetworkPacket packet)
        {
            lock (available)
            {
                available.Enqueue(packet);
            }
        }

        public void Dispose()
        {
            NetworkPacket packet = null;
            while (available.Count > 0)
            {
                packet = available.Dequeue();
                packet.Dispose();
            }
        }
    }
}
