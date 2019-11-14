using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YQPLCMgt.Helper
{
    public class ScanDevice : DeviceBase
    {
        public ScanDevice(int lineno, string no, string name, string ip, int maxBarcodeCount, int port) : base(lineno, no, name)
        {
            this.IP = ip;
            this.Port = port;
            this.MaxBarcodeCount = maxBarcodeCount;
        }

        /// <summary>
        /// 最大扫码个数
        /// </summary>
        public int MaxBarcodeCount { get; set; } = 1;

        /// <summary>
        ///IP地址 端口默认9004
        /// </summary>
        public string IP { get; set; }

        public int Port { get; set; }

        public List<string> LastScan { get; set; } = new List<string>();

        private string _Data;
        /// <summary>
        /// 扫描到的有效条码，以竖线分割
        /// </summary>
        public string Data { get => _Data; set => Set(ref _Data, value); }

        private string _ScanTime;
        public string ScanTime { get => _ScanTime; set => Set(ref _ScanTime, value); }
    }
}
