using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace OpenP2P
{
    /// <summary>
    /// This enum specifies STUN message type.
    /// </summary>
    public enum STUNMethod
    {
        None                        = 0x0,

        //STUN
        BindingRequest              = 0x0001,
        BindingResponse             = 0x0101,
        BindingErrorResponse        = 0x0111,
        SharedSecretRequest         = 0x0002,
        SharedSecretResponse        = 0x0102,
        SharedSecretErrorResponse   = 0x0112,

        //TURN
        AllocateRequest             = 0x0003,
        AllocateResponse            = 0x0103,
        AllocateError               = 0x0113,
        RefreshRequest              = 0x0004,
        RefreshResponse             = 0x0104,
        RefereshError               = 0x0114,
        SendRequest                 = 0x0006,
        SendResponse                = 0x0106,
        SendError                   = 0x0116,
        DataRequest                 = 0x0007,
        DataResponse                = 0x0107,
        DataError                   = 0x0117,
        CreatePermissionRequest     = 0x0008,
        CreatePermissionResponse    = 0x0108,
        CreatePermissionError       = 0x0118,
        ChannelBindRequest          = 0x0009,
        ChannelBindResponse         = 0x0109,
        ChannelBindError            = 0x0119
    }

    public enum STUNAttribute
    {
        None = 0x0,

        //STUN standard attributes
        MappedAddress       = 0x0001,
        ResponseAddress     = 0x0002,
        ChangeRequest       = 0x0003,
        SourceAddress       = 0x0004,
        ChangedAddress      = 0x0005,
        Username            = 0x0006,
        Password            = 0x0007,
        MessageIntegrity    = 0x0008,
        ErrorCode           = 0x0009,
        UnknownAttribute    = 0x000A,
        ReflectedFrom       = 0x000B,
        XorMappedAddress    = 0x0020,
        XorOnly             = 0x0021,
        ServerName          = 0x8022,
        OtherAddress        = 0x802C,

        //TURN extras
        ChannelNumber       = 0x000C,
        Lifetime            = 0x000D,
        AlternateServer     = 0x000E,
        Bandwidth           = 0x0010,
        DestinationAddress  = 0x0011,
        XorPeerAddress      = 0x0012,
        Data                = 0x0013,
        Realm               = 0x0014,
        Nonce               = 0x0015,
        XorRelayedAddress   = 0x0016,
        EvenPort            = 0x0018,
        RequestedTransport  = 0x0019,
        DontFragment        = 0x001A,
        TimerVal            = 0x0021,
        ReservationToken    = 0x0022
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
            for (int i = 0; i < address.Length; i += 2)
                ipv6 += BitConverter.ToUInt16(address, i) + ":";
            ipv6 += port;
            return ipv6;
        }
    }

    /// <summary>
    /// STUN Client based on RFC5389 and RFC3489
    /// https://tools.ietf.org/html/rfc5389
    /// https://tools.ietf.org/html/rfc3489
    /// </summary>
    public class STUNClient
    {
        public IPEndPoint stunHost = null;
        public string[] stunAddresses = new string[] { "stun2.l.google.com:19302", "stun3.l.google.com:19302" };
        public string stunDefaultAddress = "stun.l.google.com:19302";
        public string stunAddress = "";
        public int stunPort = 0;
        public const int stunDefaultPort = 3478;

        public IPEndPoint turnHost = null;
        public string turnDefaultAddress = "34.70.87.145";
        public string turnAddress = "";
        public int turnPort = 0;
        public const int turnDefaultPort = 3478;

        private NetworkProtocol protocol = null;

        public byte[] transactionID = null;
        public uint magicCookie = 0x2112A442;
        public byte[] nonce = null;

        private string mappedAddress = "";
        private string changedAddress = "";
        private string sourceAddress = "";
        private string localAddress = "";
        private string originalMappedAddress = "";

        public int testId = -1;

        public STUNNat nat = STUNNat.Unspecified;

        public STUNClient(NetworkProtocol p)
        {
            protocol = p;
            protocol.AttachResponseListener(ChannelType.STUN, OnResponse);
            protocol.AttachErrorListener(NetworkErrorType.ErrorNoResponseSTUN, OnError);

            transactionID = GenerateTransactionID();
        }

        public void ConnectSTUN(bool isTest)
        {
            localAddress = protocol.socket.socket4.LocalEndPoint.ToString();
            if (isTest)
                test1a(stunDefaultAddress);
            else
            {
                testId = -1;
                ConnectSTUN(stunDefaultAddress, false, false);
            }
        }

        public void ConnectSTUN(string address, bool changeIP, bool changePort)
        {
            stunHost = protocol.GenerateHostAddressAndPort(address, stunDefaultPort);

            MessageSTUN message = protocol.Create<MessageSTUN>();
            message.method = STUNMethod.BindingRequest;
            message.transactionID = transactionID;
            message.WriteChangeRequest(changeIP, changePort);
            //message.WriteString(STUNAttribute.Username, "joelruiz2@gmail.com");
            //message.WriteString(STUNAttribute.Password, "Housedoor10??");

            Console.WriteLine("Sending STUN Request to: " + stunHost.ToString());
            protocol.SendSTUN(stunHost, message, NetworkConfig.SocketReliableRetryDelay);
        }

        public void ConnectTURN(string address, bool isFirst)
        {
            if(isFirst)
            {
                turnAllocateCount = 0;
            } else
            {
                turnAllocateCount++;
                if (turnAllocateCount > 5)
                    return;
            }
            if (address == null || address.Length == 0)
                address = turnDefaultAddress;

            turnHost = protocol.GenerateHostAddressAndPort(address, turnDefaultPort);

            MessageSTUN message = protocol.Create<MessageSTUN>();
            message.method = STUNMethod.AllocateRequest;
            message.transactionID = transactionID;
           
            //message.WriteString(STUNAttribute.ServerName, "OpenP2P");
            message.WriteUInt(STUNAttribute.Lifetime, 300);
            message.WriteUInt(STUNAttribute.RequestedTransport, (uint)(17 << 24));
            message.WriteEmpty(STUNAttribute.DontFragment);
            
            message.WriteString(STUNAttribute.Username, message.username);
            message.WriteString(STUNAttribute.Realm, message.realm);
            

            if (nonce != null)
                message.WriteBytes(STUNAttribute.Nonce, nonce);

            message.WriteMessageIntegrity();
            

            Console.WriteLine("TURN Method: " + Enum.GetName(typeof(STUNMethod), message.method) + " (" + ((int)message.method).ToString("X") + ")");
            Console.WriteLine("TURN Request sent to: " + turnHost.ToString());

            protocol.SendSTUN(turnHost, message, NetworkConfig.SocketReliableRetryDelay);

            
        }

        

        public void OnError(object sender, NetworkPacket packet)
        {
            MessageSTUN message = (MessageSTUN)packet.messages[0];

            if (message.method == STUNMethod.BindingRequest && testId > -1)
                CheckTests(false);
        }

        public int turnAllocateCount = 0;

        public void OnResponse(object sender, NetworkMessage msg)
        {
            NetworkPacket packet = (NetworkPacket)sender;
            MessageSTUN message = (MessageSTUN)msg;

            Console.WriteLine("STUN Host: " + packet.remoteEndPoint.ToString());
            Console.WriteLine("STUN Response Method: " + Enum.GetName(typeof(STUNMethod), message.method));
            //Console.WriteLine("STUN Response Length: " + methodLength);

            Console.WriteLine("STUN Attributes: \n" + GetAttributeKeys(message));
            //Console.WriteLine("MappedAddress: " + mappedAddress);
            //Console.WriteLine("XorMappedAddress: " + message.Get(STUNAttribute.XorMappedAddress).ToString());
            //Console.WriteLine("SourceAddress: " + sourceAddress);
            //Console.WriteLine("ChangedAddress: " + changedAddress);
            if (message.method == STUNMethod.BindingResponse)
            {
                if (testId > -1)
                {
                    Console.WriteLine("STUN Test #" + testId);

                    changedAddress = message.GetString(STUNAttribute.ChangedAddress);
                    mappedAddress = message.GetString(STUNAttribute.MappedAddress);
                    sourceAddress = message.GetString(STUNAttribute.SourceAddress);

                    CheckTests(true);
                }
                else
                {
                   
                }
            }

            if( message.method == STUNMethod.AllocateError )
            {
                nonce = (byte[])message.Get(STUNAttribute.Nonce);
                ConnectTURN(null, false);
            }
        }

        public void test1a(string address)
        {
            testId = 0;
            ConnectSTUN(address, false, false);
        }

        public void test1b(string address)
        {
            testId = 1;
            ConnectSTUN(address, false, false);
        }

        public void test2a(string address)
        {
            testId = 2;
            ConnectSTUN(address, true, true);
        }

        public void test2b(string address)
        {
            testId = 3;
            ConnectSTUN(address, true, true);
        }

        public void test3(string address)
        {
            testId = 4;
            ConnectSTUN(address, false, true);
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

                    if (!hadResponse)
                    {
                        Console.WriteLine("UDP Blocked");
                        return;
                    }

                    originalMappedAddress = mappedAddress;

                    if (localAddress.Equals(mappedAddress))
                        test2a(stunDefaultAddress);
                    else
                        test2b(stunDefaultAddress);

                    break;

                case 1: //test1b

                    if (!originalMappedAddress.Equals(mappedAddress))
                    {
                        Console.WriteLine("NAT Type: Symmetric Nat");
                        nat = STUNNat.Symmetric;
                        return;
                    }

                    test3(changedAddress);
                    break;

                case 2: //test2a

                    if (!hadResponse)
                    {
                        Console.WriteLine("NAT Type: Symmetric UDP Firewall");
                        nat = STUNNat.SymmetricUDPFirewall;
                        return;
                    }

                    Console.WriteLine("Open Internet");
                    nat = STUNNat.OpenInternet;
                    break;

                case 3: //test2b

                    if (!hadResponse)
                    {
                        test1b(changedAddress);
                        return;
                    }

                    Console.WriteLine("Nat Type: Full Cone");
                    nat = STUNNat.FullCone;
                    break;

                case 4: //test3

                    if (!hadResponse)
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
                int id = (int)entry.Key;
                if (attrKeys.Length > 0)
                    attrKeys += "\n";
                object value = entry.Value;
                string valueStr = "";
                if (value is string v)
                    valueStr = v;
                else if (value is byte[] b)
                    valueStr = NetworkSerializer.ByteArrayToHexString(b);
                else
                    valueStr = value.ToString();
                attrKeys += key + "(" + id.ToString("X") + ") = " + valueStr;
            }
            return attrKeys;
        }

        public static byte[] GenerateTransactionID()
        {
            Guid guid = Guid.NewGuid();
            byte[] bytes = guid.ToByteArray();
            byte[] magicCookie = BitConverter.GetBytes(0x2112A442);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(magicCookie);
            Array.Copy(magicCookie, 0, bytes, 0, magicCookie.Length);
            return bytes;
        }
    }
}