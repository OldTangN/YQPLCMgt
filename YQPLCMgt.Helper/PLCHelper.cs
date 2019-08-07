using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using MyLogLib;

namespace YQPLCMgt.Helper
{
    public class PLCHelper
    {
        public string IP { get; private set; }
        public int Port { get; private set; }
        public int SendTimeout { get; private set; }
        public int ReceiveTimeout { get; private set; }
        public event Action<string> OnShowMsg;
        /// <summary>
        /// PLC连接状态
        /// </summary>
        public bool IsConnected { get; set; } = false;

        public PLCHelper(string ip, int port, int sendTimeout = 2000, int rcvTimeout = 2000)
        {
            this.IP = ip;
            this.Port = port;
            this.SendTimeout = sendTimeout;
            this.ReceiveTimeout = rcvTimeout;
        }

        private Socket socket;
        public bool Connect()
        {
            try
            {
                socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
                socket.SendTimeout = this.SendTimeout;
                socket.ReceiveTimeout = this.ReceiveTimeout;
                socket.Connect(new IPEndPoint(IPAddress.Parse(IP), Port));
                IsConnected = true;
            }
            catch (Exception ex)
            {
                IsConnected = false;
                string errMsg = $"连接PLC失败,IP:{IP},Port:{Port}";
                ShowMsg(errMsg);
                MyLog.WriteLog(errMsg, ex);
                return false;
            }
            return true;
        }

        public void DisConnect()
        {
            IsConnected = false;
            try
            {
                socket.Disconnect(false);
                socket.Dispose();
                socket = null;
            }
            catch
            {
            }
        }

        private readonly object lockObj = new object { };

        /// <summary>
        /// 发送并接收应答（同步）
        /// </summary>
        /// <param name="protocol">发送协议内容</param>
        /// <param name="tryTimes"></param>
        /// <returns></returns>
        public PLCResponse Send(string cmdText, int tryTimes = 2)
        {
            lock (lockObj)
            {
                int tmpTimes = 1;
                PLCResponse resp = new PLCResponse();
                if (socket == null || !socket.Connected)
                {
                    if (!Connect())
                    {
                        resp.ErrorMsg = "连接PLC失败！";
                        return resp;
                    }
                }
                do
                {
                    tmpTimes++;
                    try
                    {
                        ShowMsg(DateTime.Now.ToString("HH:mm:ss.fff") + " 发送：" + cmdText);
                        byte[] sendBuffer = Encode(cmdText);
                        int sendLen = socket.Send(sendBuffer);
                        if (sendLen == sendBuffer.Length)//TODO:测试发送长度是否一致
                        {
                            resp.HasError = false;
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        string errMsg = $"发送失败，当前第{tmpTimes}次！";
                        MyLog.WriteLog(errMsg, ex);
                        ShowMsg(errMsg);
                        Connect();
                    }
                } while (tmpTimes <= tryTimes);

                if (resp.HasError)//发送失败
                {
                    resp.ErrorMsg = $"发送失败！尝试发送次数{tryTimes}次。";
                    return resp;
                }
                try
                {
                    byte[] buffer = new byte[1024 * 1024];
                    int rcvLength = socket.Receive(buffer);
                    resp.HasError = false;
                    resp.Text = Decode(buffer.Take(rcvLength).ToArray());
                    resp.Text = resp.Text.Replace("\n", "");
                    resp.Text = resp.Text.Split('\r')[0];//处理万一有粘包
                    ShowMsg(DateTime.Now.ToString("HH:mm:ss.fff") + " 接收：" + resp.Text);
                }
                catch (Exception ex)
                {
                    resp.HasError = true;
                    resp.ErrorMsg = "PLC响应异常！";
                    MyLog.WriteLog("PLC响应异常！", ex);
                }
                return resp;
            }
        }
      
        private void ShowMsg(string msg)
        {
            try
            {
                OnShowMsg?.Invoke(msg);
            }
            catch (Exception ex)
            {
                MyLog.WriteLog("ShowMsg回调函数异常！", ex);
            }
        }

        public PLCResponse SetOnePoint(string ioPoint, int val)
        {
            return Send($"WR {ioPoint} {val}\r");
        }

        private byte[] Encode(string msg)
        {
            return Encoding.UTF8.GetBytes(msg);
        }

        private string Decode(byte[] data)
        {
            return Encoding.UTF8.GetString(data);
        }
    }
}
