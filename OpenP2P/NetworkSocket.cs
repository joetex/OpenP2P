
using System;
using System.Net;
using System.Net.Sockets;

namespace OpenP2P
{
    public class NetworkSocket 
    {
        public Socket socket4;
        //public IPEndPoint remote4;
        //public IPEndPoint local4;
        public IPEndPoint anyHost4;

        public Socket socket6;
        //public IPEndPoint remote6;
        //public IPEndPoint local6;
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

        //public NetworkThread threads = null;

        public event EventHandler<NetworkPacket> OnReceive;
        public event EventHandler<NetworkPacket> OnSend;
        public event EventHandler<NetworkPacket> OnError;
        /*
        public NetworkSocket(string remoteHost, int remotePort, int localPort)
        {
            //Setup(remoteHost, remotePort, localPort);
            
        }
        public NetworkSocket(string remoteHost, int remotePort)
        {
            //Setup(remoteHost, remotePort, 0);
        }*/
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

        public static bool supportsIpv6 = false;
        public static bool supportsIpv4 = false;

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
                    Console.WriteLine("Found IPV6: " + ip.ToString());
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
                    Console.WriteLine("Found IPV4: " + ip.ToString());
                }
            }
            return supportsIpv4;
        }

        

        //public void AttachThreads(NetworkThread t)
        //{
        //threads = t;
        //}

        /**
         * Setup the connection credentials and socket configuration
         */
        public void SetupIPv4(string localIP, int localPort)
        {
            //remote4 = new IPEndPoint(IPAddress.Parse(remoteHost), remotePort);
            //local4 = new IPEndPoint(IPAddress.Parse(remoteHost), localPort);
            anyHost4 = new IPEndPoint(IPAddress.Any, localPort);
            try
            {
                socket4 = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                //socket.DualMode = true;
                socket4.ExclusiveAddressUse = false;

                socket4.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                socket4.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer, NetworkConfig.BufferMaxLength * NetworkConfig.SocketBufferCount);
                socket4.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendBuffer, NetworkConfig.BufferMaxLength * NetworkConfig.SocketBufferCount);
                socket4.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, NetworkConfig.SocketReceiveTimeout);
                //socket4.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontRoute, true);
                //socket4.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.NoChecksum, true);
                //if (localPort != 0)
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
            //remote6 = new IPEndPoint(IPAddress.Parse(remoteHost), remotePort);
            //local6 = new IPEndPoint(IPAddress.Parse(remoteHost), localPort);
            ///if( local6.AddressFamily == AddressFamily.InterNetwork )
            //    local6 = new IPEndPoint(local6.Address.MapToIPv6(), localPort);

            anyHost6 = new IPEndPoint(IPAddress.IPv6Any, localPort);

            try
            {
                socket6 = new Socket(AddressFamily.InterNetworkV6, SocketType.Dgram, ProtocolType.Udp);
                //socket.DualMode = true;
                
                socket6.ExclusiveAddressUse = false;
                socket6.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                socket6.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, true);
                //socket6.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontRoute, NetworkConfig.SocketReceiveTimeout);
                socket6.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer, NetworkConfig.BufferMaxLength * NetworkConfig.SocketBufferCount);
                socket6.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendBuffer, NetworkConfig.BufferMaxLength * NetworkConfig.SocketBufferCount);
                socket6.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, NetworkConfig.SocketReceiveTimeout);

                //if (localPort != 0)
                socket6.Bind(anyHost6);
                
                //Listen(Reserve());

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
           // ExecuteListen(packet);
            //lock (NetworkThread.RECVQUEUE)
            //{
            //    NetworkThread.RECVQUEUE.Add(packet);
            //}
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
               
                //if (socket.Available == 0)
                //{
                    //Listen(packet);
                //    return;
                //}

                //NetworkConfig.ProfileBegin("RECV");
                int bytesReceived = socket.ReceiveFrom(packet.ByteBuffer, ref packet.remoteEndPoint);
                //socket.BeginReceiveFrom(packet.ByteBuffer, 0, packet.ByteBuffer.Length, SocketFlags.None, ref packet.remoteEndPoint, OnReceiveFromCallback, packet);
                packet.SetBufferLength(bytesReceived);

                //Console.WriteLine("Received ("+bytesReceived+" bytes) from: " + packet.remoteEndPoint.ToString());

                //NetworkConfig.ProfileEnd("RECV");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            ///Listen(packet); //listen again
        }

        public void InvokeOnRecieve(NetworkPacket packet)
        {
            if (OnReceive != null) //notify any event listeners
                OnReceive.Invoke(this, packet);
        }

        public void OnReceiveFromCallback(IAsyncResult res)
        {
            //NetworkConfig.ProfileBegin("ON_RECV");

            NetworkPacket packet = (NetworkPacket)res.AsyncState;

            Socket socket = socket4;
            if (packet.networkIPType == NetworkIPType.IPv6)
                socket = socket6;
            int bytesReceived = socket.EndReceive(res);
            packet.SetBufferLength(bytesReceived);
            
            if (OnReceive != null) //notify any event listeners
                OnReceive.Invoke(this, packet);
            //NetworkConfig.ProfileEnd("ON_RECV");

            Listen(packet);
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
                //NetworkConfig.ProfileBegin("SENDQUEUE_INSERT");
                thread.SENDQUEUE.Enqueue(packet);
                //NetworkConfig.ProfileEnd("SENDQUEUE_INSERT");
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
                //NetworkConfig.ProfileBegin("SEND");
                if (packet.networkIPType == NetworkIPType.IPv4)
                    packet.byteSent = socket4.SendTo(packet.ByteBuffer, packet.byteLength, SocketFlags.None, packet.remoteEndPoint);
                else
                    packet.byteSent = socket6.SendTo(packet.ByteBuffer, packet.byteLength, SocketFlags.None, packet.remoteEndPoint);
                //NetworkConfig.ProfileEnd("SEND");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            
            if (packet.message.header.sendType == SendType.Message && packet.message.header.isReliable)
            {
                //Console.WriteLine("Adding Reliable: " + packet.ackkey);
                packet.sentTime = NetworkTime.Milliseconds();
                packet.retryCount++;

               // lock (thread.RELIABLEQUEUE)
                {
                    

                    //NetworkConfig.ProfileBegin("RELIABLE_INSERT");
                    thread.RELIABLEQUEUE.Enqueue(packet);
                    //NetworkConfig.ProfileEnd("RELIABLE_INSERT");
                }
            }
            else
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
            //NetworkConfig.ProfileBegin("POOL_RESERVE");
            NetworkPacket packet = thread.PACKETPOOL.Reserve();
            //NetworkConfig.ProfileEnd("POOL_RESERVE");
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
            //NetworkConfig.ProfileBegin("POOL_FREE");
            thread.PACKETPOOL.Free(packet);
            //NetworkConfig.ProfileEnd("POOL_FREE");
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
