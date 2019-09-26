using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;

using MyLogLib;
using Newtonsoft.Json;
using RabbitMQ;
using RabbitMQ.YQMsg;
using YQPLCMgt.Helper;

namespace YQPLCMgt.UI.ViewModel
{
    public class MainViewModel : ObservableObject, IDisposable
    {

        #region public property
        public string IP_StopPLC { get => _IP_StopPLC; set => Set(ref _IP_StopPLC, value); }
        public string IP_MachinePLC { get => _IP_MachinePLC; set => Set(ref _IP_MachinePLC, value); }
        public int Port_StopPLC { get => _Port_StopPLC; set => Set(ref _Port_StopPLC, value); }
        public int Port_MachinePLC { get => _Port_MachinePLC; set => Set(ref _Port_MachinePLC, value); }
        public DataSource Source { get => _Source; set => _Source = value; }
        public bool IsConnected_StopPLC { get => _IsConnected_StopPLC; set => Set(ref _IsConnected_StopPLC, value); }
        public bool IsConnected_MachinePLC { get => _IsConnected_MachinePLC; set => Set(ref _IsConnected_MachinePLC, value); }
        public bool InitCompleted { get => _InitCompleted; set => Set(ref _InitCompleted, value); }
        #endregion

        #region private field
        private int _Port_StopPLC = 8501;
        private int _Port_MachinePLC = 8501;
        private string _IP_StopPLC = "192.168.0.10";
        private string _IP_MachinePLC = "192.168.0.11";
        private DataSource _Source;
        private CancellationTokenSource cancelToken;
        private ClientMQ mqClient;
        private bool _IsConnected_StopPLC;
        private bool _IsConnected_MachinePLC;
        private bool _InitCompleted = false;
        /// <summary>
        /// 专机PLC
        /// </summary>
        private PLCHelper plcMachine;
        /// <summary>
        /// 挡停PLC
        /// </summary>
        private PLCHelper plcStop;
        /// <summary>
        /// 扫码枪
        /// </summary>
        private List<ScannerHelper> scanHelpers;
        #endregion


        public MainViewModel()
        {
            Source = new DataSource();
        }

        #region 初始化
        public void Init()
        {
            InitMQ();
            InitPLC();
            InitScan();
            InitCompleted = true;
        }

        /// <summary>
        /// 初始化MQ Client
        /// <para>MQ需要手动关闭，否则后台线程不退出</para>
        /// </summary>
        public void InitMQ()
        {
            ShowMsg("初始化MQ...");
            mqClient?.Close();
            try
            {
                mqClient = new ClientMQ();
                mqClient.singleArrivalEvent += MqClient_singleArrivalEvent;
                mqClient.ReceiveMessage();
                ShowMsg("初始化MQ完毕！");
            }
            catch (Exception ex)
            {
                MyLog.WriteLog("初始化MQ失败！", ex);
                ShowMsg("初始化MQ失败！");
            }
        }

        /// <summary>
        /// 初始化扫码枪
        /// </summary>
        private void InitScan()
        {
            ShowMsg("初始化扫码枪...");
            if (scanHelpers != null)
            {
                scanHelpers.ForEach(p => p.DisConnect());
            }
            scanHelpers = new List<ScannerHelper>();
            string errComNames = "";
            foreach (var item in _Source.ScanDevices)
            {
                ScannerHelper scanHelper;
                if (item.IOType == ScannerIO.Socket)
                {
                    scanHelper = new SocketScannerHelper(item);
                }
                else
                {
                    scanHelper = new SerialScannerHelper(item);
                }
                scanHelper.OnScanned += ScannedCallback;
                scanHelper.OnError += ShowMsg;
                if (!scanHelper.Connect())
                {
                    errComNames += item.IP + ",";
                }
                scanHelpers.Add(scanHelper);
            }

            if (!string.IsNullOrEmpty(errComNames))
            {
                errComNames = errComNames.Remove(errComNames.Length - 1, 1);
                ShowMsg("条码枪串口初始化失败——" + errComNames);
            }
        }

        /// <summary>
        /// 初始化PLC
        /// </summary>
        private void InitPLC()
        {
            ShowMsg("初始化PLC...");
            plcStop?.DisConnect();
            plcMachine?.DisConnect();

            plcStop = new PLCHelper(IP_StopPLC, Port_StopPLC);
            plcStop.OnShowMsg += ShowMsg;
            IsConnected_StopPLC = plcStop.Connect();

            plcMachine = new PLCHelper(IP_MachinePLC, Port_MachinePLC);
            plcMachine.OnShowMsg += ShowMsg;
            IsConnected_MachinePLC = plcMachine.Connect();
            ShowMsg("初始化PLC完毕！");
        }
        #endregion

