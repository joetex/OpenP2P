using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace OpenP2P
{
    /// <summary>
    /// This enum specifies STUN message type.
    /// </summary>
    public enum STUNMethod
    {
        None = 0x0,
        BindingRequest = 0x0001,
        BindingResponse = 0x0101,
        BindingErrorResponse = 0x0111,
        SharedSecretRequest = 0x0002,
        SharedSecretResponse = 0x0102,
        SharedSecretErrorResponse = 0x0112
    }

    public enum STUNAttribute
    {
        None = 0x0,
        MappedAddress = 0x0001,
        ResponseAddress = 0x0002,
        ChangeRequest = 0x0003,
        SourceAddress = 0x0004,
        ChangedAddress = 0x0005,
        Username = 0x0006,
        Password = 0x0007,
        MessageIntegrity = 0x0008,
        ErrorCode = 0x0009,
        UnknownAttribute = 0x000A,
        ReflectedFrom = 0x000B,
        XorMappedAddress = 0x8020,
        XorOnly = 0x0021,
        ServerName = 0x8022
    }

    public enum STUNNat
    {
        /// <summary>
        /// Unspecified NAT Type
        /// </summary>
        Unspecified,

        /// <summary>
        /// Open internet. for example Virtual Private Servers.
        /// </summary>
        OpenInternet,

        /// <summary>
        /// Full Cone NAT. Good to go.
        /// </summary>
        FullCone,

        /// <summary>
        /// Restricted Cone NAT.
        /// It mean's client can only receive data only IP addresses that it sent a data before.
        /// </summary>
        Restricted,

        /// <summary>
        /// Port-Restricted Cone NAT.
        /// Same as <see cref="Restricted"/> but port is included too.
        /// </summary>
        PortRestricted,

        /// <summary>
        /// Symmetric NAT.
        /// It's means the client pick's a different port for every connection it made.
        /// </summary>
        Symmetric,

        /// <summary>
        /// Same as <see cref="OpenInternet"/> but only received data from addresses that it sent a data before.
        /// </summary>
        SymmetricUDPFirewall,
    }

    public class STUNAddress
    {
        public byte family = 0;
        public ushort port = 0;
        public byte[] address = null;

        public override string ToString()
        {
            //ipv4
            if (family == 1)
                return address[0] + "." + address[1] + "." + address[2] + "." + address[3] + ":" + port;
           
            //ipv6
            string ipv6 = "";
            for(int i=0; i<address.Length; i+=2 )
            {
                ushort part = BitConverter.ToUInt16(address, i);
                ipv6 += part + ":";
            }
            ipv6 += port;
            return ipv6;
        }
    }

    public class STUNClient
    {
        public IPEndPoint stunHost = null;
        public string stunDefaultAddress = "stun.l.google.com:19302";
        public string stunAddress = "";
        public int stunPort = 0;
        public const int stunDefaultPort = 3478;

        NetworkProtocol protocol = null;

        public byte[] transactionID = null;

        string mappedAddress = "";
        string changedAddress = "";
        string sourceAddress = "";
        string localAddress = "";
        string originalMappedAddress = "";

        public int testId = 0;

        public STUNNat nat = STUNNat.Unspecified;

        public STUNClient(NetworkProtocol p)
        {
            protocol = p;
            protocol.AttachResponseListener(ChannelType.STUN, OnResponseSTUN);
            protocol.AttachErrorListener(NetworkErrorType.ErrorNoResponseSTUN, OnErrorSTUN);

            transactionID = GenerateTransactionID();
        }
        

        public void ConnectToSTUN()
        {
            localAddress = protocol.socket.socket4.LocalEndPoint.ToString();
            test1a(stunDefaultAddress);
        }
        
        public void ConnectToSTUN(string address, bool changeIP, bool changePort)
        {
            GenerateHostAddressAndPort(address);

            MessageSTUN message = protocol.Create<MessageSTUN>();
            message.method = STUNMethod.BindingRequest;
            message.transactionID = transactionID;
            message.WriteChangeRequest(changeIP, changePort);

            Console.WriteLine("Sending STUN Request to: " + stunHost.ToString());
            protocol.SendSTUN(stunHost, message, NetworkConfig.SocketReliableRetryDelay*3);
        }

        public void test1a(string address)
        {
            testId = 0;
            ConnectToSTUN(address, false, false);
        }
        public void test1b(string address)
        {
            testId = 1;
            ConnectToSTUN(address, false, false);
        }
        public void test2a(string address)
        {
            testId = 2;
            ConnectToSTUN(address, true, true);
        }
        public void test2b(string address)
        {
            testId = 3;
            ConnectToSTUN(address, true, true);
        }
        public void test3(string address)
        {
            testId = 4;
            ConnectToSTUN(address, false, true);
        }

        
        public void OnErrorSTUN(object sender, NetworkPacket packet)
        {
            CheckTests(false);
        }
        

        public void OnResponseSTUN(object sender, NetworkMessage msg)
        {
            NetworkPacket packet = (NetworkPacket)sender;
            MessageSTUN message = (MessageSTUN)msg;

            if (message.method != STUNMethod.BindingResponse)
                return;
            
            changedAddress = message.GetString(STUNAttribute.ChangedAddress);
            if( changedAddress.Length == 0 )
            {
                changedAddress = "stun4.l.google.com:19302";
            }
            mappedAddress = message.GetString(STUNAttribute.MappedAddress);
            sourceAddress = message.GetString(STUNAttribute.SourceAddress);

            

            Console.WriteLine("STUN Test #" + testId);
            //Console.WriteLine("STUN Host: " + packet.remoteEndPoint.ToString());
            //Console.WriteLine("STUN Response Method: " + Enum.GetName(typeof(STUNMethod), message.method));
            //Console.WriteLine("STUN Response Length: " + methodLength);

            Console.WriteLine("STUN Attributes: " + GetAttributeKeys(message));
            //Console.WriteLine("MappedAddress: " + mappedAddress);
            //Console.WriteLine("XorMappedAddress: " + message.Get(STUNAttribute.XorMappedAddress).ToString());
            //Console.WriteLine("SourceAddress: " + sourceAddress);
            //Console.WriteLine("ChangedAddress: " + changedAddress);

            CheckTests(true);
        }


        //
        //STUN Test Flow (RFC 3489 and RFC 5389):
        //https://github.com/ccding/go-stun/blob/master/stun/discover.go#L25
        //
        public void CheckTests(bool hadResponse)
        {
            
            switch (testId)
            {
                case 0: //test1a
                    
                    if( !hadResponse )
                    {
                        Console.WriteLine("UDP Blocked");
                        return;
                    }

                    originalMappedAddress = mappedAddress;

                    if ( localAddress.Equals(mappedAddress) )
                        test2a(stunDefaultAddress);
                    else
                        test2b(stunDefaultAddress);
                   
                    break;
                case 1: //test1b

                    if( !originalMappedAddress.Equals(mappedAddress) )
                    {
                        Console.WriteLine("NAT Type: Symmetric Nat");
                        nat = STUNNat.Symmetric;
                        return;
                    }

                    test3(changedAddress);
                    break;
                case 2: //test2a
                    
                    if( !hadResponse )
                    {
                        Console.WriteLine("NAT Type: Symmetric UDP Firewall");
                        nat = STUNNat.SymmetricUDPFirewall;
                        return;
                    }

                    Console.WriteLine("Open Internet");
                    nat = STUNNat.OpenInternet;
                    break;
                case 3: //test2b

                    if( !hadResponse )
                    {
                        test1b(changedAddress);
                        return;
                    }

                    Console.WriteLine("Nat Type: Full Cone");
                    nat = STUNNat.FullCone;
                    break;
                case 4: //test3

                    if(!hadResponse)
                    {
                        Console.WriteLine("NAT Type: Port Restricted");
                        nat = STUNNat.PortRestricted;
                        return;
                    }

                    Console.WriteLine("NAT Type: Restricted");
                    nat = STUNNat.Restricted;
                    break;
            }
        }


        public void Reset()
        {
            testId = 0;
        }

        public string GetAttributeKeys(MessageSTUN message)
        {
            string attrKeys = "";
            foreach (KeyValuePair<STUNAttribute, object> entry in message.response)
            {
                string key = Enum.GetName(typeof(STUNAttribute), entry.Key);
                if (attrKeys.Length > 0)
                    attrKeys += ", ";
                attrKeys += key;
            }
            return attrKeys;
        }

        public static byte[] GenerateTransactionID()
        {
            Guid guid = Guid.NewGuid();
            return guid.ToByteArray();
        }

        public void GenerateHostAddressAndPort(string address)
        {
            int stunAddressColonPos = address.IndexOf(':');
            if (stunAddressColonPos > -1)
            {
                stunPort = int.Parse(address.Substring(stunAddressColonPos + 1));
                stunAddress = address.Substring(0, stunAddressColonPos);
            }
            else
            {
                stunAddress = address;
                stunPort = stunDefaultPort;
            }

            stunHost = protocol.GetEndPoint(stunAddress, stunPort);
        }
    }
}
