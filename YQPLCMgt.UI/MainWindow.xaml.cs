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

        private PLCHelper plcTest;

        private void BtnConnect_Click(object sender, RoutedEventArgs e)
        {
            plcTest?.DisConnect();
            plcTest = new PLCHelper(txtIP.Text, Convert.ToInt32(txtPort.Text), true);
            plcTest.OnShowMsg += AppendText;
            bool rlt = plcTest.Connect();
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
            if (plcTest == null)
            {
                return;
            }
            var rlt = plcTest.Send(txtSend.Text + "\r");
            if (rlt.HasError)
            {
                MessageBox.Show(rlt.ErrorMsg);
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
            plcTest?.DisConnect();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            viewModel.Dispose();
            Environment.Exit(0);
        }

        private SocketScannerHelper scan;

        private int idx = 0;
        private void BtnScanTest_Click(object sender, RoutedEventArgs e)
        {
            scan?.DisConnect();
            scan = new SocketScannerHelper(new ScanDevice(0, "", "", combScanner.SelectedValue.ToString(), 3, 9004));
            scan.OnScanned += (dev, stopNo, data) =>
            {
                AppendText(idx++ + " -- " + data);
                //格式 条码+\r   0x0D
                List<string> codes = data.Split('\r').ToList();
                codes.RemoveAll(p => string.IsNullOrEmpty(p) || p == "ERROR" || p.Length < 4);
                if (codes.Count > dev.MaxBarcodeCount)//扫码值超过最大扫码个数
                {
                    codes.RemoveRange(0, codes.Count - dev.MaxBarcodeCount);//保留最后符合数量的扫码值
                }
                codes.Sort((s1, s2) => { return s2.Length - s1.Length; });//托盘码最后传
            };
            scan.OnError += AppendText;
            if (!scan.Connect())
            {
                AppendText("连接扫码枪失败！");
                return;
            }
            scan.TriggerScan("");//触发扫码枪指令
        }

        private void BtnDeviceCtl_Click(object sender, RoutedEventArgs e)
        {
            var device = cmbDevices.SelectedItem as DeviceBase;
            if (device == null)
            {
                return;
            }
            PLCHelper plc = null;
            if (device is StopDevice)
            {
                var stop = device as StopDevice;
                plc?.DisConnect();
                plc = new PLCHelper(stop.PLCIP, Convert.ToInt32(txtPort.Text), false);
                if (!plc.Connect())
                {
                    return;
                }
            }
            else if (device is MachineDevice)
            {
                var machine = device as MachineDevice;
                plc?.DisConnect();
                plc = new PLCHelper(machine.PLCIP, Convert.ToInt32(txtPort.Text), false);
                if (!plc.Connect())
                {
                    return;
                }
            }
            else
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
            plc.DisConnect();
        }

        InfoWindow info;
        private void BtnShowInfo_Click(object sender, RoutedEventArgs e)
        {
            if (info != null)
            {
                info.Activate();
            }
            else
            {
                info = new InfoWindow();
                info.DataContext = viewModel;
                info.Closed += (p1, p2) => { info = null; };
                info.Owner = this;
                info.Show();
            }
        }

        private void BtnClearBuffer_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("是否全部重新上传挡停、专机状态，以及扫码枪内容？", "重要提示", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
            {
                viewModel.Source.ScanDevices?.ForEach(s => s.Data = "");
                viewModel.Source.MachineDevices?.ForEach(m => { m.STATUS = -1; m.PALLET_COUNT = 0; });
                viewModel.Source.StopDevices?.ForEach(s => s.STATUS = -1);
                AppendText("清理完毕！\r\n");
            }
        }

        private void btnTest_Click(object sender, RoutedEventArgs e)
        {
            //MessageBox.Show(viewModel.IsCorrectBarcode("\r").ToString());
            //MessageBox.Show(viewModel.IsCorrectBarcode("1-23-45").ToString());
            //MessageBox.Show(viewModel.IsCorrectBarcode("a1b23c4d5,1").ToString());
            //MessageBox.Show(viewModel.IsCorrectBarcode(".12312?").ToString());
            //MessageBox.Show(viewModel.IsCorrectBarcode("?12312").ToString());
        }
    }
}
