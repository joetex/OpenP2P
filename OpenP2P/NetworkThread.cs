using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OpenP2P
{
    class NetworkThread
    {
        public const int MIN_BUFFER_COUNT = 100000;
        public const int MAX_BUFFER_SIZE = 2000;
        public static int MAX_SENDRATE_PERFRAME = 1000;

        //public static NetworkSocketEventPool EVENTPOOL = new NetworkSocketEventPool(MIN_BUFFER_COUNT, MAX_BUFFER_SIZE);
        public static NetworkStreamPool STREAMPOOL = new NetworkStreamPool(MIN_BUFFER_COUNT, MAX_BUFFER_SIZE);

        public static Queue<NetworkStream> SENDQUEUE = new Queue<NetworkStream>();
        public static Queue<NetworkStream> RECVQUEUE = new Queue<NetworkStream>();

        public static Thread SENDTHREAD = new Thread(NetworkThread.SendThread);
        public static Thread RECVTHREAD = new Thread(NetworkThread.RecvThread);

        public static void StartNetworkThreads()
        {
            SENDTHREAD.Start();
            RECVTHREAD.Start();
        }

        public static void SendThread()
        {
            NetworkStream stream = null;
            int queueCount = 0;

            while (true)
            {
                lock (SENDQUEUE)
                {
                    queueCount = SENDQUEUE.Count;
                    if (queueCount > 0)
                        stream = SENDQUEUE.Dequeue();
                }

                //sleep if empty, to avoid 100% cpu
                if (queueCount == 0)
                {
                    Thread.Sleep(1);
                    continue;
                }

                //avoid filling up the network card's RING buffer
                //important on virtual servers with lower buffer settings
                if (queueCount % MAX_SENDRATE_PERFRAME == 0)
                    Thread.Sleep(1);

                stream.socket.ExecuteSend(stream);
            }
        }

        public static void RecvThread()
        {
            NetworkStream stream = null;
            int queueCount = 0;

            while (true)
            {
                lock (RECVQUEUE)
                {
                    queueCount = RECVQUEUE.Count;
                    if (queueCount > 0)
                        stream = RECVQUEUE.Dequeue();
                }

                //sleep if empty, to avoid 100% cpu
                if (queueCount == 0)
                {
                    Thread.Sleep(1);
                    continue;
                }

                stream.socket.ExecuteListen(stream);
            }
        }
    }
}
