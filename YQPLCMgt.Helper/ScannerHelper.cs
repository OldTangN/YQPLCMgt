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
        public event Action<ScanDevice, string> OnScanned;
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

        protected void RaiseScanned(ScanDevice scan, string data)
        {
            try
            {
                OnScanned?.Invoke(this.Scanner, data);
            }
            catch (Exception ex)
            {
                string errMsg = "OnScanned事件委托异常！";
                RaiseError(errMsg);
                MyLog.WriteLog(errMsg, ex);
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

        public abstract bool Connect();

        public abstract void DisConnect();

        protected abstract bool Send(byte[] data);

        public void TriggerScan()
        {
            Task.Run(() =>
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

                byte[] bytStart, bytStop;
                bytStart = new byte[] { 0x16, 0x54, 0x0d };//启动
                Send(bytStart);
                Thread.Sleep(500);//扫描500ms
                bytStop = new byte[] { 0x16, 0x55, 0x0d };//停止
                Send(bytStop);
            });
        }
    }
}
