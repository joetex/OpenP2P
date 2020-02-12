using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenP2P
{
    public partial class NetworkClient
    {
        public void MainThread()
        {
            long difftime = 0;
            long startTime = NetworkTime.Milliseconds();
            long curTime = 0;

            while (true)
            {
                curTime = NetworkTime.Milliseconds();
                difftime = curTime - startTime;
                if (difftime < NetworkConfig.NetworkSendRate)
                {
                    continue;
                }

                startTime = curTime;

                NetworkFrame();
            }
        }

        public void NetworkFrame()
        {
            //SendHeartbeat();
        }
    }
}
