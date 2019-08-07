using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YQPLCMgt.Helper
{
    public class BarcodeMsg : MsgBase
    {
        public BarcodeMsg(string scannerNo)
        {
            this.MESSAGE_TYPE = "message";
            this.DEVICE_TYPE = "E001";//扫码枪
            this.NO = scannerNo;
            this.time_stamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }

        public string BAR_CODE { get; set; }
    }
}
