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
            packetCount++;
            NetworkPacket packet = new NetworkPacket(initialBufferSize);
            //available.Add(packet);
            available.Enqueue(packet);
        }

        /**
         * Reserve a NetworkBuffer from this pool.
         */
        public NetworkPacket Reserve()
        {
            NetworkPacket packet = null;
            //while (packet == null)
            lock (available)
            {
                if (available.Count == 0)
                    New();

                //available.TryTake(out packet);
                //available.TryTake(out packet);
                packet = available.Dequeue();
            }

            if (packet == null)
                return Reserve();

            packet.header.isReliable = false;
            packet.header.sendType = SendType.Message;
            packet.ackkey = 0;
            packet.retryCount = 0;
            packet.sentTime = 0;
            packet.acknowledged = false;

            return packet;
        }

        /**
         * Free a reserved NetworkBuffer from this pool by NetworkBuffer object.
         */
        public void Free(NetworkPacket packet)
        {
            packet.header.isReliable = false;

            lock (available)
            {
                //available.Add(packet);
                available.Enqueue(packet);
            }

        }

        public void Dispose()
        {
            NetworkPacket packet = null;
            while (available.Count > 0)
            {
                //if( available.TryTake(out packet) )
                packet = available.Dequeue();
                //NetworkPacket packet = null;
                //available.TryTake(out packet);
                    packet.Dispose();
            }
        }
    }
}
