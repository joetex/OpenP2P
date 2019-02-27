using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OpenP2P
{
    class Program
    {
        static void Main(string[] args)
        {
            NetworkSocket server = new NetworkSocket(9000);
            NetworkSocket client = new NetworkSocket("127.0.0.1", 9000);

            server.Listen();

            for (int i = 0; i < 1000; i++)
            {
                client.Send("" + i);
                Thread.Sleep(10);
            }

            Console.WriteLine("FInished");
            Thread.Sleep(10000);
            //Console.ReadLine();
        }
    }
}
