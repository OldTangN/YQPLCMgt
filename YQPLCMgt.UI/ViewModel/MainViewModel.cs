using MyLogLib;
using Newtonsoft.Json;
using RabbitMQ;
using RabbitMQ.YQMsg;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using YQPLCMgt.Helper;

namespace YQPLCMgt.UI.ViewModel
{
    public class MainViewModel : ObservableObject, IDisposable
    {
        #region public property
        public DataSource Source { get => _Source; set => _Source = value; }
        public bool PLC1_Status { get => _PLC1_Status; set => Set(ref _PLC1_Status, value); }
        public bool PLC2_Status { get => _PLC2_Status; set => Set(ref _PLC2_Status, value); }
        public bool PLC3_Status { get => _PLC3_Status; set => Set(ref _PLC3_Status, value); }
        public bool InitCompleted { get => _InitCompleted; set => Set(ref _InitCompleted, value); }

        public bool[] PLC_Status { get => _PLC_Status; set => Set(ref _PLC_Status, value); }

        private ICollectionView _LstPLCs;
        public ICollectionView LstPLCs => _LstPLCs;

        private List<DeviceBase> _MachineAndStop;
        public List<DeviceBase> MachineAndStop { get => _MachineAndStop; set => Set(ref _MachineAndStop, value); }

        private List<DeviceBase> _Scanners;
        public List<DeviceBase> Scanners { get => _Scanners; set => Set(ref _Scanners, value); }
        #endregion

        #region private field
        private bool _PLC1_Status = false;
        private bool _PLC2_Status = false;
        private bool _PLC3_Status = false;

        private bool[] _PLC_Status = new bool[3] { true, true, true };

        private DataSource _Source;
        private CancellationTokenSource cancelToken;
        private ClientMQ mqClient;

        private bool _InitCompleted = false;
        /// <summary>
        /// PLC
        /// </summary>
        private PLCHelper[] plcs;


        /// <summary>
        /// 扫码枪
        /// </summary>
        private List<ScannerHelper> scanHelpers;
        #endregion

        public MainViewModel()
        {
            Source = new DataSource();
            _LstPLCs = CollectionViewSource.GetDefaultView(new List<string>()
            {
                "全部","10.50.57.40", "10.50.57.51", "10.50.57.61"
            });
            MachineAndStop = new List<DeviceBase>();
            MachineAndStop.AddRange(Source.StopDevices);
            MachineAndStop.AddRange(Source.MachineDevices);
            Scanners = new List<DeviceBase>();
            Scanners.AddRange(Source.ScanDevices);
        }

        #region 初始化
        public async void Init()
        {
            await InitMQ();
            await InitPLC();
            await InitScan();
            InitCompleted = true;
        }

        /// <summary>
        /// 初始化MQ Client
        /// <para>MQ需要手动关闭，否则后台线程不退出</para>
        /// </summary>
        public Task InitMQ()
        {
            return Task.Run(() =>
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
            });
        }

