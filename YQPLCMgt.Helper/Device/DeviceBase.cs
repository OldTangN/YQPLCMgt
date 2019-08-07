using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YQPLCMgt.Helper
{
    public class DeviceBase
    {
        public DeviceBase(string no, string name)
        {
            this.NO = no;
            try
            {
                this.DEVICE_TYPE = no.Substring(0, 4);
            }
            catch (Exception e)
            {
                this.DEVICE_TYPE = e.Message;
            }
            this.NAME = name;
        }

        /// <summary>
        /// 设备编号
        /// </summary>
        public string NO { get; set; }

        /// <summary>
        /// 设备类型
        /// </summary>
        public string DEVICE_TYPE { get; set; }

        /// <summary>
        /// 设备名称
        /// </summary>
        public string NAME { get; set; }
    }
}
