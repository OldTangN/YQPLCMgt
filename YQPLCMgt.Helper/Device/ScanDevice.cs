using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YQPLCMgt.Helper
{
    public class ScanDevice : DeviceBase
    {
        public ScanDevice(string no, string name, string ip, int port = 9004) : base(no, name)
        {
            this.IP = ip;
            this.Port = port;
        }

        /// <summary>
        ///IP地址 端口默认9004
        /// </summary>
        public string IP { get; set; }

        public int Port { get; set; }

        public List<string> LastScan { get; set; } = new List<string>();

        public string Data { get; set; }
    }
}
