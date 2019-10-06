using RabbitMQ.YQMsg;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YQServer.Device
{
    public interface IDevice
    {
        /// <summary>
        /// 设备编号
        /// </summary>
        string NO { get; set; }

        string NAME { get; set; }
        /// <summary>
        /// 设备类型
        /// </summary>
        string DEVICE_TYPE { get; set; }

        /// <summary>
        /// 上次放行时间
        /// </summary>
        DateTime? LAST_PASS_TIME { get; set; }

        /// <summary>
        /// 当前接收到的消息
        /// </summary>
        PLCMsg CurrMsg { get; set; }
        void DoWork(PLCMsg msg);
    }
}
