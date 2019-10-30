using ManagedLXEAPI;
using Newtonsoft.Json;
using RabbitMQ;
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
using YQPLCMgt.Helper;
using YQPLCMgt.UI.ViewModel;

namespace YQPLCMgt.UI
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

        private MainViewModel viewModel;
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            viewModel = new MainViewModel();
            viewModel.OnShowMsg += AppendText;
            this.DataContext = viewModel;
            Task.Run(() => { viewModel.Init(); });
            List<DeviceBase> lstDevices = new List<DeviceBase>();
            lstDevices.AddRange(viewModel.Source.StopDevices);
            lstDevices.AddRange(viewModel.Source.MachineDevices);
            cmbDevices.ItemsSource = lstDevices;
        }

        private PLCHelper plc;

        private void BtnConnect_Click(object sender, RoutedEventArgs e)
        {
            plc?.DisConnect();
            plc = new PLCHelper(txtIP.Text, Convert.ToInt32(txtPort.Text), true);
            plc.OnShowMsg += AppendText;
            bool rlt = plc.Connect();
            if (rlt)
            {
                MessageBox.Show("PLC连接成功.");
            }
            else
            {
                MessageBox.Show("PLC连接失败.");
            }
        }

        private void BtnSend_Click(object sender, RoutedEventArgs e)
        {
            if (plc == null)
            {
                return;
            }
            var rlt = plc.Send(txtSend.Text + "\r");
            if (rlt.HasError)
            {
                MessageBox.Show(rlt.ErrorMsg);
            }
            else
            {

            }
        }

        private void AppendText(string txt)
        {
            rtxtMsg.Dispatcher.Invoke(() =>
            {
                if (rtxtMsg.Document.Blocks.Count > 200)
                {
                    rtxtMsg.Document.Blocks.Clear();
                }
                rtxtMsg.AppendText(DateTime.Now.ToString("HH:mm:ss.fff") + " -- " + txt + "\n");
            });

        }

        private void CmbSend_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbSend.SelectedIndex > 0)
            {
                txtSend.Text = cmbSend.SelectedValue.ToString();
            }
        }

        private void BtnDisConnect_Click(object sender, RoutedEventArgs e)
        {
            plc?.DisConnect();
        }

        private void BtnScan_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Window_Closed(object sender, EventArgs e)
        {
            viewModel.Dispose();
            Environment.Exit(0);
        }

        private void BtnTest_Click(object sender, RoutedEventArgs e)
        {
            string strmsg = "{\"COMMAND_ID\":\"2\",\"DEVICE_TYPE\":\"\",\"NO\":\"E01501\",\"MESSAGE_TYPE\":\"control\",\"time_stamp\":\"2019 - 10 - 24 16:45:46\"}";
            viewModel.MqClient_singleArrivalEvent(strmsg);
            return;
            //List<string> codes = new List<string> { "1", "22", "333", "444", "666666", "55555" };
            //codes.Sort((s1, s2) => { return s2.Length - s1.Length; });
            //return;
            TestMsg msg = new TestMsg();
            msg.DEVICE_TYPE = "E101";
            msg.NO = "1";
            msg.MESSAGE_TYPE = "execute";
            msg.Data.Add(new Data() { WORK_ID = "1", Bar_code = "bar1", result = "0" });
            msg.Data.Add(new Data() { WORK_ID = "2", Bar_code = "bar2", result = "0" });
            msg.Data.Add(new Data() { WORK_ID = "3", Bar_code = "bar3", result = "0" });
            string strJson = JsonConvert.SerializeObject(msg);//转Json字符串
            AppendText(strJson);//界面日志

            string str =
"{" + Environment.NewLine +
"\"DEVICE_TYPE\":\"1\"," + Environment.NewLine +
"\"NO\":\"1\"," + Environment.NewLine +
"\"MESSAGE_TYPE\":\"execute\"," + Environment.NewLine +
"\"DATA\":" + Environment.NewLine +
"[" + Environment.NewLine +
"{\"WORK_ID\":\"1\",\"Bar_code\":\"12757222006\",\"result\":\"0\"}," + Environment.NewLine +
"{\"WORK_ID\":\"2\",\"Bar_code\":\"12757222007\",\"result\":\"0\"}," + Environment.NewLine +
"{\"WORK_ID\":\"3\",\"Bar_code\":\"12757222008\",\"result\":\"0\"}" + Environment.NewLine +
"]," + Environment.NewLine +
"\"time_stamp\":\"2019-06-13 03:28:54\"" + Environment.NewLine +
"}";

            AppendText(str);
            TestMsg msg1 = JsonConvert.DeserializeObject<TestMsg>(str);//转实体对象
            MessageBox.Show(msg1.MESSAGE_TYPE);
        }

        private SocketScannerHelper scan;

        private int idx = 0;
        private void BtnScanTest_Click(object sender, RoutedEventArgs e)
        {
            scan?.DisConnect();
            scan = new SocketScannerHelper(new ScanDevice("E00102", "人工PCB工位挡停前扫码枪", combScanner.SelectedValue.ToString(), 9004));
            scan.OnScanned += (dev, stopNo, data) =>
            {
                AppendText(idx++ + " -- " + data);
                //格式条码+\r
                List<string> codes = data.Split('\r').ToList();
                codes.Sort((s1, s2) => { return s2.Length - s1.Length; });//托盘码最后传
                foreach (var barcode in codes)
                {
                    if (string.IsNullOrEmpty(barcode) || barcode.Length < 4)//TODO:条码长度过滤非法数据
                    {
                        continue;
                    }
                    AppendText(" ---- " + barcode);
                }
            };
            scan.OnError += AppendText;
            if (!scan.Connect())
            {
                AppendText("连接扫码枪失败！");
                return;
            }
            scan.TriggerScan("");//触发扫码枪指令
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(() => { viewModel.Init(); });
        }

        private void BtnDeviceCtl_Click(object sender, RoutedEventArgs e)
        {
            if (plc == null || !plc.IsConnected)
            {
                return;
            }
            string strCmd = "";
            if (chkRead.IsChecked.GetValueOrDefault())
            {
                strCmd = $"RD {cmbDevices.SelectedValue}.U\r";
            }
            else
            {
                strCmd = $"WR {cmbDevices.SelectedValue}.U {txtRWValue.Text}\r";
            }
            var rlt = plc.Send(strCmd);
            if (rlt.HasError)
            {
                MessageBox.Show(rlt.ErrorMsg);
            }
            else
            {
                if (chkRead.IsChecked.GetValueOrDefault())
                {
                    txtRWValue.Text = rlt.Text;
                }
            }
        }


        private int i = 0;
        private int i2 = 1;
        readonly object obj = new { };
        private void BtnRobot2_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(() =>
            {
                while (true)
                {
                    lock (obj)
                    {
                      
                        Thread t = new Thread((obj) =>
                        {
                            lock (obj)
                            {
                                Thread.Sleep(1000);
                                AppendText("2222222222:" + obj.ToString());
                                i2++;
                            }
                        });
                        t.IsBackground = true;
                        AppendText("开始22222222222" );
                        t.Start(i2);
                        i++;
                        AppendText("1111111111:" + i.ToString());
                    }
                    Thread.Sleep(1000);
                }
            });
            return;
            if (plc == null || !plc.IsConnected)
            {
                return;
            }
            btnRobot2.IsEnabled = false;
            Task.Run(() =>
            {
                int[] flags = new int[] { 11, 12, 13, 14, 15, 16 };//17
                string strCmd = "";
                while (true)
                {
                    Thread.Sleep(1000);
                    strCmd = $"RD DM219.U\r";
                    var rltRead = plc.Send(strCmd);
                    if (rltRead.HasError)
                    {
                        AppendText(rltRead.ErrorMsg);
                    }
                    else
                    {
                        if (Convert.ToInt32(rltRead.Text) == 4)
                        {
                            strCmd = $"WR DM219.U {flags[i]}\r";
                            i++;
                            if (i >= flags.Length)
                            {
                                i = 0;
                            }
                            var rltWrite = plc.Send(strCmd);
                            if (rltWrite.HasError)
                            {
                                AppendText(rltWrite.ErrorMsg);
                            }
                        }
                        else
                        {
                            continue;
                        }
                    }
                }
            });
        }

        PLCHelper plcBig = new PLCHelper("10.50.57.51", 8501, false);
        PLCHelper plcSmall = new PLCHelper("10.50.57.61", 8501, false);
        CancellationTokenSource cancelSource = null;
        private int j = 0;
        private void BtnRobot3_Click(object sender, RoutedEventArgs e)
        {
            plcBig?.DisConnect();
            plcSmall?.DisConnect();
            cancelSource?.Cancel();
            if (!plcBig.Connect() || !plcSmall.Connect())
            {
                MessageBox.Show("连接后2台PLC失败!");
                return;
            }
            cancelSource = new CancellationTokenSource();
            Task.Run(() =>
            {
                j = 0;
                string strCmd = "";
                int[] flags = new int[] { 11, 12, 13, 14, 15, 16, };//21, 22, 23, 24, 25, 26
                while (!cancelSource.IsCancellationRequested)
                {
                    Thread.Sleep(1000);
                    //判断单托盘到位
                    strCmd = $"RD DM116.U\r";
                    var rltRead1 = plcSmall.Send(strCmd);
                    if (rltRead1.HasError)
                    {
                        AppendText(rltRead1.ErrorMsg);
                        continue;
                    }
                    if (Convert.ToInt32(rltRead1.Text) != 1)
                    {
                        continue;
                    }
                    //判断大托盘到位
                    strCmd = $"RD DM115.U\r";
                    var rltRead2 = plcBig.Send(strCmd);
                    if (rltRead2.HasError)
                    {
                        AppendText(rltRead2.ErrorMsg);
                        continue;
                    }
                    if (Convert.ToInt32(rltRead2.Text) != 1)
                    {
                        continue;
                    }
                    Thread.Sleep(1000);
                    strCmd = $"RD DM234.U\r";//查询机器人状态
                    var rltReadRobot = plcSmall.Send(strCmd);
                    if (rltReadRobot.HasError)
                    {
                        AppendText(rltReadRobot.ErrorMsg);
                        continue;
                    }
                    if (Convert.ToInt32(rltReadRobot.Text) == 4)
                    {
                        strCmd = $"WR DM234.U {flags[j]}\r";//从第一表位开始抓取
                        var rltWriteRobot = plcSmall.Send(strCmd);
                        if (rltWriteRobot.HasError)
                        {
                            AppendText(rltWriteRobot.ErrorMsg);
                            continue;
                        }
                        while (true)//等待抓取完成
                        {
                            Thread.Sleep(1000);
                            strCmd = $"RD DM234.U\r";//查询机器人状态
                            rltWriteRobot = plcSmall.Send(strCmd);
                            if (rltWriteRobot.HasError)
                            {
                                AppendText(rltWriteRobot.ErrorMsg);
                                continue;
                            }
                            if (Convert.ToInt32(rltWriteRobot.Text) == 4)
                            {
                                strCmd = $"WR DM116.U 2\r";//放行小托盘
                                plcSmall.Send(strCmd);
                                if (flags[j] == 16)
                                {
                                    //放行大托盘
                                    strCmd = $"WR DM115.U 2\r";//放行大托盘
                                    plcBig.Send(strCmd);
                                }
                                break;
                            }
                        }
                        j++;
                        if (j >= flags.Length)
                        {
                            j = 0;
                        }
                    }
                }
            }, cancelSource.Token);

            MessageBox.Show("启动6转1抓表机器人成功！");
        }

        private void BtnShowInfo_Click(object sender, RoutedEventArgs e)
        {
            InfoWindow info = new InfoWindow();
            info.DataContext = viewModel;
            info.Show();
        }

        private void BtnClearBuffer_Click(object sender, RoutedEventArgs e)
        {
            viewModel.Source.MachineDevices?.ForEach(m => m.STATUS = -1);
            viewModel.Source.StopDevices?.ForEach(s => s.STATUS = -1);
            viewModel.Source.MachineDevices?.ForEach(m => m.PALLET_COUNT = 0);
            viewModel.Source.ScanDevices?.ForEach(s => s.Data = "");
            AppendText("清理完毕！\r\n");
        }
    }
}
