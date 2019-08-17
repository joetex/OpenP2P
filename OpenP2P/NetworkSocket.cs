
using System;
using System.Net;
using System.Net.Sockets;

namespace OpenP2P
{
    public class NetworkSocket 
    {
        public Socket socket4;
        public IPEndPoint anyHost4;

        public Socket socket6;
        public IPEndPoint anyHost6;

        public Socket sendSocket = null;

        public NetworkThread thread = null;

        public enum NetworkIPType
        {
            Any,
            IPv4,
            IPv6,
            LAST
        }
        
        public event EventHandler<NetworkPacket> OnReceive;
        public event EventHandler<NetworkPacket> OnSend;
        public event EventHandler<NetworkPacket> OnError;

        public static bool supportsIpv6 = false;
        public static bool supportsIpv4 = false;

        public NetworkSocket(string localIP, int localPort)
        {
            Setup(localIP, localPort);
        }

        public void Setup(string localIP, int localPort)
        {
            thread = new NetworkThread();
            thread.StartNetworkThreads();

            if (IsSupportIpv4())
                SetupIPv4(localIP, localPort);

            if (IsSupportIpv6())
                SetupIPv6(localIP, localPort);

            //thread.UpdatePriority();
        }

        
        public bool IsSupportIpv6()
        {
            if (supportsIpv6)
                return supportsIpv6;

            IPAddress[] AllIPs = Dns.GetHostAddresses(Dns.GetHostName());
            foreach (IPAddress ip in AllIPs)
            {
                if (ip.AddressFamily == AddressFamily.InterNetworkV6)
                {
                    supportsIpv6 = true;
                    //Console.WriteLine("Found IPV6: " + ip.ToString());
                }
            }
            return supportsIpv6;
        }

        public bool IsSupportIpv4()
        {
            if (supportsIpv4)
                return supportsIpv4;
            
            IPAddress[] AllIPs = Dns.GetHostAddresses(Dns.GetHostName());
            foreach (IPAddress ip in AllIPs)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    supportsIpv4 = true;
                    //Console.WriteLine("Found IPV4: " + ip.ToString());
                }
            }
            return supportsIpv4;
        }
        
        /**
         * Setup the connection credentials and socket configuration
         */
        public void SetupIPv4(string localIP, int localPort)
        {
            anyHost4 = new IPEndPoint(IPAddress.Any, localPort);
            try
            {
                socket4 = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                socket4.ExclusiveAddressUse = false;
                socket4.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                socket4.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer, NetworkConfig.BufferMaxLength * NetworkConfig.SocketBufferCount);
                socket4.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendBuffer, NetworkConfig.BufferMaxLength * NetworkConfig.SocketBufferCount);
                socket4.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, NetworkConfig.SocketReceiveTimeout);
                socket4.Bind(anyHost4);

                Listen(Reserve());

                sendSocket = socket4;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        /**
         * Setup the connection credentials and socket configuration
         */
        public void SetupIPv6(string localIP, int localPort)
        {
            anyHost6 = new IPEndPoint(IPAddress.IPv6Any, localPort);

            try
            {
                socket6 = new Socket(AddressFamily.InterNetworkV6, SocketType.Dgram, ProtocolType.Udp);
                socket6.ExclusiveAddressUse = false;
                socket6.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                socket6.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, true);
                socket6.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer, NetworkConfig.BufferMaxLength * NetworkConfig.SocketBufferCount);
                socket6.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendBuffer, NetworkConfig.BufferMaxLength * NetworkConfig.SocketBufferCount);
                socket6.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, NetworkConfig.SocketReceiveTimeout);
                socket6.Bind(anyHost6);

                sendSocket = socket6;
            }
            catch(Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public void Failed(NetworkErrorType errorType, string errorMsg, NetworkPacket packet)
        {
            packet.Error(errorType, errorMsg);

            if(OnError != null)
                OnError.Invoke(errorType, packet);

        }

        /**
         * Request a Listen on the RecvThread
         */
        public void Listen(NetworkPacket packet)
        {
            if (packet == null)
                packet = Reserve();

            packet.Reset();

            thread.BeginRecvThread(packet);
        }

        /**
         * Execute Listen
         * Enters from RecvThread, and begins listening for packets.
         */
        public void ExecuteListen(NetworkPacket packet)
        {
            packet.Reset();
            
            try
            {
                Socket socket = socket4;
                if(packet.networkIPType == NetworkIPType.IPv6)
                    socket = socket6;
               
                int bytesReceived = socket.ReceiveFrom(packet.ByteBuffer, ref packet.remoteEndPoint);
                packet.SetBufferLength(bytesReceived);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public void InvokeOnRecieve(NetworkPacket packet)
        {
            if (OnReceive != null) //notify any event listeners
                OnReceive.Invoke(this, packet);
        }

        
        /**
         * Begin Send
         * Starts the NetworkPacket for writing data to byte buffer.
         */
        public NetworkPacket Prepare(EndPoint endPoint)
        {
            NetworkPacket packet = Reserve();
            packet.remoteEndPoint = endPoint;
            packet.SetBufferLength(0);
            return packet;
        }
        
        /**
         * End Send
         * Finish writing the packet and push to send queue for SendThread
         */
        public void Send(NetworkPacket packet)
        {
            packet.Complete();

            lock (thread.SENDQUEUE)
            {
                thread.SENDQUEUE.Enqueue(packet);
            }
        }

        /**
         * Send Internal
         * Thread triggers send to remote point
         */
        public void SendFromThread(NetworkPacket packet)
        {
            try
            {
                if (packet.networkIPType == NetworkIPType.IPv4)
                    packet.byteSent = socket4.SendTo(packet.ByteBuffer, packet.byteLength, SocketFlags.None, packet.remoteEndPoint);
                else
                    packet.byteSent = socket6.SendTo(packet.ByteBuffer, packet.byteLength, SocketFlags.None, packet.remoteEndPoint);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            bool hasReliable = false;
            for(int i=0; i<packet.messages.Count; i++)
            {
                if (packet.messages[i].header.sendType == SendType.Message 
                    && packet.messages[i].header.isReliable)
                {
                    packet.messages[i].header.sentTime = NetworkTime.Milliseconds();
                    packet.messages[i].header.retryCount++;
                    
                    thread.RELIABLEQUEUE.Enqueue(packet);

                    hasReliable = true;
                }

            }
            
            if( !hasReliable )
            {
                Free(packet);
            }
            
            if (OnSend != null) //notify any event listeners
                OnSend.Invoke(this, packet);
        }

        /**
         * Reserve a socket event from the pool.  
         * Setup the socket, completed callback and UserToken
         */
        public NetworkPacket Reserve()
        {
            NetworkPacket packet = thread.PACKETPOOL.Reserve();
            packet.socket = this;
            packet.remoteEndPoint = anyHost4;
            return packet;
        }

        /**
         * Free a socket event back into the pool.
         * Reset all the initial properties to null.
         */
        public void Free(NetworkPacket packet)
        {
            thread.PACKETPOOL.Free(packet);
        }

        /**
         * Clean any open socket events and shutdown socket.
         */
        public void Dispose()
        {
            try
            {
                socket4.Shutdown(SocketShutdown.Both);
                socket4.Disconnect(false);
                socket4.Close();
                socket4.Dispose();

                socket6.Shutdown(SocketShutdown.Both);
                socket6.Disconnect(false);
                socket6.Close();
                socket6.Dispose();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