        /// <summary>
        /// 扫码枪接收回调
        /// </summary>
        /// <param name="scan"></param>
        /// <param name="strCodes"></param>
        private void ScannedCallback(ScanDevice scan, string strCodes)
        {
            ShowMsg("扫码:" + strCodes);
            //4230001001000222206603:365/111,4230001001000222206597:365/256,4230001001000222206580:361/372
            //数据格式 -- 条码:X/Y,条码:X/Y
            List<Barcode> lstBarcodes = new List<Barcode>();
            foreach (string oneData in strCodes.Split(','))
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
            //TODO：1扫N条码处理
            //托盘码+1个液晶码
            //托盘码+1个上壳码
            //托盘码+6个上壳码
            //托盘码最后上传
            //服务端记录非托盘码，当上报托盘码时，立即进行缓存数据与托盘码的绑定
            lstBarcodes.ForEach(p =>
            {
                try
                {
                    BarcodeMsg msg = new BarcodeMsg(scan.NO);
                    msg.BAR_CODE = p.Code;
                    string strJson = JsonConvert.SerializeObject(msg);
                    mqClient.SentMessage(strJson);
                }
                catch (Exception ex)
                {
                    string errMsg = "条码上报MQ服务器失败！";
                    MyLog.WriteLog(errMsg, ex);
                    OnShowMsg(errMsg);
                }
            });
        }

        /// <summary>
        /// MQ消息接收回调
        /// </summary>
        /// <param name="data"></param>
        private void MqClient_singleArrivalEvent(string data)
        {
            ShowMsg(data);
            MsgBase msg = JsonConvert.DeserializeObject<MsgBase>(data);
            if (msg.MESSAGE_TYPE == "control")
            {
                if (msg is ControlMsg ctlMsg)
                {
                    var stop = _Source.StopDevices.FirstOrDefault(p => p.NO == ctlMsg.NO);
                    var machine = _Source.MachineDevices.FirstOrDefault(p => p.NO == ctlMsg.NO);
                    if (stop != null)
                    {
                        var resp = plcStop.SetOnePoint(stop.DMAddr_Status, ctlMsg.COMMAND_ID);
                        if (!resp.HasError)
                        {
                            ResponseServer(stop.DEVICE_TYPE, stop.NO, ctlMsg.COMMAND_ID.ToString());
                        }
                        else
                        {
                            ShowMsg(resp.ErrorMsg);
                        }
                    }
                    if (machine != null)
                    {
                        var resp = plcStop.SetOnePoint(machine.DMAddr_Status, ctlMsg.COMMAND_ID);
                        if (!resp.HasError)
                        {
                            ResponseServer(machine.DEVICE_TYPE, machine.NO, ctlMsg.COMMAND_ID.ToString());
                        }
                        else
                        {
                            ShowMsg(resp.ErrorMsg);
                        }
                    }
                }
            }
            else if (msg.MESSAGE_TYPE == "task")
            {

            }
            else if (msg.MESSAGE_TYPE == "data")
            {

            }
            else
            {

            }
        }

        private void ResponseServer(string DEVICE_TYPE, string NO, string STATUS)
        {
            HeartBeatMsg respMsg = new HeartBeatMsg();
            respMsg.DEVICE_TYPE = DEVICE_TYPE;
            respMsg.NO = NO;
            respMsg.STATUS = STATUS;
            mqClient.SentMessage(JsonConvert.SerializeObject(respMsg));
        }

        public event Action<string> OnShowMsg;

        #region command

        #region StartCmd
        private RelayCommand _StartCmd;
        public RelayCommand StartCmd
        {
            get
            {
                if (_StartCmd == null)
                {
                    _StartCmd = new RelayCommand(Start, CanStart);
                }
                return _StartCmd;
            }
        }

        private void Start()
        {
            cancelToken = new CancellationTokenSource();
            Task.Run(new Action(MonitorStopDevice), cancelToken.Token);
            Task.Run(new Action(MonitorMachine), cancelToken.Token);
        }

        private bool CanStart()
        {
            return InitCompleted && IsConnected_StopPLC && IsConnected_MachinePLC;
        }

