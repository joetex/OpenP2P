using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OpenP2P
{
    public class NetworkThread
    {
        public NetworkProtocol protocol = null;

        public NetworkPacketPool PACKETPOOL = new NetworkPacketPool(NetworkConfig.PacketPoolBufferInitialCount, NetworkConfig.PacketPoolBufferMaxLength);
        public Queue<NetworkPacket> SENDQUEUE = new Queue<NetworkPacket>(NetworkConfig.PacketPoolBufferInitialCount);
        public Queue<NetworkPacket> RECVQUEUE = new Queue<NetworkPacket>(NetworkConfig.PacketPoolBufferInitialCount);
        public Queue<NetworkPacket> RELIABLEQUEUE = new Queue<NetworkPacket>(NetworkConfig.PacketPoolBufferInitialCount);
        public Dictionary<uint, NetworkPacket> ACKNOWLEDGED = new Dictionary<uint, NetworkPacket>();
        
        public List<Thread> SENDTHREADS = new List<Thread>();
        public List<Thread> RECVTHREADS = new List<Thread>();
        public List<Thread> RELIABLETHREADS = new List<Thread>();
        
        public NetworkPacket recvPacket = null;
        public int recvId = 0;
        public int failedReliableCount = 0;
        public int sentBufferSize = 0;

        public void StartNetworkThreads()
        {
            for (int i = 0; i < NetworkConfig.MAX_SEND_THREADS; i++)
            {
                Thread t = new Thread(new ParameterizedThreadStart(SendThread));
                SENDTHREADS.Add(t);
                SENDTHREADS[i].Start(i);
                t.Priority = ThreadPriority.Highest;
            }
            for (int i = 0; i < NetworkConfig.MAX_RECV_THREADS; i++)
            {
                Thread t = new Thread(RecvProcessThread);
                t.Priority = ThreadPriority.Highest;
                RECVTHREADS.Add(t);
                RECVTHREADS[i].Start();
            }
            for (int i = 0; i < NetworkConfig.MAX_RELIABLE_THREADS; i++)
            {
                //RELIABLETHREADS.Add(new Thread(ReliableThread));
                //RELIABLETHREADS[i].Start();
            }

            //UpdatePriority();
        }

       

        public void SendThread(object id)
        {
            NetworkPacket packet = null;
            int queueCount;
            uint sentCount = 0;
            uint packetsPerFrame = 0;
            bool isFirstThread = (int)id == 0;
            while (true)
            {
                if(isFirstThread) 
                    ReliableThread();

                lock (SENDQUEUE)
                {
                    queueCount = SENDQUEUE.Count;
                
                    if( queueCount > 0 )
                        packet = SENDQUEUE.Dequeue();
                }

                if (queueCount == 0)
                {
                    Thread.Sleep(NetworkConfig.ThreadWaitingSleepTime);
                    continue;
                }

                packet.socket.SendFromThread(packet);

                sentBufferSize += packet.byteSent;
                sentCount += (uint)packet.byteSent;
                packetsPerFrame++;
                if( packetsPerFrame > NetworkConfig.ThreadSendSleepPacketsPerFrame )
                {
                    packetsPerFrame = 0;
                    sentCount = 0;
                    Thread.Sleep(NetworkConfig.ThreadWaitingSleepTime);
                    continue;
                }
                if( sentCount > NetworkConfig.ThreadSendSleepPacketSizePerFrame)
                {
                    sentCount = 0;
                    packetsPerFrame = 0;
                    Thread.Sleep(NetworkConfig.ThreadWaitingSleepTime);
                }
            }
        }


        public void UpdatePriority()
        {

            Process p = Process.GetCurrentProcess();
            foreach (ProcessThread pt in p.Threads)
            {
                for(int i=0; i<SENDTHREADS.Count; i++)
                {
                    Thread sendThread = SENDTHREADS[i];
                

                    
                }

                pt.IdealProcessor = 0;
                pt.ProcessorAffinity = (IntPtr)1;
            }
        }


        public void BeginRecvThread(NetworkPacket packet)
        {
            Thread t = new Thread(RecvThread);
            t.Priority = ThreadPriority.Highest;
            RECVTHREADS.Add(t);
            RECVTHREADS[RECVTHREADS.Count-1].Start(packet);
        }
        

        public void RecvThread(object opacket)
        {
            NetworkPacket packet = (NetworkPacket)opacket;
         
            while (true)
            {
                packet.socket.ExecuteListen(packet);

                lock (RECVQUEUE)
                {
                    RECVQUEUE.Enqueue(packet);
                }

                packet = packet.socket.Reserve();
            }
        }

       
        public void ReliableThread()
        {
            int queueCount = 0;
            lock(RELIABLEQUEUE)
            {
                queueCount = RELIABLEQUEUE.Count;
            }
            
            if (queueCount == 0)
                return;

            NetworkPacket packet = null;
            lock (RELIABLEQUEUE)
            {
                packet = RELIABLEQUEUE.Dequeue();
            }

            long difftime;
            bool isAcknowledged;
            long curtime = NetworkTime.Milliseconds();
            bool hasFailed = false;
            bool shouldResend = false;
            NetworkMessage message;

            for(int i=0; i<packet.messages.Count; i++)
            {
                message = packet.messages[i];

                lock (ACKNOWLEDGED)
                {
                    isAcknowledged = ACKNOWLEDGED.Remove(message.header.ackkey);
                }

                if (isAcknowledged)
                {
                    packet.socket.Free(packet);
                    return;
                }

                difftime = curtime - message.header.sentTime;
                if (difftime > packet.retryDelay)
                {
                    if (message.header.retryCount > NetworkConfig.SocketReliableRetryAttempts)
                    {
                        if (message.header.channelType == ChannelType.Server)
                        {
                            packet.socket.Failed(NetworkErrorType.ErrorConnectToServer, "Unable to connect to server.", packet);
                        }
                        else if( message.header.channelType == ChannelType.STUN)
                        {
                            packet.socket.Failed(NetworkErrorType.ErrorNoResponseSTUN, "Unable to connect to server.", packet);
                        }

                        failedReliableCount++;
                        packet.socket.Failed(NetworkErrorType.ErrorReliableFailed, "Failed to deliver " + message.header.retryCount + " packets (" + failedReliableCount + ") times.", packet);

                        hasFailed = true;
                        packet.socket.Free(packet);
                        return;
                    }

                    shouldResend = true;
                    Console.WriteLine("Resending " + message.header.sequence + ", attempt #" + message.header.retryCount);
                    packet.socket.Send(packet);
                    return;
                }
            }
                    

            if( hasFailed )
            {
                
            }
            else if( shouldResend )
            {
                
            }
            
            lock(RELIABLEQUEUE)
            {
                RELIABLEQUEUE.Enqueue(packet);
            }
            
        
            //Thread.Sleep(MIN_RELIABLE_SLEEP_TIME);
            return;
        }


        public void RecvProcessThread()
        {
            int queueCount;
            NetworkPacket packet = null;

            while (true)
            {
                lock (RECVQUEUE)
                {
                    queueCount = RECVQUEUE.Count;
                    if( queueCount > 0 )
                        packet = RECVQUEUE.Dequeue();
                }

                if (queueCount == 0)
                {
                    Thread.Sleep(NetworkConfig.ThreadRecvProcessSleepTime);
                    continue;
                }
               
                packet.socket.InvokeOnRecieve(packet);

                packet.socket.Free(packet);
            }
        }
    }
}
