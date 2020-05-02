using System;
using System.Net.NetworkInformation;
using System.Timers;

namespace OpenP2P
{
    /// <summary>
    /// Network Interface Traffic Watch
    /// by Mohamed Mansour
    /// 
    /// Free to use under GPL open source license!
    /// </summary>
    public class InterfaceTrafficWatch
    {

        public static NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces();

        public static void TestNetwork()
        {
            foreach (NetworkInterface adapter in adapters)
            {
                IPInterfaceProperties properties = adapter.GetIPProperties();
                IPv4InterfaceStatistics stats = adapter.GetIPv4Statistics();
                Console.WriteLine(adapter.Description);
                Console.WriteLine("     Speed .................................: {0}", (float)adapter.Speed / 8.0f / 1000.0f / 1000.0f);
                Console.WriteLine("     Output queue length....................: {0}", stats.OutputQueueLength);
                Console.WriteLine("     Multicast Support......................: {0}", adapter.SupportsMulticast);
            }
        }
    }
 }
