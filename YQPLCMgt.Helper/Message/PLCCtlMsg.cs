using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YQPLCMgt.Helper
{
    /// <summary>
    /// 主控下发的设备控制类消息
    /// </summary>
    public class PLCCtlMsg : MsgBase
    {
        public PLCCtlMsg()
        {

        }
        /// <summary>
        /// 控制命令
        /// </summary>
        public int COMMAND_ID { get; set; }
    }
}
