
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OpenP2P
{
    public class NetworkSocket2
    {
        public Socket socket;
        public IPEndPoint remote;
        public IPEndPoint local;
        public IPEndPoint anyHost;
        public NetworkThread thread = null;

        //public NetworkThread threads = null;

        public event EventHandler<NetworkStream> OnReceive;
        public event EventHandler<NetworkStream> OnSend;

        public NetworkSocket2(string remoteHost, int remotePort, int localPort)
        {
            Setup(remoteHost, remotePort, localPort);
        }
        public NetworkSocket2(string remoteHost, int remotePort)
        {
            Setup(remoteHost, remotePort, 0);
        }
        public NetworkSocket2(int localPort)
        {
            Setup("::FFFF:127.0.0.1", 0, localPort);
        }

        //public void AttachThreads(NetworkThread t)
        //{
            //threads = t;
        //}

        /**
         * Setup the connection credentials and socket configuration
         */
        public void Setup(string remoteHost, int remotePort, int localPort)
        {

            remote = new IPEndPoint(IPAddress.Parse(remoteHost), remotePort);
            local = new IPEndPoint(IPAddress.Parse(remoteHost), localPort);
            anyHost = new IPEndPoint(IPAddress.Any, 0);

            socket = new Socket(AddressFamily.InterNetworkV6, SocketType.Dgram, ProtocolType.Udp);
            socket.DualMode = true;
            socket.ExclusiveAddressUse = false;
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            socket.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, false);
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer, NetworkConfig.BufferMaxLength * NetworkConfig.SocketBufferCount);
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendBuffer, NetworkConfig.BufferMaxLength * NetworkConfig.SocketBufferCount);
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, NetworkConfig.SocketReceiveTimeout);
            //if (localPort != 0)
            socket.Bind(local);

            thread = new NetworkThread();
            thread.StartNetworkThreads();
        }

        /**
         * Request a Listen on the RecvThread
         */
        public void Listen(NetworkStream stream)
        {
            if (stream == null)
                stream = Reserve();

            stream.Reset();

            socket.BeginReceiveFrom(stream.ByteBuffer, 0, stream.ByteBuffer.Length, SocketFlags.None, ref stream.remoteEndPoint, ExecuteListen, stream);

            //ExecuteListen(stream);
        }

        /**
         * Execute Listen
         * Enters from RecvThread, and begins listening for packets.
         */
        public void ExecuteListen(IAsyncResult iar)
        {
            NetworkStream stream = (NetworkStream)iar.AsyncState;

            stream.Reset();

            try
            {
                int bytesReceived = socket.EndReceiveFrom(iar, ref stream.remoteEndPoint);

                //int bytesReceived = socket.ReceiveFrom(stream.ByteBuffer, ref stream.remoteEndPoint);
                stream.SetBufferLength(bytesReceived);

                if (OnReceive != null) //notify any event listeners
                    OnReceive.Invoke(this, stream);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            Listen(stream); //listen again
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

            socket.BeginSendTo(stream.ByteBuffer, 0, stream.byteLength, SocketFlags.None, stream.remoteEndPoint, SendInternal, stream);
            //lock (threads.SENDQUEUE)
            //{
            //   threads.SENDQUEUE.Enqueue(stream);
            //}
        }

        /**
         * Send Internal
         * Thread triggers send to remote point
         */
        public void SendInternal(IAsyncResult iar)
        {
            NetworkStream stream = (NetworkStream)iar.AsyncState;
            try
            {
                stream.byteSent = socket.EndSendTo(iar);
                //stream.byteSent = socket.SendTo(stream.ByteBuffer, stream.byteLength, SocketFlags.None, stream.remoteEndPoint);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            if (stream.header.sendType == SendType.Message && stream.header.isReliable)
            {
                //Console.WriteLine("Adding Reliable: " + stream.ackkey);
                stream.sentTime = NetworkTime.Milliseconds();
                stream.retryCount++;

                /*
                lock (NetworkThread.RELIABLEQUEUE)
                {
                    

                    NetworkThread.RELIABLEQUEUE.Enqueue(stream);
                }*/
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
            NetworkStream stream = thread.STREAMPOOL.Reserve();
            //stream.socket = this;
            
            return stream;
        }

        /**
         * Free a socket event back into the pool.
         * Reset all the initial properties to null.
         */
        public void Free(NetworkStream stream)
        {
            thread.STREAMPOOL.Free(stream);
        }

        /**
         * Clean any open socket events and shutdown socket.
         */
        public void Dispose()
        {
            try
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Disconnect(false);
                socket.Close();
                socket.Dispose();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
