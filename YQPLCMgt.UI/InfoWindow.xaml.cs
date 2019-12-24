using Newtonsoft.Json;
using RabbitMQ.YQMsg;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using YQPLCMgt.Helper;
using YQPLCMgt.UI.ViewModel;

namespace YQPLCMgt.UI
{
    /// <summary>
    /// InfoWindow.xaml 的交互逻辑
    /// </summary>
    public partial class InfoWindow : Window
    {
        public InfoWindow()
        {
            InitializeComponent();
        }

        private void miEnableOrNot_Click(object sender, RoutedEventArgs e)
        {
            MenuItem mi = sender as MenuItem;
            ContextMenu ctxMenu = mi.Parent as ContextMenu;
            DataGrid dataGrid = ctxMenu.PlacementTarget as DataGrid;
            if (dataGrid.SelectedItems != null && dataGrid.SelectedItems.Count > 0)
            {
                foreach (var item in dataGrid.SelectedItems)
                {
                    DeviceBase device = item as DeviceBase;
                    if (device != null)
                    {
                        device.Enable = !device.Enable;
                    }
                }
            }
        }

        private string cfgFileName = "DeviceEnable.txt";
        private void SaveCfg()
        {
            try
            {
                FileStream fs = new FileStream(cfgFileName, FileMode.Create);
                StreamWriter writer = new StreamWriter(fs);
                foreach (var dev in viewModel.Source.ScanDevices)
                {
                    writer.WriteLine($"{dev.NO},{dev.Enable}");
                }
                foreach (var dev in viewModel.Source.StopDevices)
                {
                    writer.WriteLine($"{dev.NO},{dev.Enable}");
                }
                foreach (var dev in viewModel.Source.MachineDevices)
                {
                    writer.WriteLine($"{dev.NO},{dev.Enable}");
                }
                writer.Close();
                writer.Dispose();
                fs.Close();
                fs.Dispose();
            }
            catch (Exception)
            {
            }
        }
        private void ReadCfg()
        {
            try
            {
                FileStream fs = new FileStream(cfgFileName, FileMode.OpenOrCreate);
                StreamReader reader = new StreamReader(fs);
                while (!reader.EndOfStream)
                {
                    string data = reader.ReadLine();
                    if (string.IsNullOrEmpty(data))
                    {
                        continue;
                    }
                    try
                    {
                        string[] arrData = data.Split(',');
                        var scan = this.viewModel.Source.ScanDevices.FirstOrDefault(p => p.NO == arrData[0]);
                        if (scan != null)
                        {
                            scan.Enable = Convert.ToBoolean(arrData[1]);
                        }
                        var stop = this.viewModel.Source.StopDevices.FirstOrDefault(p => p.NO == arrData[0]);
                        if (stop != null)
                        {
                            stop.Enable = Convert.ToBoolean(arrData[1]);
                        }
                        var machine = this.viewModel.Source.MachineDevices.FirstOrDefault(p => p.NO == arrData[0]);
                        if (machine != null)
                        {
                            machine.Enable = Convert.ToBoolean(arrData[1]);
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
           
        }

        private MainViewModel viewModel;
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            viewModel = DataContext as MainViewModel;
            ReadCfg();
        }

        private void MiReSendMsg_Click(object sender, RoutedEventArgs e)
        {
            MenuItem mi = sender as MenuItem;
            ContextMenu ctxMenu = mi.Parent as ContextMenu;
            DataGrid dataGrid = ctxMenu.PlacementTarget as DataGrid;
            if (dataGrid.SelectedItems != null && dataGrid.SelectedItems.Count > 0)
            {
                foreach (var item in dataGrid.SelectedItems)
                {
                    DeviceBase device = item as DeviceBase;
                    if (device is StopDevice)
                    {
                        try
                        {
                            StopDevice stop = device as StopDevice;
                            PLCMsg msg = new PLCMsg()
                            {
                                NO = stop.NO,
                                DEVICE_TYPE = stop.DEVICE_TYPE,
                                PALLET_COUNT = stop.PALLET_COUNT,
                                STATUS = stop.STATUS,
                                MESSAGE_TYPE = "plc",
                                time_stamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")
                            };
                            string strMsg = JsonConvert.SerializeObject(msg);
                            viewModel?.mqClient?.SentMessage(strMsg);
                            MessageBox.Show("发送挡停信息完毕！");
                        }
                        catch (Exception ex)
                        {
                            MyLogLib.MyLog.WriteLog("手动发送挡停信息失败！", ex);
                            MessageBox.Show("发送挡停信息失败！");
                        }
                    }
                    else if (device is MachineDevice)
                    {
                        try
                        {
                            MachineDevice machine = device as MachineDevice;
                            PLCMsg msg = new PLCMsg()
                            {
                                NO = machine.NO,
                                DEVICE_TYPE = machine.DEVICE_TYPE,
                                PALLET_COUNT = machine.PALLET_COUNT,
                                STATUS = machine.STATUS,
                                MESSAGE_TYPE = "plc",
                                time_stamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")
                            };
                            string strMsg = JsonConvert.SerializeObject(msg);
                            viewModel?.mqClient?.SentMessage(strMsg);
                            MessageBox.Show("发送挡停信息完毕！");
                        }
                        catch (Exception ex)
                        {
                            MyLogLib.MyLog.WriteLog("手动发送专机信息失败！", ex);
                            MessageBox.Show("发送专机信息失败！");
                        }
                    }
                    else if (device is ScanDevice)
                    {
                        try
                        {
                            StopDevice stop = (device as ScanDevice).Stop;
                            if (stop == null)
                            {
                                return;
                            }
                            PLCMsg msg = new PLCMsg()
                            {
                                NO = stop.NO,
                                DEVICE_TYPE = stop.DEVICE_TYPE,
                                PALLET_COUNT = stop.PALLET_COUNT,
                                STATUS = stop.STATUS,
                                MESSAGE_TYPE = "plc",
                                time_stamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")
                            };
                            string strMsg = JsonConvert.SerializeObject(msg);
                            viewModel?.mqClient?.SentMessage(strMsg);
                            MessageBox.Show("上传档停状态完毕！");
                        }
                        catch (Exception ex)
                        {
                            MyLogLib.MyLog.WriteLog("手动发送挡停信息失败！", ex);
                            MessageBox.Show("发送挡停信息失败！");
                        }
                    }
                }
            }
        }

        private void MiReScan_Click(object sender, RoutedEventArgs e)
        {
            MenuItem mi = sender as MenuItem;
            ContextMenu ctxMenu = mi.Parent as ContextMenu;
            DataGrid dataGrid = ctxMenu.PlacementTarget as DataGrid;
            if (dataGrid.SelectedItems != null && dataGrid.SelectedItems.Count > 0)
            {
                foreach (var item in dataGrid.SelectedItems)
                {
                    ScanDevice scan = item as ScanDevice;
                    var stop = viewModel.Source.StopDevices.FirstOrDefault(p => p.Scan_Device_No == scan.NO && p.STATUS == 1);
                    if (stop == null || string.IsNullOrEmpty(stop.Scan_Device_No))
                    {
                        continue;
                    }
                    viewModel.TriggerScan(stop);
                }
            }
        }

        private void BtnSaveCfg_Click(object sender, RoutedEventArgs e)
        {
            SaveCfg();
            MessageBox.Show("保存完毕！");
        }
    }
}
