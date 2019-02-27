using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace OpenP2P
{
    public class NetworkSocket
    {
        public Socket socket;
        public IPEndPoint remote;
        public IPEndPoint local;

        public static NetworkSocketEventPool pool = new NetworkSocketEventPool(10, 1500);
        public Dictionary<int, NetworkSocketEvent> activeEvents;

        public NetworkSocket(string remoteHost, int remotePort, int localPort)
        {
            remote = new IPEndPoint(IPAddress.Parse(remoteHost), remotePort);
            local = new IPEndPoint(IPAddress.Any, localPort);

            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            socket.ExclusiveAddressUse = false;
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            socket.Bind(local);
        }

        public void Listen()
        {
            NetworkSocketEvent socketEvent = Reserve();

            if (!socket.ReceiveFromAsync(socketEvent.args))
            {
                ProcessEvent(socketEvent);
            }
        }

        public void Send(byte[] data)
        {
            NetworkSocketEvent socketEvent = Reserve();
            socketEvent.SetBuffer(data);

            if (!socket.SendToAsync(socketEvent.args))
            {
                ProcessEvent(socketEvent);
            }
        }

        void OnReceive(NetworkSocketEvent socketEvent)
        {
            if (socketEvent.args.BytesTransferred > 0
                && socketEvent.args.SocketError == SocketError.Success)
            {
                byte[] data = new byte[socketEvent.args.BytesTransferred];
                for (int i = 0; i < socketEvent.args.BytesTransferred; i++)
                {
                    data[i] = socketEvent.args.Buffer[i];
                }
                Console.WriteLine("Received message: " + System.Text.Encoding.ASCII.GetString(data));
            }
            
        }

        void OnSend(NetworkSocketEvent socketEvent)
        {

        }

        public NetworkSocketEvent Reserve()
        {
            NetworkSocketEvent socketEvent = pool.Reserve();
            socketEvent.args.AcceptSocket = socket;
            socketEvent.args.Completed += new EventHandler<SocketAsyncEventArgs>(OnSocketCompleted);
            socketEvent.args.UserToken = socketEvent;

            activeEvents.Add(socketEvent.id, socketEvent);

            return socketEvent;
        }

        public void Free(NetworkSocketEvent socketEvent)
        {
            activeEvents.Remove(socketEvent.id);
            pool.Free(socketEvent);
        }

        void OnSocketCompleted(object sender, SocketAsyncEventArgs e)
        {
            ProcessEvent((NetworkSocketEvent)e.UserToken);
        }

        void ProcessEvent(NetworkSocketEvent socketEvent)
        {
            // determine which type of operation just completed and call the associated handler
            switch (socketEvent.args.LastOperation)
            {
                case SocketAsyncOperation.Receive:
                    OnReceive(socketEvent);
                    Listen(); //listen again
                    break;
                case SocketAsyncOperation.Send:
                    OnSend(socketEvent);
                    break;
                default:
                    throw new ArgumentException("The last operation completed on the socket was not a receive or send");
            }

            Free(socketEvent);
        }

        

        public void Dispose()
        {
            try
            {
                foreach (KeyValuePair<int, NetworkSocketEvent> entry in activeEvents)
                {
                    Free(entry.Value);
                }

                socket.Shutdown(SocketShutdown.Both);
                socket.Disconnect(false);
                socket.Close();
                socket.Dispose();
            }
            catch (Exception e)
            {

            }
        
        }
    }
}
