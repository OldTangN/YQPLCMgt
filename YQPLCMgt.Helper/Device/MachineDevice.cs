using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YQPLCMgt.Helper
{

    public class MachineDevice : DeviceBase
    {
        public MachineDevice(string no, string name, string dmAddrPallet, string dmAddrStatus) : base(no, name)
        {
            this.DMAddr_Pallet = dmAddrPallet;
            this.DMAddr_Status = dmAddrStatus;
        }

        /// <summary>
        /// 托盘数DM地址
        /// </summary>
        public string DMAddr_Pallet { get; set; }

        /// <summary>
        /// 状态DM地址
        /// </summary>
        public string DMAddr_Status { get; set; }
    }

}