        private void MonitorStopDevice()
        {
            int start = 100;
            int count = 25;
            while (!cancelToken.IsCancellationRequested)
            {
                Thread.Sleep(2000);
                try
                {
                    PLCResponse response = plcStop.Send($"RDS DM{start}.U {count}\r");
                    if (response.HasError)
                    {
                        ShowMsg(response.ErrorMsg);
                    }
                    else
                    {
                        if (_Source.ErrorMsg.Keys.Contains(response.Text))
                        {
                            ShowMsg(_Source.ErrorMsg[response.Text]);
                        }
                        else
                        {
                            string[] getStrs = response.Text.Split(' ').ToArray();
                            int[] getValues = new int[getStrs.Length];
                            for (int i = 0; i < getValues.Length; i++)
                            {
                                getValues[i] = Convert.ToInt32(getStrs[i]);
                                #region 上报状态至MQ服务器
                                //获取挡停
                                var stop = _Source.StopDevices.FirstOrDefault(p => p.DMAddr_Status == "DM" + (start + i));
                                if (stop != null)//专机状态PLC
                                {
                                    #region 根据挡停状态触发扫码枪
                                    if (getValues[i] == 1 && !string.IsNullOrEmpty(stop.Scan_Device_No))//有托盘
                                    {
                                        //获取扫码枪
                                        var scan = scanHelpers.FirstOrDefault(p => p.Scanner.NO == stop.Scan_Device_No);
                                        scan?.TriggerScan();//触发扫码枪，进行扫码
                                    }
                                    #endregion

                                    PLCMsg plcMsg = new PLCMsg();
                                    plcMsg.DEVICE_TYPE = stop.DEVICE_TYPE;
                                    plcMsg.NO = stop.NO;
                                    plcMsg.STATUS = getValues[i];
                                    string strJson = JsonConvert.SerializeObject(plcMsg);
                                    mqClient.SentMessage(strJson);
                                    Thread.Sleep(50);
                                }
                                #endregion
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MyLog.WriteLog("MonitorStopDevice异常！", ex);
                    ShowMsg(ex.Message);
                }
            }
        }

        private void MonitorMachine()
        {
            int start = 200;
            int count = 45;
            while (!cancelToken.IsCancellationRequested)
            {
                try
                {
                    PLCResponse response = plcMachine.Send($"RDS DM{start}.U {count}\r");
                    if (response.HasError)
                    {
                        ShowMsg(response.ErrorMsg);
                    }
                    else
                    {
                        if (_Source.ErrorMsg.Keys.Contains(response.Text))
                        {
                            ShowMsg(_Source.ErrorMsg[response.Text]);
                        }
                        else
                        {
                            string[] getStrs = response.Text.Split(' ').ToArray();
                            int[] getValues = new int[getStrs.Length];
                            for (int i = 0; i < getValues.Length; i++)
                            {
                                string dmAddr = "DM" + (start + i);
                                getValues[i] = Convert.ToInt32(getStrs[i]);
                                #region 上报状态至MQ服务器
                                //获取专机
                                var machine = _Source.MachineDevices.FirstOrDefault(p => p.DMAddr_Status == dmAddr);
                                if (machine != null)//专机状态PLC
                                {
                                    PLCMsg plcMsg = new PLCMsg();
                                    plcMsg.DEVICE_TYPE = machine.DEVICE_TYPE;
                                    plcMsg.NO = machine.NO;
                                    plcMsg.STATUS = getValues[i];
                                    if (!string.IsNullOrEmpty(machine.DMAddr_Pallet))//有单独的托盘DM
                                    {
                                        int idx = Convert.ToInt32(machine.DMAddr_Pallet.Replace("DM", "")) - start;
                                        plcMsg.PALLET_COUNT = getValues[idx];
                                    }
                                    else
                                    {
                                        plcMsg.PALLET_COUNT = plcMsg.STATUS > 0 ? 1 : 0;
                                    }
                                    string strJson = JsonConvert.SerializeObject(plcMsg);
                                    mqClient.SentMessage(strJson);
                                    Thread.Sleep(50);
                                }
                                #endregion
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MyLog.WriteLog("MonitorMachine异常！", ex);
                    ShowMsg(ex.Message);
                }
                finally
                {
                    Thread.Sleep(2000);
                }
            }
        }

        #endregion

        #region StopCmd
        private RelayCommand _StopCmd;
        public RelayCommand StopCmd
        {
            get
            {
                if (_StopCmd == null)
                {
                    _StopCmd = new RelayCommand(Stop, () => InitCompleted);
                }
                return _StopCmd;
            }
        }

        private void Stop()
        {
            cancelToken?.Cancel();
            cancelToken = null;
        }
        #endregion

        #endregion

        public void Dispose()
        {
            mqClient?.Close();
            plcStop?.DisConnect();
            plcMachine?.DisConnect();
        }

        private void ShowMsg(string msg)
        {
            try
            {
                OnShowMsg?.Invoke(msg);
            }
            catch (Exception ex)
            {
                MyLog.WriteLog("OnShowMsg事件委托异常！", ex);
            }
        }
    }
}
