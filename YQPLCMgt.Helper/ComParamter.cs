using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YQPLCMgt.Helper
{
    public class ComParamter
    {
        /// <summary>
        /// 2400-8-e-1
        /// </summary>
        public ComParamter()
        {
        }

        /// <summary>
        /// 串口参数,例如：COM3-115200-8-n-1
        /// </summary>
        /// <param name="comParam"></param>
        public ComParamter(string comParam)
        {
            string[] com_arr = comParam.Split('-');
            this.PortName = com_arr[0];
            this.BaudRate = Convert.ToInt32(com_arr[1]);
            this.DataBits = Convert.ToInt32(com_arr[2]);
            switch (com_arr[3].ToUpper())
            {
                case "E":
                    this.Parity = Parity.Even;
                    break;
                case "O":
                    this.Parity = Parity.Odd;
                    break;
                case "N":
                    this.Parity = Parity.None;
                    break;
                default:
                    this.Parity = Parity.None;
                    break;
            }
            this.StopBits = com_arr[4] == "1" ? StopBits.One : StopBits.None;
        }
        public ComParamter(string _PortName, int _BaudRate, Parity _Parity, int _DataBits, StopBits _StopBits)
        {
            this.PortName = _PortName;
            this.BaudRate = _BaudRate;
            this.Parity = _Parity;
            this.DataBits = _DataBits;
            this.StopBits = _StopBits;
        }
        /// <summary>
        /// COM1,COM2 
        /// </summary>
        public string PortName { set; get; } = "";

        /// <summary>
        /// 波特率
        /// </summary>
        public int BaudRate { set; get; } = 2400;

        /// <summary>
        /// 校验位
        /// </summary>
        public Parity Parity { set; get; } = Parity.Even;

        /// <summary>
        /// 比特位
        /// </summary>
        public int DataBits { set; get; } = 8;

        /// <summary>
        /// 停止位
        /// </summary>
        public StopBits StopBits { set; get; } = StopBits.One;

        /// <summary>
        /// 发送数据
        /// </summary>
        public List<byte> SendData { set; get; } = new List<byte>();

        /// <summary>
        /// 接收数据
        /// </summary>
        public List<byte> RecData { set; get; } = new List<byte>();

        /// <summary>
        /// 接收次数
        /// </summary>
        public int RcvCount { set; get; } = 3;
    }
}
