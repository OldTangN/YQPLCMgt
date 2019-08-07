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
using ManagedLXEAPI;
using Newtonsoft.Json;
using RabbitMQ;
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
        }

        private PLCHelper plc;

        private void BtnConnect_Click(object sender, RoutedEventArgs e)
        {
            plc?.DisConnect();
            plc = new PLCHelper(txtIP.Text, Convert.ToInt32(txtPort.Text));
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
            //SCANMsg msg = new SCANMsg();
            //msg.NO = "23";
            //msg.data = new List<string>() { "aaaaa", "bbbbb" };
            //string strJson = JsonConvert.SerializeObject(msg);
            //AppendText(strJson);
            //msg = new SCANMsg();
            //strJson = JsonConvert.SerializeObject(msg);
            //AppendText(strJson);
            //string ss = "{\"data\":[\"aaaaa\",\"bbbbb\"],\"DEVICE_TYPE\":\"01\",\"NO\":\"23\",\"MESSAGE_TYPE\":null,\"time_stamp\":null}";
            //SCANMsg msg1 = JsonConvert.DeserializeObject<SCANMsg>(ss);
            //SeperatorMsg msg2 = JsonConvert.DeserializeObject<SeperatorMsg>(ss);
        }

        private ScanHelper scan;
        private Thread scanThread;
        private int idx = 0;
        private void BtnScanTest_Click(object sender, RoutedEventArgs e)
        {
            scan?.DisConnect();
            try { scanThread?.Abort(); } catch { };
            scan = new ScanHelper(new ScanDevice("E00102", "人工PCB工位挡停前扫码枪", txtScannerIp.Text));
            scan.OnScanned += (dev, data) =>
            {
                AppendText(idx++ + " -- " + data);
                try
                {
                    List<Barcode> lstBarcodes = new List<Barcode>();
                    foreach (string oneData in data.Split(','))
                    {
                        Barcode barcode = new Barcode()
                        {
                            Code = oneData.Split(':')[0],
                            X = Convert.ToInt32(oneData.Split(':')[1].Split('/')[0]),
                            Y = Convert.ToInt32(oneData.Split(':')[1].Split('/')[1])
                        };
                        lstBarcodes.Add(barcode);
                    }
                    lstBarcodes.Sort((b1, b2) =>
                    {
                        return b1.Y - b2.Y;//从上到下排序
                    });
                    lstBarcodes.ForEach(p => AppendText(p.Code));
                }
                catch (Exception)
                {
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
    }
}
