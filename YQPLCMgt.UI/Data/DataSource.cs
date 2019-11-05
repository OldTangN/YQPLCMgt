using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YQPLCMgt.Helper;

namespace YQPLCMgt.UI
{
    public class DataSource
    {
        /// <summary>
        /// PLC通信协议
        /// </summary>
        public Dictionary<string, string> ProtocolData { get; set; }

        /// <summary>
        /// PLC错误信息
        /// </summary>
        public Dictionary<string, string> ErrorMsg { get; set; }

        /// <summary>
        /// PLC控制挡停和专机的DM值表
        /// </summary>
        public List<StopDevice> StopDevices { get; set; }

        /// <summary>
        /// 条码枪设置
        /// </summary>
        public List<ScanDevice> ScanDevices { get; set; }
        public List<MachineDevice> MachineDevices { get; set; }
        public DataSource()
        {
            this.ProtocolData = new Dictionary<string, string>();
            this.ErrorMsg = new Dictionary<string, string>();
            this.StopDevices = new List<StopDevice>();
            this.ScanDevices = new List<ScanDevice>();
            this.MachineDevices = new List<MachineDevice>();
            InitErrorMsg();
            InitProtocolData();
            InitStopDevice();
            InitScanDevice();
            InitMachineDevice();
        }

        /// <summary>
        /// PLC通信协议
        /// </summary>
        private void InitProtocolData()
        {
            this.ProtocolData.Add("查询机型 ", "?K");
            this.ProtocolData.Add("确认出错编号", "?E");
            this.ProtocolData.Add("确认运行模式", "?M");
            this.ProtocolData.Add("数据读取", "RD DM00001.U");
            this.ProtocolData.Add("连接数据读取S", "RDS DM00001.U 2");
            this.ProtocolData.Add("数据写入", "WR DM00001.U 10");
            this.ProtocolData.Add("连接数据写入S", "WRS DM00001.U 2 1 2");
        }

        /// <summary>
        /// PLC错误信息
        /// </summary>
        private void InitErrorMsg()
        {
            this.ErrorMsg.Add("E0", "软元件编号异常");
            this.ErrorMsg.Add("E1", "指令异常");
            this.ErrorMsg.Add("E2", "程序未登陆");
            this.ErrorMsg.Add("E4", "禁止写入");
            this.ErrorMsg.Add("E5", "主机错误");
            this.ErrorMsg.Add("E6", "无注释");
        }

        /// <summary>
        /// 初始化挡停
        /// </summary>
        private void InitStopDevice()
        {
            string strPath = AppDomain.CurrentDomain.BaseDirectory + "Data\\Stop.txt";
            List<string[]> lstStop = GetCSVData(strPath);
            if (lstStop.Count == 0)
            {
                return;
            }
            foreach (var stop in lstStop)
            {
                StopDevice stopDevice = new StopDevice(Convert.ToInt32(stop[0]),stop[1], stop[2], stop[3], stop[4], stop[5]);
                StopDevices.Add(stopDevice);
            }
        }

        /// <summary>
        /// 初始扫码枪
        /// </summary>
        private void InitScanDevice()
        {
            string strPath = AppDomain.CurrentDomain.BaseDirectory + "Data\\Scan.txt";
            List<string[]> lstScan = GetCSVData(strPath);
            if (lstScan.Count == 0)
            {
                return;
            }
            foreach (var scan in lstScan)
            {
                ScanDevice scanDevice = new ScanDevice(Convert.ToInt32(scan[0]), scan[1], scan[2], scan[3], Convert.ToInt32(scan[4]), 9004);
                ScanDevices.Add(scanDevice);
            }
        }

        private void InitMachineDevice()
        {
            string strPath = AppDomain.CurrentDomain.BaseDirectory + "Data\\Machine.txt";
            List<string[]> lstMachine = GetCSVData(strPath);
            if (lstMachine.Count == 0)
            {
                return;
            }
            foreach (var m in lstMachine)
            {
                MachineDevice machineDevice = new MachineDevice(Convert.ToInt32(m[0]), m[1], m[2], m[3], m[4], m[5], Convert.ToInt32(m[6]));
                MachineDevices.Add(machineDevice);
            }
        }

        /// <summary>
        /// 获取CSV文件内容，CSV文件首行为标题行
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <returns>字符串数组列表</returns>
        public List<string[]> GetCSVData(string path)
        {
            List<string[]> lstData = new List<string[]>();
            StreamReader reader = new StreamReader(path);
            string strHeader = reader.ReadLine();//首行标题
            int len = strHeader.Split(',').Length;
            while (!reader.EndOfStream)
            {
                string strData = reader.ReadLine();
                if (string.IsNullOrWhiteSpace(strData))
                {
                    continue;
                }
                string[] arrData = strData.Split(',');
                if (arrData.Length != len)//判断列数一致
                {
                    continue;
                }
                lstData.Add(arrData);
            }
            return lstData;
        }
    }
}
