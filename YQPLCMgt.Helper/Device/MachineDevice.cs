using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YQPLCMgt.Helper
{

    public class MachineDevice : DeviceBase
    {
        public MachineDevice(string no, string name, string dmAddrPallet, string dmAddrStatus, string plc_ip,int max_pallet_count) : base(no, name)
        {
            this.DMAddr_Pallet = dmAddrPallet;
            this.DMAddr_Status = dmAddrStatus;
            this.PLCIP = plc_ip;
            this.Max_Pallet_Count = max_pallet_count;
        }

        /// <summary>
        /// 托盘数DM地址
        /// </summary>
        public string DMAddr_Pallet { get; set; }

        /// <summary>
        /// 状态DM地址
        /// </summary>
        public string DMAddr_Status { get; set; }

        public string PLCIP { get; set; }

        public int Max_Pallet_Count { get; set; }

        /// <summary>
        /// 当前状态
        /// </summary>
        public int STATUS { get; set; } = -1;
    }
}
