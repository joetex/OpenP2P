
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
        Queue<NetworkMessage> available = new Queue<NetworkMessage>();
        //ConcurrentBag<NetworkPacket> available = new ConcurrentBag<NetworkPacket>();
        int initialPoolCount = 0;
        public int count = 0;

        public NetworkMessagePool(int initPoolCount)
        {
            initialPoolCount = initPoolCount;

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
            count++;
            NetworkMessage message = new NetworkMessage();
            //available.Add(packet);
            available.Enqueue(message);
        }

        /**
         * Reserve a NetworkBuffer from this pool.
         */
        public NetworkMessage Reserve()
        {
            NetworkMessage message = null;
            //while (packet == null)
            lock (available)
            {
                if (available.Count == 0)
                    New();

                //available.TryTake(out packet);
                //available.TryTake(out packet);
                message = available.Dequeue();
            }

            if (message == null)
                return Reserve();

           

            return message;
        }

        /**
         * Free a reserved NetworkBuffer from this pool by NetworkBuffer object.
         */
        public void Free(NetworkMessage message)
        {

            lock (available)
            {
                //available.Add(packet);
                available.Enqueue(message);
            }

        }

        public void Dispose()
        {
            NetworkMessage message = null;
            while (available.Count > 0)
            {
                //if( available.TryTake(out packet) )
                message = available.Dequeue();
                //NetworkPacket packet = null;
                //available.TryTake(out packet);
                //message.Dispose();
            }
        }
    }
}


