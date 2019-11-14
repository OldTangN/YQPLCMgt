using MyLogLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace YQPLCMgt.UI.Common
{
    public static class SingleWinInstance
    {
        private const int WS_SHOWNORMAL = 1;
        private const int WM_ACTIVATE = 0x0006;//激活
        private const int WS_VISIBLE = 268435456;//窗体可见
        private const int WS_MINIMIZEBOX = 131072;//有最小化按钮
        private const int WS_MAXIMIZEBOX = 65536;//有最大化按钮
        private const int WS_BORDER = 8388608;//窗体有边框
        private const int GWL_STYLE = (-16);//窗体样式
        private const int GW_HWNDFIRST = 0;
        private const int GW_OWNER = 4;
        private const int GW_CHILD = 5;
        private const int GW_HWNDNEXT = 2;
        private const int SW_HIDE = 0;
        private const int SW_SHOW = 5;

        [DllImport("user32.dll", EntryPoint = "SetParent")]
        public extern static IntPtr SetParent(IntPtr childPtr, IntPtr parentPtr);

        [DllImport("user32.dll", EntryPoint = "GetWindow")]//获取窗体句柄，hwnd为源窗口句柄
        /*wCmd指定结果窗口与源窗口的关系，它们建立在下述常数基础上：
        GW_CHILD
        寻找源窗口的第一个子窗口
        GW_HWNDFIRST
        为一个源子窗口寻找第一个兄弟（同级）窗口，或寻找第一个顶级窗口
        GW_HWNDLAST
        为一个源子窗口寻找最后一个兄弟（同级）窗口，或寻找最后一个顶级窗口
        GW_HWNDNEXT
        为源窗口寻找下一个兄弟窗口
        GW_HWNDPREV
        为源窗口寻找前一个兄弟窗口
        GW_OWNER
        寻找窗口的所有者
        */
        public static extern int GetWindow(int hwnd, int wCmd);


        [DllImport("User32.dll")]
        private static extern bool ShowWindowAsync(IntPtr hWnd, int cmdShow);
        [DllImport("user32.dll")]
        public static extern int ShowWindow(int hwnd, int nCmdShow);
        /*
        
      SW_HIDE   =   0;  隐藏窗口，活动状态给令一个窗口
      SW_SHOWNORMAL   =   1;
      SW_NORMAL   =   1; 
      SW_SHOWMINIMIZED   =   2; 最小化窗口，并将其激活  
      SW_SHOWMAXIMIZED   =   3; 最大化窗口，并将其激活    
      SW_MAXIMIZE   =   3; 
      SW_SHOWNOACTIVATE   =   4;  用最近的大小和位置显示一个窗口，同时不改变活动窗口
      SW_SHOW   =   5; 用当前的大小和位置显示一个窗口，同时令其进入活动状态
      SW_MINIMIZE   =   6;最小化窗口，活动状态给令一个窗口 
      SW_SHOWMINNOACTIVE   =   7;最小化一个窗口，同时不改变活动窗口 
      SW_SHOWNA   =   8;  用当前的大小和位置显示一个窗口，不改变活动窗口 
      SW_RESTORE   =   9;   用原来的大小和位置显示一个窗口，同时令其进入活动状态     
      SW_SHOWDEFAULT   =   10;      
      SW_MAX   =   10;   
         */
        [DllImport("User32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("User32.dll")]
        private static extern IntPtr GetTopWindow(IntPtr hWnd);

        [DllImport("User32.dll", EntryPoint = "SendMessage")]
        public static extern int SendMessage(IntPtr hwnd, int wmsg, IntPtr wparam, IntPtr lparam);

        private static Mutex mutex = null;
        public static bool CreateMutex()
        {
            return CreateMutex(Assembly.GetEntryAssembly().FullName);
        }

        public static bool CreateMutex(string name)
        {
            bool result = false;
            mutex = new Mutex(true, name, out result);
            return result;
        }
        public static void ReleaseMutex()
        {
            if (mutex != null)
            {
                mutex.Close();
            }
        }
        public static Process GetRunningInstance()
        {
            Process currentProcess = Process.GetCurrentProcess(); //获取当前进程
            //获取当前运行程序完全限定名
            string currentFileName = currentProcess.MainModule.FileName;
            //获取进程名为ProcessName的Process数组。
            Process[] processes = Process.GetProcessesByName(currentProcess.ProcessName);
            //遍历有相同进程名称正在运行的进程
            foreach (Process process in processes)
            {
                if (process.MainModule.FileName == currentFileName)
                {
                    if (process.Id != currentProcess.Id) //根据进程ID排除当前进程
                        return process;//返回已运行的进程实例
                }
            }
            return null;
        }
        public static bool HandleRunningInstance(Process instance)
        {
            try
            {
                //确保窗口没有被最小化或最大化
                ShowWindowAsync(instance.MainWindowHandle, WS_SHOWNORMAL);
                //设置为foreground window
                SetForegroundWindow(instance.MainWindowHandle);
                //int handle = GetWindow(instance.MainWindowHandle.ToInt32(), GW_HWNDFIRST);
                //MyLog.LogMessage(handle.ToString());
                //handle = GetWindow(instance.MainWindowHandle.ToInt32(), GW_CHILD);
                //SendMessage(GetTopWindow(instance.MainWindowHandle), WM_ACTIVATE, new IntPtr(), new IntPtr());
                //SendMessage(GetTopWindow(instance.MainWindowHandle), WM_ACTIVATE, new IntPtr(), new IntPtr());
                //隐藏窗体
                //ShowWindow(handle, SW_HIDE);
                //显示窗体  
                //ShowWindow(handle, SW_SHOW);
                //SetForegroundWindow(instance.MainWindowHandle);
                //MyLog.LogMessage(handle.ToString());
                //SetForegroundWindow(new IntPtr(handle));
                //SendMessage(instance.MainWindowHandle, WM_ACTIVATE, new IntPtr(), new IntPtr());

                //SendMessage(handle, WM_ACTIVATE, new IntPtr(), new IntPtr());
            }
            catch (Exception ex)
            {
                MyLog.WriteLog("激活失败", ex);
                return false;
            }
            return true;
        }

        public static bool HandleRunningInstance()
        {
            Process p = GetRunningInstance();
            if (p != null)
            {
                HandleRunningInstance(p);
                return true;
            }
            return false;
        }

    }
}
