
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

        public event EventHandler<NetworkStream> OnReceive;
        public event EventHandler<NetworkStream> OnSend;
        public event EventHandler<NetworkStream> OnError;
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

        public void Failed(NetworkErrorType errorType, NetworkStream stream)
        {
            if(OnError != null)
                OnError.Invoke(errorType, stream);

        }

        /**
         * Request a Listen on the RecvThread
         */
        public void Listen(NetworkStream stream)
        {
            if (stream == null)
                stream = Reserve();

            stream.Reset();

            thread.BeginRecvThread(stream);
           // ExecuteListen(stream);
            //lock (NetworkThread.RECVQUEUE)
            //{
            //    NetworkThread.RECVQUEUE.Add(stream);
            //}
        }

        /**
         * Execute Listen
         * Enters from RecvThread, and begins listening for packets.
         */
        public void ExecuteListen(NetworkStream stream)
        {
            //stream.Reset();
            
            try
            {
                
                Socket socket = socket4;
                if(stream.networkIPType == NetworkIPType.IPv6)
                    socket = socket6;
               
                //if (socket.Available == 0)
                //{
                    //Listen(stream);
                //    return;
                //}

                //NetworkConfig.ProfileBegin("RECV");
                int bytesReceived = socket.ReceiveFrom(stream.ByteBuffer, ref stream.remoteEndPoint);
                //socket.BeginReceiveFrom(stream.ByteBuffer, 0, stream.ByteBuffer.Length, SocketFlags.None, ref stream.remoteEndPoint, OnReceiveFromCallback, stream);
                stream.SetBufferLength(bytesReceived);

                Console.WriteLine("Received ("+bytesReceived+" bytes) from: " + stream.remoteEndPoint.ToString());

                //NetworkConfig.ProfileEnd("RECV");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            ///Listen(stream); //listen again
        }

        public void InvokeOnRecieve(NetworkStream stream)
        {
            if (OnReceive != null) //notify any event listeners
                OnReceive.Invoke(this, stream);
        }

        public void OnReceiveFromCallback(IAsyncResult res)
        {
            //NetworkConfig.ProfileBegin("ON_RECV");

            NetworkStream stream = (NetworkStream)res.AsyncState;

            Socket socket = socket4;
            if (stream.networkIPType == NetworkIPType.IPv6)
                socket = socket6;
            int bytesReceived = socket.EndReceive(res);
            stream.SetBufferLength(bytesReceived);
            
            if (OnReceive != null) //notify any event listeners
                OnReceive.Invoke(this, stream);
            //NetworkConfig.ProfileEnd("ON_RECV");

            Listen(stream);
        }
        
        /**
         * Begin Send
         * Starts the NetworkStream for writing data to byte buffer.
         */
        public NetworkStream Prepare(EndPoint endPoint)
        {
            NetworkStream stream = Reserve();
            stream.remoteEndPoint = endPoint;
            stream.SetBufferLength(0);
            return stream;
        }
        
        /**
         * End Send
         * Finish writing the stream and push to send queue for SendThread
         */
        public void Send(NetworkStream stream)
        {
            stream.Complete();

            lock (thread.SENDQUEUE)
            {
                //NetworkConfig.ProfileBegin("SENDQUEUE_INSERT");
                thread.SENDQUEUE.Enqueue(stream);
                //NetworkConfig.ProfileEnd("SENDQUEUE_INSERT");
            }
        }

        /**
         * Send Internal
         * Thread triggers send to remote point
         */
        public void SendInternal(NetworkStream stream)
        {
            try
            {
                //NetworkConfig.ProfileBegin("SEND");
                if (stream.networkIPType == NetworkIPType.IPv4)
                    stream.byteSent = socket4.SendTo(stream.ByteBuffer, stream.byteLength, SocketFlags.None, stream.remoteEndPoint);
                else
                    stream.byteSent = socket6.SendTo(stream.ByteBuffer, stream.byteLength, SocketFlags.None, stream.remoteEndPoint);
                //NetworkConfig.ProfileEnd("SEND");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            
            if (stream.header.sendType == SendType.Request && stream.header.isReliable)
            {
                //Console.WriteLine("Adding Reliable: " + stream.ackkey);
                stream.sentTime = NetworkTime.Milliseconds();
                stream.retryCount++;

               // lock (thread.RELIABLEQUEUE)
                {
                    

                    //NetworkConfig.ProfileBegin("RELIABLE_INSERT");
                    thread.RELIABLEQUEUE.Enqueue(stream);
                    //NetworkConfig.ProfileEnd("RELIABLE_INSERT");
                }
            }
            else
            {
                Free(stream);
            }

            if (OnSend != null) //notify any event listeners
                OnSend.Invoke(this, stream);
        }

        /**
         * Reserve a socket event from the pool.  
         * Setup the socket, completed callback and UserToken
         */
        public NetworkStream Reserve()
        {
            //NetworkConfig.ProfileBegin("POOL_RESERVE");
            NetworkStream stream = thread.STREAMPOOL.Reserve();
            //NetworkConfig.ProfileEnd("POOL_RESERVE");
            stream.socket = this;
            stream.remoteEndPoint = anyHost4;
            return stream;
        }

        /**
         * Free a socket event back into the pool.
         * Reset all the initial properties to null.
         */
        public void Free(NetworkStream stream)
        {
            //NetworkConfig.ProfileBegin("POOL_FREE");
            thread.STREAMPOOL.Free(stream);
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
