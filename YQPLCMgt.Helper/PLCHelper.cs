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
        /// <summary>
        /// 显示日志报文
        /// </summary>
        private bool ShowLog { get; set; } = true;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <param name="showLog">显示发送、接收报文</param>
        /// <param name="sendTimeout">发送超时</param>
        /// <param name="rcvTimeout"></param>
        public PLCHelper(string ip, int port, bool showLog, int sendTimeout = 2000, int rcvTimeout = 2000)
        {
            this.IP = ip;
            this.Port = port;
            this.SendTimeout = sendTimeout;
            this.ReceiveTimeout = rcvTimeout;
            this.ShowLog = showLog;
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
                MyLog.WriteLog(errMsg, ex, "PLC");
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
                        resp.ErrorMsg = "[" + this.IP + "] 连接PLC失败！";
                        return resp;
                    }
                }
                do
                {
                    tmpTimes++;
                    try
                    {
                        string logMsg = DateTime.Now.ToString("HH:mm:ss.fff") + " [" + this.IP + "] 发送：" + cmdText;
                        if (ShowLog)
                        {
                            ShowMsg(logMsg);
                        }
                        if (!cmdText.StartsWith("RD"))
                        {
                            MyLog.WriteLog(logMsg, "PLC");
                        }
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
                        string errMsg = $"[{this.IP}] 发送失败，当前第{tmpTimes}次！";
                        MyLog.WriteLog(errMsg, ex, "PLC");
                        ShowMsg(errMsg);
                        Connect();
                    }
                    Thread.Sleep(100);
                } while (tmpTimes <= tryTimes);

                if (resp.HasError)//发送失败
                {
                    resp.ErrorMsg = $"[{this.IP}] 发送失败！尝试发送次数{tryTimes}次。";
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
                    string rcvMsg = DateTime.Now.ToString("HH:mm:ss.fff") + " [" + this.IP + "] 接收：" + resp.Text;
                    if (ShowLog)
                    {
                        ShowMsg(rcvMsg);
                    }
                    //MyLog.WriteLog(rcvMsg,"PLC");
                }
                catch (Exception ex)
                {
                    resp.HasError = true;
                    resp.ErrorMsg = "[" + this.IP + "] PLC响应异常！";
                    MyLog.WriteLog("[" + this.IP + "] PLC响应异常！", ex, "PLC");
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
                //MyLog.WriteLog("ShowMsg回调函数异常！", ex);
            }
        }

        public PLCResponse SetOnePoint(string ioPoint, int val)
        {
            MyLog.WriteLog($"{this.IP}设置{ioPoint}:{val}","PLC");
            return Send($"WR {ioPoint} {val}\r");
        }

        public PLCResponse ReadOnePoint(string ioPoint)
        {
            MyLog.WriteLog($"{this.IP}读取{ioPoint}", "PLC");
            return Send($"RD {ioPoint}.U\r");           
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
