using System;
using System.Collections.Generic;
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
    }
}
