using Newtonsoft.Json;
using RabbitMQ.YQMsg;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YQServer.Device
{
    /// <summary>
    /// 2号机器人前扫码挡停 -- 上下表 换6表位托盘
    /// </summary>
    public class E00211 : DeviceBase
    {
        
        public override void DoWork(PLCMsg msg)
        {
            CurrMsg = msg;
        }
    }
}
