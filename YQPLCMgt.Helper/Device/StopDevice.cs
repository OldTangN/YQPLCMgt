using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YQPLCMgt.Helper
{
    public class StopDevice : DeviceBase
    {
        public StopDevice(string no, string name, string dmAddrStatus,string scan_device_no,string plc_ip) : base(no, name)
        {
            this.DMAddr_Status = dmAddrStatus;
            this.Scan_Device_No = scan_device_no;
            this.PLCIP = plc_ip;
        }

        /// <summary>
        /// 专机状态 DM地址
        /// </summary>
        public string DMAddr_Status { get; set; }

        /// <summary>
        /// 对应自动扫码枪编号
        /// </summary>
        public string Scan_Device_No { get; set; }

        public string PLCIP { get; set; }

        /// <summary>
        /// 当前状态
        /// </summary>
        public int STATUS { get; set; } = -1;
    }
}
