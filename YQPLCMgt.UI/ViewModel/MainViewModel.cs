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
                "全部","192.168.0.10", "192.168.0.20", "192.168.0.30"
            });
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
                        "192.168.0.10", "192.168.0.20", "192.168.0.30"
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
                    plcs[i] = new PLCHelper(plc_ips[i], 8501, true);
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
            //格式条码+\r
            List<string> codes = data.Split('\r').ToList();
            codes.Sort((s1, s2) => { return s2.Length - s1.Length; });//托盘码最后传
            foreach (var barcode in codes)
            {
                if (string.IsNullOrEmpty(barcode) || barcode.Length < 4)//TODO:条码长度过滤非法数据
                {
                    continue;
                }
                try
                {
                    BarcodeMsg msg = new BarcodeMsg(scan.NO);
                    msg.BAR_CODE = barcode.Split(',')[0];//TODO: 条码,库编号 识别6表位托盘
                    string strJson = JsonConvert.SerializeObject(msg);
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
        private void MqClient_singleArrivalEvent(string data)
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

            if (msg.MESSAGE_TYPE == "control")
            {
                var ctlMsg = JsonConvert.DeserializeObject<ControlMsg>(data);
                var stop = _Source.StopDevices.FirstOrDefault(p => p.NO == ctlMsg.NO);
                var machine = _Source.MachineDevices.FirstOrDefault(p => p.NO == ctlMsg.NO);
                if (stop != null)
                {
                    var plc = plcs.FirstOrDefault(p => p.IP == stop.PLCIP);
                    var resp = plc.SetOnePoint(stop.DMAddr_Status, ctlMsg.COMMAND_ID);
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
                    var plc = plcs.FirstOrDefault(p => p.IP == machine.PLCIP);
                    var resp = plc.SetOnePoint(machine.DMAddr_Status, ctlMsg.COMMAND_ID);
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
            else if (msg.MESSAGE_TYPE == "task")
            {
                var tskMsg = JsonConvert.DeserializeObject<TaskMsg>(data);
            }
            else if (msg.MESSAGE_TYPE == "data")
            {
                var dataMsg = JsonConvert.DeserializeObject<DataMsg>(data);
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
            mqClient?.SentMessage(JsonConvert.SerializeObject(respMsg));
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
        private int chutiao_count = 0;
        private int fujiao_count = 0;
        private DateTime lastChutiaoTime = DateTime.Now;
        private DateTime lastFujiaoTime = DateTime.Now;
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
                        PLCResponse response = plc.Send($"RDS DM{start}.U {count}\r");
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
#if AUTO_PASS
                                    string pass_cmd = "2";//TODO:放行指令
#endif

                                    #region 上报状态至MQ服务器
                                    //获取专机
                                    var machine = _Source.MachineDevices.FirstOrDefault(p => p.DMAddr_Status == dmAddr && p.PLCIP == plc.IP);
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
                                        mqClient?.SentMessage(strJson);
                                        Thread.Sleep(50);
#if AUTO_PASS
                                        //TODO:多表位发送启动动作命令
                                        if (machine.Max_Pallet_Count > 1 && machine.Max_Pallet_Count == plcMsg.PALLET_COUNT)
                                        {
                                            var resp = plc.Send($"WR {dmAddr}.U 3\r");
                                            Thread.Sleep(1000);
                                            if (resp.HasError)
                                            {
                                                ShowMsg(resp.ErrorMsg);
                                            }
                                        }
#endif
                                    }

                                    //获取挡停
                                    var stop = _Source.StopDevices.FirstOrDefault(p => p.DMAddr_Status == dmAddr && plc.IP == p.PLCIP);
                                    if (stop != null)//专机状态PLC
                                    {
                                        #region 根据挡停状态触发扫码枪
                                        if (getValues[i] == 1 && !string.IsNullOrEmpty(stop.Scan_Device_No))//有托盘
                                        {
                                            //获取扫码枪
                                            var scan = scanHelpers?.FirstOrDefault(p => p.Scanner.NO == stop.Scan_Device_No);
                                            scan?.TriggerScan();//触发扫码枪，进行扫码
                                        }
                                        #endregion

#if AUTO_PASS
                                        if ("E00214" == stop.NO)//TODO:初调前挡停自动放行命令2、3
                                        {
                                            pass_cmd = (chutiao_count / 8 % 2 + 2).ToString();
                                            chutiao_count++;
                                        }
                                        if ("E00215" == stop.NO)//TODO:复校前挡停自动放行命令2、3、4、5
                                        {
                                            pass_cmd = (fujiao_count / 8 % 4 + 2).ToString();
                                            fujiao_count++;
                                        }
#endif

                                        PLCMsg plcMsg = new PLCMsg();
                                        plcMsg.DEVICE_TYPE = stop.DEVICE_TYPE;
                                        plcMsg.NO = stop.NO;
                                        plcMsg.STATUS = getValues[i];
                                        string strJson = JsonConvert.SerializeObject(plcMsg);
                                        mqClient?.SentMessage(strJson);
                                        Thread.Sleep(50);
                                    }

                                    #endregion
#if AUTO_PASS
                                    //TODO:测试代码，直接发放行命令
                                    if (getValues[i] == 1)
                                    {
                                        var resp = plc.Send($"WR {dmAddr}.U {pass_cmd}\r");
                                        if (machine?.DEVICE_TYPE == "E021" || machine.NO == "E022")
                                        {
                                            Thread.Sleep(10000);
                                        }
                                        else
                                        {
                                            Thread.Sleep(1000);
                                        }
                                        if (resp.HasError)
                                        {
                                            ShowMsg(resp.ErrorMsg);
                                        }
                                    }
#endif
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MyLog.WriteLog("MonitorDevice异常！", ex);
                    ShowMsg("MonitorDevice异常！" + ex.Message + "\r" + ex.StackTrace);
                }
                Thread.Sleep(5000);
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
