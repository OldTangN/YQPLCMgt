using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YQPLCMgt.Helper
{
    public class StopDevice : DeviceBase
    {
        public StopDevice(int lineno, string no, string name, string dmAddrStatus, string scan_device_no, string plc_ip) : base(lineno, no, name)
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

        private int _STATUS = -1;
        /// <summary>
        /// 当前状态
        /// </summary>
        public int STATUS { get => _STATUS; set => Set(ref _STATUS, value); }
    }
}
