
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

        public event EventHandler<NetworkPacket> OnReceive;
        public event EventHandler<NetworkPacket> OnSend;

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
        public void Listen(NetworkPacket packet)
        {
            if (packet == null)
                packet = Reserve();

            packet.Reset();

            socket.BeginReceiveFrom(packet.ByteBuffer, 0, packet.ByteBuffer.Length, SocketFlags.None, ref packet.remoteEndPoint, ExecuteListen, packet);

            //ExecuteListen(packet);
        }

        /**
         * Execute Listen
         * Enters from RecvThread, and begins listening for packets.
         */
        public void ExecuteListen(IAsyncResult iar)
        {
            NetworkPacket packet = (NetworkPacket)iar.AsyncState;

            packet.Reset();

            try
            {
                int bytesReceived = socket.EndReceiveFrom(iar, ref packet.remoteEndPoint);

                //int bytesReceived = socket.ReceiveFrom(packet.ByteBuffer, ref packet.remoteEndPoint);
                packet.SetBufferLength(bytesReceived);

                if (OnReceive != null) //notify any event listeners
                    OnReceive.Invoke(this, packet);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            Listen(packet); //listen again
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

            socket.BeginSendTo(packet.ByteBuffer, 0, packet.byteLength, SocketFlags.None, packet.remoteEndPoint, SendInternal, packet);
            //lock (threads.SENDQUEUE)
            //{
            //   threads.SENDQUEUE.Enqueue(packet);
            //}
        }

        /**
         * Send Internal
         * Thread triggers send to remote point
         */
        public void SendInternal(IAsyncResult iar)
        {
            NetworkPacket packet = (NetworkPacket)iar.AsyncState;
            try
            {
                packet.byteSent = socket.EndSendTo(iar);
                //packet.byteSent = socket.SendTo(packet.ByteBuffer, packet.byteLength, SocketFlags.None, packet.remoteEndPoint);
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

                /*
                lock (NetworkThread.RELIABLEQUEUE)
                {
                    

                    NetworkThread.RELIABLEQUEUE.Enqueue(packet);
                }*/
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
            NetworkPacket packet = thread.PACKETPOOL.Reserve();
            //packet.socket = this;
            
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
