using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using YQPLCMgt.UI.Common;

namespace YQPLCMgt.UI
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
            if (SingleWinInstance.CreateMutex("YQPLCMgt.UI.exe"))
            {
                try
                {
                    MainWindow fmw = new MainWindow();
                    fmw.ShowDialog();
                }
                catch (Exception EX)
                {
                    MyLogLib.MyLog.WriteLog(EX);
                    MessageBox.Show(EX.Message);
                }
            }
            else//程序已经运行
            {
                Process p = SingleWinInstance.GetRunningInstance();
                if (p != null) //已经有应用程序副本执行
                {
                    SingleWinInstance.HandleRunningInstance(p);
                }
                else //启动第一个应用程序
                {

                }
                Environment.Exit(0);
            }
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            MessageBox.Show(e.Exception.Message);
        }
    }
}
