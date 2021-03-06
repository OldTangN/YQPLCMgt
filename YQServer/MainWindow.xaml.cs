﻿using MyLogLib;
using Newtonsoft.Json;
using RabbitMQ;
using RabbitMQ.YQMsg;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using YQServer.Device;

namespace YQServer
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void BtnStartServer_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(() =>
            {
                ShowMsg("初始化MQ服务...");
                try
                {
                    dtStart = DateTime.Now;
                    GlobalData.MQ = new ServerMQ();
                    GlobalData.MQ.singleArrivalEvent += MQ_singleArrivalEvent;
                    GlobalData.MQ.ReceiveMessage();
                    ShowMsg("MQ服务初始化成功！");
                }
                catch (Exception ex)
                {
                    ShowMsg("MQ服务初始失败!\r\n" + ex.Message + "\r\n" + ex.StackTrace);
                }
            });
        }

        private DateTime dtStart;
        private void MQ_singleArrivalEvent(string data)
        {
            ShowMsg(data);
            MsgBase msg = null;
            try
            {
                msg = JsonConvert.DeserializeObject<MsgBase>(data);
            }
            catch (Exception ex)
            {
                string errMsg = "协议格式错误！";
                MyLog.WriteLog(errMsg, ex);
                ShowMsg(errMsg);
                return;
            }
            //判断时间戳在启动时间之前的信息丢弃
            DateTime dtMsg;
            if (DateTime.TryParse(msg.time_stamp, out dtMsg))
            {
                if (dtMsg < dtStart)
                {
                    return;
                }
            }
            List<string> jiaobiaoNos = new List<string>()
            {
                "E00113", "E00114", "E00115", "E00117", "E02001",
                "E02101", "E02201"//先跑初调1和复校1
            };
            try
            {
                if (msg.MESSAGE_TYPE == "plc")
                {
                    if (!jiaobiaoNos.Contains(msg.NO))
                    {
                        return;
                    }
                    var dev = DeviceBase.GetDevice(msg.NO);
                    PLCMsg plcMsg = JsonConvert.DeserializeObject<PLCMsg>(data);
                    dev.DoWork(plcMsg);
                    this.Dispatcher.Invoke(() =>
                    {
                        if (!dgDevices.Items.Contains(dev))
                        {
                            dgDevices.Items.Add(dev);
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                ShowMsg(ex.Message + ex.StackTrace);
            }
        }

        private void ShowMsg(string msg)
        {
            this.Dispatcher.Invoke(() =>
            {
                if (rtxtMsg.Document.Blocks.Count > 20)
                {
                    rtxtMsg.Document.Blocks.Clear();
                }
                rtxtMsg.AppendText(msg + "\r\n");
            });
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(()=> {

                while (true)
                {
                    Thread.Sleep(10);
                    MyLog.WriteLog("fdskfjdslfjdslfjdslfjsljdskfsdkl", "SCAN");
                }
            });
        }
    }
}
