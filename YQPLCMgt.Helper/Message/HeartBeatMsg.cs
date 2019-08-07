using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YQPLCMgt.Helper
{
    public class HeartBeatMsg : MsgBase
    {
        public HeartBeatMsg()
        {
            this.MESSAGE_TYPE = "heart";
        }
        /// <summary>
        /// 设备状态
        /// </summary>
        public string STATUS { get; set; }
    }
}
