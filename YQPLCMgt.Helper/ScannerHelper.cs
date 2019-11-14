using MyLogLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace YQPLCMgt.Helper
{
    public abstract class ScannerHelper
    {
        public ScanDevice Scanner { get; protected set; }
        public event Action<ScanDevice, string, string> OnScanned;
        public event Action<string> OnError;

        //private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        //{
        //    int len = serialPort.BytesToRead;
        //    byte[] buffer = new byte[len];
        //    serialPort.Read(buffer, 0, len);
        //    Buffer.AddRange(buffer);//断帧、粘包处理
        //    int idx_0a;//0x0d 0x0a 结束符
        //    while ((idx_0a = Buffer.IndexOf(0x0a)) != -1)//断帧、粘包处理
        //    {
        //        byte[] data = Buffer.Take(idx_0a - 1).ToArray();//截取0x0d 0x0a前报文
        //        Buffer.RemoveRange(0, idx_0a + 1);
        //        string strCodes = Encoding.Default.GetString(data);//TODO:编码格式 Default=GB2312
        //        RaiseScanned(this.Scanner, strCodes);
        //    }
        //}

        protected void RaiseScanned(ScanDevice scan, string stopNo, string data)
        {
            try
            {
                OnScanned?.Invoke(this.Scanner, stopNo, data);
            }
            catch (Exception ex)
            {
                string errMsg = "OnScanned事件委托异常！";
                RaiseError(errMsg);
                //MyLog.WriteLog(errMsg, ex);
            }
        }

        protected void RaiseError(string errMsg)
        {
            try
            {
                OnError?.Invoke(errMsg);
            }
            catch (Exception ex)
            {
                MyLog.WriteLog("OnError事件委托异常！", ex);
            }
        }

        public abstract bool Connect(bool autoTrigger = false);

        public abstract void DisConnect();

        protected abstract bool Send(byte[] data);

        /// <summary>
        /// 触发扫码，并等待结果返回
        /// </summary>
        public Task TriggerScan(string stopNo)
        {
            Task tsk = new Task((obj) =>
            {
                #region 基恩士扫码枪默认触发命令
                //string strStart = "LON\r\n";
                //string strStop = "LOFF\r\n";
                //byte[] bytStart, bytStop;
                //bytStart = Encoding.ASCII.GetBytes(strStart);
                //Send(bytStart);
                //Thread.Sleep(500);//扫描500ms
                //bytStop = Encoding.ASCII.GetBytes(strStop);
                //Send(bytStop);
                #endregion

                MyLog.WriteLog($"{this.Scanner.NAME}{this.Scanner.IP}触发扫码");
                byte[] bytStart, bytStop;
                bytStart = new byte[] { 0x16, 0x54, 0x0d };//启动
                if (Connect(false))//重新连接，防止历史数据干扰
                {
                    Send(bytStart);
                    //TODO:根据不同扫码枪设置超时时间
                    Thread.Sleep(1500);//扫描时间1500ms  不可减少，防止119扫码枪扫码超时
                    bytStop = new byte[] { 0x16, 0x55, 0x0d };//停止
                    Send(bytStop);
                    Thread.Sleep(100);
                    string data = Receive();
                    if (string.IsNullOrEmpty(data) || data.Length < 4)//未扫到，重新扫
                    {
                        Thread.Sleep(100);
                        Send(bytStart);
                        Thread.Sleep(1500);//扫描时间1500ms  不可减少，防止119扫码枪扫码超时
                        bytStop = new byte[] { 0x16, 0x55, 0x0d };//停止
                        Send(bytStop);
                        data = Receive();
                    }
                    DisConnect();
                    RaiseScanned(Scanner, obj?.ToString(), data);
                }
                else
                {
                    RaiseError($"{this.Scanner.NAME}{this.Scanner.IP}扫码枪无法连接!");
                }
            }, stopNo);
            tsk.Start();
            return tsk;
        }

        protected abstract string Receive();
    }
}
