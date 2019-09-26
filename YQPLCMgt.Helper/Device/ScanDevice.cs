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
            this.IOType = ScannerIO.Socket;
        }

        public ScanDevice(string no, string name, string comName) : base(no, name)
        {
            this.ComName = comName;
            this.IOType = ScannerIO.Com;
        }

        /// <summary>
        ///IP地址 端口默认9004
        /// </summary>
        public string IP { get; set; }

        public int Port { get; set; }

        public string ComName { get; set; }

        public ScannerIO IOType { get;private set; }
    }

    public enum ScannerIO
    {
        Socket = 1,
        Com = 2,
    }
}
