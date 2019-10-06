using RabbitMQ.YQMsg;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YQServer.Device
{
    public abstract class DeviceBase : IDevice
    {
        /// <summary>
        /// 设备编号
        /// </summary>
        public string NO { get; set; }

        public string NAME { get; set; }

        /// <summary>
        /// 当前接收到的消息
        /// </summary>
        public PLCMsg CurrMsg { get; set; }

        /// <summary>
        /// 上次放行时间
        /// </summary>
        public DateTime? LAST_PASS_TIME { get; set; } = null;
        /// <summary>
        /// 设备类型
        /// </summary>
        public string DEVICE_TYPE { get; set; }

        public abstract void DoWork(PLCMsg msg);

        public static IDevice GetDevice(string no)
        {
            var d = GlobalData.Devices.FirstOrDefault(p => p.NO == no);
            if (d == null)
            {
                d = (IDevice)System.Reflection.Assembly.GetExecutingAssembly().CreateInstance($"YQServer.Device.{no}");
                GlobalData.Devices.Add(d);
                d.NO = no;
                d.DEVICE_TYPE = no.Substring(0, no.Length - 2);
            }
            return d;
        }
    }
}
