using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace OpenP2P
{
    public class TURNClient
    {
        public IPEndPoint turnHost = null;
        public string turnDefaultAddress = "34.70.87.145";
        public string turnAddress = "";
        public int turnPort = 0;
        public const int turnDefaultPort = 3478;

        NetworkProtocol protocol = null;

        public byte[] transactionID = null;
        
        string localAddress = "";
        

        public TURNClient(NetworkProtocol p)
        {
            protocol = p;
            protocol.AttachResponseListener(ChannelType.STUN, OnResponseSTUN);
            protocol.AttachErrorListener(NetworkErrorType.ErrorNoResponseSTUN, OnErrorSTUN);

            transactionID = GenerateTransactionID();
        }
        
       
        public void Connect(string address)
        {
            if (address.Length == 0)
                address = turnDefaultAddress;

            turnHost = protocol.GenerateHostAddressAndPort(address, turnDefaultPort);

            MessageSTUN message = protocol.Create<MessageSTUN>();
            message.method = STUNMethod.AllocateRequest;
            message.transactionID = transactionID;
            message.WriteString(STUNAttribute.Username, "joe");
            message.WriteString(STUNAttribute.Password, "test");
            message.WriteString(STUNAttribute.Realm, "test");
            //message.WriteEmpty(STUNAttribute.DontFragment);
            //message.WriteString(STUNAttribute.ServerName, "OpenP2P");
            //message.WriteUInt(STUNAttribute.Lifetime, 300);
            //message.WriteUInt(STUNAttribute.RequestedTransport, (17 << 24));

            //message.WriteMessageIntegrity();
            protocol.SendSTUN(turnHost, message, NetworkConfig.SocketReliableRetryDelay);

            Console.WriteLine("TURN Method: " + Enum.GetName(typeof(STUNMethod), message.method) + " (" + ((int)message.method).ToString("X") + ")");
            Console.WriteLine("TURN Request sent to: " + turnHost.ToString());
        }


        public void OnErrorSTUN(object sender, NetworkPacket packet)
        {
        }


        public void OnResponseSTUN(object sender, NetworkMessage msg)
        {
            NetworkPacket packet = (NetworkPacket)sender;
            MessageSTUN message = (MessageSTUN)msg;

            //if (message.method != STUNMethod.BindingResponse)
            //    return;
            
            
            //Console.WriteLine("STUN Host: " + packet.remoteEndPoint.ToString());
            Console.WriteLine("TURN Response Method: " + Enum.GetName(typeof(STUNMethod), message.method) + " (" + ((int)message.method).ToString("X") + ")");
            //Console.WriteLine("STUN Response Length: " + methodLength);

            Console.WriteLine("TURN Attributes: \n" + GetAttributeKeys(message));
            //Console.WriteLine("MappedAddress: " + mappedAddress);
            //Console.WriteLine("XorMappedAddress: " + message.Get(STUNAttribute.XorMappedAddress).ToString());
            //Console.WriteLine("SourceAddress: " + sourceAddress);
            //Console.WriteLine("ChangedAddress: " + changedAddress);
            
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
                attrKeys += key + "(" + id.ToString("X") + ") = " + entry.Value.ToString();
            }
            return attrKeys;
        }

        public static byte[] GenerateTransactionID()
        {
            Guid guid = Guid.NewGuid();
            return guid.ToByteArray();
        }
    }
}
