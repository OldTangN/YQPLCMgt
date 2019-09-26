using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YQPLCMgt.Helper
{
    public class MsgBase
    {
        /// <summary>
        /// 设备类型
        /// </summary>
        public string DEVICE_TYPE { get; set; }

        /// <summary>
        /// 设备编号
        /// </summary>
        public string NO { get; set; }

        /// <summary>
        /// 消息类型
        /// </summary>
        public string MESSAGE_TYPE { get; set; }

        /// <summary>
        /// 时间戳
        /// </summary>
        public string time_stamp { get; set; } = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    }
}
