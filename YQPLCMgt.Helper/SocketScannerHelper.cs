using MyLogLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace YQPLCMgt.Helper
{
    public class SocketScannerHelper : ScannerHelper
    {
        private Socket socket;

        public SocketScannerHelper(ScanDevice scanSetting)
        {
            this.Scanner = scanSetting;

        }

        private List<byte> Buffer = new List<byte>();

        private bool AutoTrigger = false;

        private CancellationTokenSource cancellation;
        public override bool Connect(bool autoTrigger = false)
        {
            try
            {
                DisConnect();
                cancellation = new CancellationTokenSource();
                socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
                socket.ReceiveTimeout = 1000;//1s接收超时
                socket.SendTimeout = 1000;//1s发送超时
                socket.Connect(new IPEndPoint(IPAddress.Parse(Scanner.IP), Scanner.Port));
                this.AutoTrigger = autoTrigger;
                if (autoTrigger)
                {
                    Task.Run(() =>
                    {
                        while (!cancellation.IsCancellationRequested)
                        {
                            string data = Receive();
                            if (!string.IsNullOrEmpty(data))
                            {
                                RaiseScanned(Scanner, data);
                            }
                        }
                    }, cancellation.Token);
                }
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

        protected override string Receive()
        {
            string data = "";
            try
            {
                byte[] buffer = new byte[1024];
                int len = socket.Receive(buffer);
                byte[] bytArrData = buffer.Take(len).ToArray();
                data = Encoding.ASCII.GetString(bytArrData);
            }
            catch (SocketException ex)//接收超时异常不处理
            {
                if (!socket.Connected)
                {
                    try
                    {
                        Connect(this.AutoTrigger);
                    }
                    catch
                    {
                    }
                }
            }
            catch (Exception ex)
            {
                if (!socket.Connected)
                {
                    try
                    {
                        socket.Connect(new IPEndPoint(IPAddress.Parse(Scanner.IP), Scanner.Port));
                    }
                    catch
                    {
                    }
                }
                string errMsg = $"{Scanner.IP}接收数据处理异常！" + ex.Message;
                MyLog.WriteLog(errMsg, ex);
                RaiseError(errMsg);
            }
            return data;
        }

        public override void DisConnect()
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
        protected override bool Send(byte[] data)
        {
            try
            {
                if (!socket.Connected)
                {
                    Connect(this.AutoTrigger);
                }
                socket.Send(data);
                return true;
            }
            catch (Exception ex1)
            {
                string errMsg = $"向{Scanner.IP}发送命令失败！（1）";
                MyLog.WriteLog(errMsg, ex1);
                RaiseError(errMsg);
                try
                {
                    if (!socket.Connected)
                    {
                        Connect(this.AutoTrigger);
                    }
                    socket.Send(data);
                    return true;
                }
                catch (Exception ex2)
                {
                    string errMsg2 = $"向{Scanner.IP}发送命令失败！（2）";
                    MyLog.WriteLog(errMsg2, ex2);
                    RaiseError(errMsg2);
                    return false;
                }
            }
        }
    }
}