        /// <summary>
        /// 初始化扫码枪
        /// </summary>
        private Task InitScan()
        {
            return Task.Run(() =>
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
                    scanHelper = new SocketScannerHelper(item);
                    scanHelper.OnScanned += ScannedCallback;
                    scanHelper.OnError += ShowMsg;
                    if (!scanHelper.Connect(item.NO == "E00101"))//有一把是自动触发的扫码枪
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
                else
                {
                    ShowMsg("条码枪初始化完毕!");
                }
            });
        }
        private bool IsAllPLCConnected = true;
        /// <summary>
        /// 初始化PLC
        /// </summary>
        private Task InitPLC()
        {
            return Task.Run(() =>
            {
                ShowMsg("初始化PLC...");
                string[] plc_ips;
                if (LstPLCs.CurrentItem.ToString() == "全部")
                {
                    plc_ips = new string[]
                    {
                        "10.50.57.40", "10.50.57.51", "10.50.57.61"
                    };
                }
                else
                {
                    plc_ips = new string[]
                    {
                        LstPLCs.CurrentItem.ToString()
                    };
                }
                IsAllPLCConnected = true;
                plcs?.ToList().ForEach(p => p?.DisConnect());
                plcs = new PLCHelper[plc_ips.Length];
                PLC_Status = new bool[plc_ips.Length];
                for (int i = 0; i < plcs.Length; i++)
                {
                    plcs[i] = new PLCHelper(plc_ips[i], 8501, false);
                    plcs[i].OnShowMsg += ShowMsg;
                    PLC_Status[i] = plcs[i].Connect();
                    IsAllPLCConnected &= PLC_Status[i];
                }
                RaisePropertyChanged("PLC_Status");
                ShowMsg("初始化PLC完毕！");
            });
        }
        #endregion

        /// <summary>
        /// 扫码枪接收回调
        /// </summary>
        /// <param name="scan"></param>
        /// <param name="strCodes"></param>
        private void ScannedCallback(ScanDevice scan, string data)
        {
            ShowMsg("扫码:" + scan.IP + " -- " + data);
            scan.Data = data?.Replace("\r", " | ").Replace("\n", "");
            scan.ScanTime = DateTime.Now.ToString("HH:mm:ss");
            RaisePropertyChanged("Scanners");
            //格式条码+\r
            List<string> codes = data.Split('\r').ToList();
            codes.Sort((s1, s2) => { return s2.Length - s1.Length; });//托盘码最后传
            //bool repeat = true; //TODO:判断上次扫码结果一样
            //if (scan.LastScan?.Count == codes.Count)
            //{
            //    foreach (var s in scan.LastScan)
            //    {
            //        if (!codes.Contains(s))
            //        {
            //            repeat = false;
            //        }
            //    }
            //}
            //if (repeat)
            //{
            //    return;
            //}
            foreach (var barcode in codes)
            {
                if (string.IsNullOrEmpty(barcode) || barcode.Length < 4 || barcode == "ERROR")//TODO:条码长度过滤非法数据
                {
                    continue;
                }
                try
                {
                    BarcodeMsg msg;
                    if (scan.NO == "E00125")//蜂鸣检测前两把扫码枪对应一个设备号
                    {
                        msg = new BarcodeMsg("E00106");
                    }
                    else
                    {
                        msg = new BarcodeMsg(scan.NO);
                    }
                    msg.BAR_CODE = barcode.Split(';')[0];//TODO: 条码,库编号 识别6表位托盘
                    string strJson = JsonConvert.SerializeObject(msg);
                    ShowMsg("发送：" + strJson);
                    mqClient?.SentMessage(strJson);
                }
                catch (Exception ex)
                {
                    string errMsg = "条码上报MQ服务器失败！";
                    MyLog.WriteLog(errMsg, ex);
                    OnShowMsg(errMsg);
                }
            }
        }

        /// <summary>
        /// MQ消息接收回调
        /// </summary>
        /// <param name="data"></param>
        public void MqClient_singleArrivalEvent(string data)
        {
            ShowMsg(data);
            MsgBase msg = null;
            try
            {
                msg = JsonConvert.DeserializeObject<MsgBase>(data);
            }
            catch (Exception ex)
            {
                string errMsg = "协议格式错误！";
                MyLog.WriteLog(errMsg, ex);
                ShowMsg(errMsg);
                return;
            }

            try
            {
                if (msg.MESSAGE_TYPE == "control")
                {
                    var ctlMsg = JsonConvert.DeserializeObject<ControlMsg>(data);
                    if (ctlMsg == null)
                    {
                        ShowMsg("control消息解析错误！");
                        return;
                    }
                    var stop = _Source.StopDevices.FirstOrDefault(p => p.NO == ctlMsg.NO);
                    var machine = _Source.MachineDevices.FirstOrDefault(p => p.NO == ctlMsg.NO);
                    if (stop != null)
                    {
                        var plc = plcs.FirstOrDefault(p => p.IP == stop.PLCIP);
                        if (plc == null)
                        {
                            return;
                        }
                        var resp = plc.SetOnePoint(stop.DMAddr_Status, ctlMsg.COMMAND_ID);
                        if (!resp.HasError)
                        {
                            stop.STATUS = ctlMsg.COMMAND_ID;
                            ResponseServer(stop, ctlMsg.COMMAND_ID);
                        }
                        else
                        {
                            ShowMsg(resp.ErrorMsg);
                        }
                    }
                    if (machine != null)
                    {
                        var plc = plcs.FirstOrDefault(p => p.IP == machine.PLCIP);
                        if (plc == null)
                        {
                            return;
                        }
                        var resp = plc.SetOnePoint(machine.DMAddr_Status, ctlMsg.COMMAND_ID);
                        if (!resp.HasError)
                        {
                            machine.STATUS = ctlMsg.COMMAND_ID;
                            ResponseServer(machine, ctlMsg.COMMAND_ID);
                        }
                        else
                        {
                            ShowMsg(resp.ErrorMsg);
                        }
                    }

                }
                //else if (msg.MESSAGE_TYPE == "task")
                //{
                //    var tskMsg = JsonConvert.DeserializeObject<TaskMsg>(data);
                //}
                //else if (msg.MESSAGE_TYPE == "data")
                //{
                //    var dataMsg = JsonConvert.DeserializeObject<DataMsg>(data);
                //}
                else
                {

                }
            }
            catch (Exception ex)
            {
                MyLog.WriteLog(ex);
                ShowMsg(ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        private void ResponseServer(DeviceBase device, int STATUS)
        {
            try
            {
                PLCMsg respMsg = new PLCMsg();
                respMsg.DEVICE_TYPE = device.DEVICE_TYPE;
                respMsg.NO = device.NO;
                respMsg.PALLET_COUNT = device.PALLET_COUNT;
                respMsg.STATUS = STATUS;
                string strMsg = JsonConvert.SerializeObject(respMsg);
                ShowMsg("应答：" + strMsg);
                mqClient?.SentMessage(strMsg);
            }
            catch (Exception ex)
            {
                MyLog.WriteLog(ex);
                ShowMsg(ex.Message + "\r\n" + ex.StackTrace);
            }
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
                    _StartCmd = new RelayCommand(Start);
                }
                return _StartCmd;
            }
        }

        private void Start()
        {
            cancelToken = new CancellationTokenSource();
            Task.Run(new Action(MonitorDevice), cancelToken.Token);
        }

        private bool CanStart()
        {
            return InitCompleted && IsAllPLCConnected;
        }

        private void MonitorDevice()
        {
            int start = 100;
            int count = 200;
            while (cancelToken != null && !cancelToken.IsCancellationRequested)
            {
                try
                {
                    foreach (var plc in plcs)
                    {
                        PLCResponse response = plc.Send($"RDS DM{start}.U {count}\r");//多台PLC读取批量DM
                        if (response.HasError)
                        {
                            ShowMsg(response.ErrorMsg);
                            continue;
                        }
                        if (_Source.ErrorMsg.Keys.Contains(response.Text))
                        {
                            ShowMsg(_Source.ErrorMsg[response.Text]);
                            continue;
                        }

                        string[] getStrs = response.Text.Split(' ').ToArray();
                        int[] getValues = new int[getStrs.Length];
                        for (int i = 0; i < getStrs.Length; i++)
                        {
                            getValues[i] = Convert.ToInt32(getStrs[i]);
                        }
                        for (int i = 0; i < getValues.Length; i++)
                        {
                            Thread thread = new Thread(new ParameterizedThreadStart((paramArr) =>
                            {
                                try
                                {
                                    object[] objParam = paramArr as object[];
                                    DealWithPLCStatus(Convert.ToInt32(objParam[0]), Convert.ToInt32(objParam[1]), objParam[2] as int[], objParam[3] as PLCHelper);
                                }
                                catch (Exception ex)
                                {
                                    MyLog.WriteLog("DealWithPLCStatus异常！", ex);
                                    ShowMsg($"处理PLC读取到的状态值异常！{ex.Message}\r{ex.StackTrace}");
                                }
                            }));
                            thread.IsBackground = true;
                            thread.Start(new object[] { start, i, getValues, plc });
                        }
                    }
                }
                catch (Exception ex)
                {
                    MyLog.WriteLog("MonitorDevice异常！", ex);
                    ShowMsg($"MonitorDevice异常！{ex.Message}\r{ex.StackTrace}");
                }
                finally
                {
                    RaisePropertyChanged("MachineAndStop");
                    string strSpan = System.Configuration.ConfigurationManager.AppSettings["PLC_LISTEN_SPAN"];
                    int span = 0;
                    if (!Int32.TryParse(strSpan, out span))
                    {
                        span = 1500;//默认1500ms
                    }
                    Thread.Sleep(span);
                }
            }
        }
        private void DealWithPLCStatus(int start, int i, int[] getValues, Helper.PLCHelper plc)
        {
            string dmAddr = "DM" + (start + i);

            #region 上报状态至MQ服务器
            //获取专机
            var machine = Source.MachineDevices.FirstOrDefault(p => p.DMAddr_Status == dmAddr && p.PLCIP == plc.IP);
            if (machine != null)//专机状态PLC
            {
                lock (machine)
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
                    if (machine.STATUS == getValues[i] && machine.PALLET_COUNT == plcMsg.PALLET_COUNT)//重复状态不上报
                    {
                        //ShowMsg($"重复状态不上传!" + machine.NO + ":" + machine.STATUS);
                        return;
                    }
                    machine.STATUS = getValues[i];
                    machine.PALLET_COUNT = plcMsg.PALLET_COUNT;
                    try
                    {
                        string strJson = JsonConvert.SerializeObject(plcMsg);
                        ShowMsg("发送：" + strJson);
                        mqClient?.SentMessage(strJson);
                    }
                    catch (Exception ex)
                    {
                        ShowMsg("上传专机状态失败!");
                        MyLog.WriteLog("上传专机状态失败！", ex);
                    }
                }
            }

            //获取挡停
            var stop = _Source.StopDevices.FirstOrDefault(p => p.DMAddr_Status == dmAddr && plc.IP == p.PLCIP);
            if (stop != null)//专机状态PLC
            {
                lock (stop)
                {
                    if (stop.STATUS == getValues[i])//重复状态不上报
                    {
                        //ShowMsg($"重复状态不上传!" + stop.NO + ":" + stop.STATUS);
                        return;
                    }
                    stop.STATUS = getValues[i];
                    #region 根据挡停状态触发扫码枪
                    if (stop.STATUS == 1 && !string.IsNullOrEmpty(stop.Scan_Device_No))//有托盘
                    {
                        //获取扫码枪
                        var scan = scanHelpers?.FirstOrDefault(p => p.Scanner.NO == stop.Scan_Device_No);
                        if (scan != null)
                        {
                            ShowMsg("触发扫码" + scan.Scanner.IP);
                            Task tsk = scan.TriggerScan();//触发扫码枪，进行扫码
                            if (scan.Scanner.NO == "E00106")//蜂鸣检测前绑码的扫码枪有2个
                            {
                                tsk.ContinueWith((tskobj) =>
                                {
                                    var scan2 = scanHelpers?.FirstOrDefault(p => p.Scanner.NO == "E00125");
                                    scan2?.TriggerScan();//触发扫码枪，进行扫码
                                });
                            }
                        }
                    }
                    #endregion

                    PLCMsg plcMsg = new PLCMsg();
                    plcMsg.DEVICE_TYPE = stop.DEVICE_TYPE;
                    plcMsg.NO = stop.NO;
                    plcMsg.STATUS = getValues[i];
                    plcMsg.PALLET_COUNT = plcMsg.STATUS > 0 ? 1 : 0;
                    stop.PALLET_COUNT = plcMsg.PALLET_COUNT;
                    try
                    {
                        string strJson = JsonConvert.SerializeObject(plcMsg);
                        ShowMsg("发送：" + strJson);
                        mqClient?.SentMessage(strJson);
                    }
                    catch (Exception ex)
                    {
                        ShowMsg("上传挡停状态失败!");
                        MyLog.WriteLog("上传挡停状态失败！", ex);
                    }
                }
            }
            #endregion
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
                    _StopCmd = new RelayCommand(Stop);
                }
                return _StopCmd;
            }
        }

        private void Stop()
        {
            cancelToken?.Cancel();
            cancelToken = null;
            if (PLC_Status != null && PLC_Status.Length > 0)
            {
                for (int i = 0; i < PLC_Status.Length; i++)
                {
                    PLC_Status[i] = false;
                }
            }
        }

        #endregion

        #region InitPlcCmd
        private RelayCommand _InitPlcCmd;
        public RelayCommand InitPlcCmd => _InitPlcCmd ?? (_InitPlcCmd = new RelayCommand(() => { InitPLC(); }));

        private RelayCommand _InitMQCmd;
        public RelayCommand InitMQCmd => _InitMQCmd ?? (_InitMQCmd = new RelayCommand(() => { InitMQ(); }));

        private RelayCommand _InitScannerCmd;
        public RelayCommand InitScannerCmd => _InitScannerCmd ?? (_InitScannerCmd = new RelayCommand(() => { InitScan(); }));
        #endregion

        #endregion

        public void Dispose()
        {
            mqClient?.Close();
            plcs?.ToList().ForEach(plc => plc?.DisConnect());
            scanHelpers?.ForEach(s => s.DisConnect());
        }

        private void ShowMsg(string msg)
        {
            try
            {
                OnShowMsg?.Invoke(msg);
            }
            catch (Exception ex)
            {
                MyLog.WriteLog("OnShowMsg委托调用异常！", ex);
            }
        }
    }
}
