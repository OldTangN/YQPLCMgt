using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YQServer.Device;
using RabbitMQ;

namespace YQServer
{
    public static class GlobalData
    {
        public static List<IDevice> Devices = new List<IDevice>();
        public static ServerMQ MQ = new ServerMQ();
    }
}
