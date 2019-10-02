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
            //Task.Run(() => { viewModel.Init(); });
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
                if (rtxtMsg.Document.Blocks.Count > 500)
                {
                    rtxtMsg.Document.Blocks.Clear();
                }
                if (txt.EndsWith("\r"))
                {
                    rtxtMsg.AppendText(txt);
                }
                else
                {
                    rtxtMsg.AppendText(txt + "\r");
                }
                rtxtMsg.ScrollToEnd();

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
        }

        private void BtnTest_Click(object sender, RoutedEventArgs e)
        {

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
            scan.OnScanned += (dev, data) =>
            {
                //格式条码+\r
                string[] codes = data.Split('\r');
                foreach (var barcode in codes)
                {
                    if (string.IsNullOrEmpty(barcode) || barcode.Length < 4)//TODO:条码长度过滤非法数据
                    {
                        continue;
                    }
                    AppendText(idx++ + " -- " + barcode);
                }
            };
            scan.OnError += AppendText;
            if (!scan.Connect())
            {
                AppendText("连接扫码枪失败！");
                return;
            }
            scan.TriggerScan();//触发扫码枪指令
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
    }
}
