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

        private void MenuItem_Click(object sender, RoutedEventArgs e)
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
            SaveCfg();
        }

        private MainViewModel viewModel;
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            viewModel = DataContext as MainViewModel;
            ReadCfg();
        }
    }
}
