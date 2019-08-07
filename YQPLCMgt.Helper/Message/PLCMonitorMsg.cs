using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YQPLCMgt.Helper
{
    public class PLCMonitorMsg : MsgBase
    {
        public PLCMonitorMsg()
        {
            this.MESSAGE_TYPE = "plc";
            this.time_stamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }
        public int PALLET_COUNT { get; set; }

        public int STATUS { get; set; }
    }
}
