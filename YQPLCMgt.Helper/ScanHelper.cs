using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MyLogLib;

namespace YQPLCMgt.Helper
{
    public class ScanHelper
    {
        public ScanDevice Scanner { get; private set; }
        private Socket socket;
        public event Action<ScanDevice, string> OnScanned;
        public event Action<string> OnError;

        public ScanHelper(ScanDevice scanSetting)
        {
            this.Scanner = scanSetting;
            socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            socket.ReceiveTimeout = 1000;//1s接收超时
            socket.SendTimeout = 1000;//1s发送超时
        }

        private List<byte> Buffer = new List<byte>();

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

        private void RaiseScanned(ScanDevice scan, string data)
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

        private void RaiseError(string errMsg)
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

        private CancellationTokenSource cancellation;
        public bool Connect()
        {
            try
            {
                cancellation = new CancellationTokenSource();
                socket.Connect(new IPEndPoint(IPAddress.Parse(Scanner.IP), Scanner.Port));
                Task tsk = new Task(() =>
                {
                    Receive();
                }, cancellation.Token);
                tsk.Start();
                return true;
            }
            catch (Exception ex)
            {
                string errMsg = $"连接扫码枪{Scanner.IP}失败！";
                MyLog.WriteLog(errMsg, ex);
                RaiseError(errMsg);
            }
            return false;
        }

        private void Receive()
        {
            while (!cancellation.Token.IsCancellationRequested)
            {
                try
                {
                    byte[] buffer = new byte[1024];
                    int len = socket.Receive(buffer);
                    byte[] bytArrData = buffer.Take(len).ToArray();
                    string strCodes = Encoding.ASCII.GetString(bytArrData);
                    RaiseScanned(Scanner, strCodes);
                }
                catch (SocketException ex)//接收超时异常不处理
                {
                    continue;
                }
                catch (Exception ex)
                {
                    string errMsg = $"接收数据处理异常！{Scanner.IP}";
                    MyLog.WriteLog(errMsg, ex);
                    RaiseError(errMsg);
                }
            }
        }

        public void DisConnect()
        {
            try
            {
                cancellation?.Cancel();
                socket?.Close();
                socket?.Dispose();
            }
            catch (Exception)
            {
            }
        }

        public bool Send(byte[] data)
        {
            try
            {
                socket.Send(data);
            }
            catch (Exception ex)
            {
                string errMsg = $"向{Scanner.IP}发送命令失败！";
                MyLog.WriteLog(errMsg, ex);
                RaiseError(errMsg);
            }
            return true;
        }

        public void TriggerScan()
        {
            Task.Run(() =>
            {
                string strStart = "LON\r\n";
                string strStop = "LOFF\r\n";
                byte[] bytStart, bytStop;
                bytStart = Encoding.ASCII.GetBytes(strStart);
                Send(bytStart);
                Thread.Sleep(500);//扫描500ms
                bytStop = Encoding.ASCII.GetBytes(strStop);
                Send(bytStop); //test
            });
        }
    }
}
