using Newtonsoft.Json;
using RabbitMQ.YQMsg;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YQServer.Device
{
    public abstract class DeviceBase : IDevice
    {
        protected static int ChuTiaoCount = 0;

        protected static int CurrChutiaoNo = 1;

        protected static int FujiaoCount = 0;
        protected static int CurrFujiaoNo = 1;

        protected static DateTime LastChutiaoPassTime = DateTime.Now;
        protected static DateTime LastFujiaoPassTime = DateTime.Now;

        protected void AskPallet_Chutiao()
        {
            //判断专机空闲，且托盘不满8个（包括在途）
            var c1 = DeviceBase.GetDevice("E02101");
            var c2 = DeviceBase.GetDevice("E02102");
            var stop = DeviceBase.GetDevice("E00214");
            if (CurrChutiaoNo == 1)//当前放行至初调1
            {
                if (ChuTiaoCount < 8)//尚未放满8个
                {
                    if (stop.CurrMsg?.STATUS == 1)//有待放行的表
                    {
                        if ((DateTime.Now - LastChutiaoPassTime).Seconds < 5)
                        {
                            return;
                        }
                        LastChutiaoPassTime = DateTime.Now;
                        //放行至初调1
                        ControlMsg ctlMsg = new ControlMsg()
                        {
                            DEVICE_TYPE = stop.DEVICE_TYPE,
                            NO = stop.NO,
                            COMMAND_ID = 2,
                            MESSAGE_TYPE = "control",
                            time_stamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                        };
                        GlobalData.MQ.SentMessage(JsonConvert.SerializeObject(ctlMsg));//启动专机
                        ChuTiaoCount++;
                        Console.WriteLine("放行至初调1 -- " + ChuTiaoCount);
                    }
                }
                else
                {
                    if (c1.CurrMsg?.PALLET_COUNT == 8)//初调1的8个到位后可切换至初调2
                    {
                        ChuTiaoCount = 0;
                        CurrChutiaoNo = 2;
                    }
                }
            }
            if (CurrChutiaoNo == 2)//当前放行至初调2
            {
                if (ChuTiaoCount < 8)//尚未放满8个
                {
                    if (stop.CurrMsg?.STATUS == 1)//有待放行的表
                    {
                        if ((DateTime.Now - LastChutiaoPassTime).Seconds < 5)
                        {
                            return;
                        }
                        LastChutiaoPassTime = DateTime.Now;
                        //放行至初调2
                        ControlMsg ctlMsg = new ControlMsg()
                        {
                            DEVICE_TYPE = stop.DEVICE_TYPE,
                            NO = stop.NO,
                            COMMAND_ID = 3,
                            MESSAGE_TYPE = "control",
                            time_stamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                        };
                        GlobalData.MQ.SentMessage(JsonConvert.SerializeObject(ctlMsg));//启动专机
                        ChuTiaoCount++;
                        Console.WriteLine("放行至初调2 -- " + ChuTiaoCount);
                    }
                }
                else
                {
                    if (c2.CurrMsg?.PALLET_COUNT == 8)//初调2的8个到位后可切换至初调1
                    {
                        ChuTiaoCount = 0;
                        CurrChutiaoNo = 1;
                    }
                }
            }
        }

        protected void AskPallet_Fujiao()
        {
            var c1 = DeviceBase.GetDevice("E02201");
            var c2 = DeviceBase.GetDevice("E02202");
            var c3 = DeviceBase.GetDevice("E02203");
            var c4 = DeviceBase.GetDevice("E02204");
            var stop = DeviceBase.GetDevice("E00215");
            if (CurrFujiaoNo == 1)//当前放行至复校1
            {
                if (FujiaoCount < 8)//尚未放满8个
                {
                    if (stop.CurrMsg?.STATUS == 1)//有待放行的表
                    {
                        if ((DateTime.Now - LastFujiaoPassTime).Seconds < 5)
                        {
                            return;
                        }
                        LastFujiaoPassTime = DateTime.Now;
                        //放行至复校1
                        ControlMsg ctlMsg = new ControlMsg()
                        {
                            DEVICE_TYPE = stop.DEVICE_TYPE,
                            NO = stop.NO,
                            COMMAND_ID = 2,
                            MESSAGE_TYPE = "control",
                            time_stamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                        };
                        GlobalData.MQ.SentMessage(JsonConvert.SerializeObject(ctlMsg));//启动专机
                        FujiaoCount++;
                        Console.WriteLine("放行至复校1 -- " + FujiaoCount);
                    }
                }
                else
                {
                    if (c1.CurrMsg?.PALLET_COUNT == 8)//复校1的8个到位后可切换至复校2
                    {
                        FujiaoCount = 0;
                        CurrFujiaoNo = 2;
                    }
                }
            }

            if (CurrFujiaoNo == 2)//当前放行至复校2
            {
                if (FujiaoCount < 8)//尚未放满8个
                {
                    if (stop.CurrMsg?.STATUS == 1)//有待放行的表
                    {
                        if ((DateTime.Now - LastFujiaoPassTime).Seconds < 5)
                        {
                            return;
                        }
                        LastFujiaoPassTime = DateTime.Now;
                        //放行至复校2
                        ControlMsg ctlMsg = new ControlMsg()
                        {
                            DEVICE_TYPE = stop.DEVICE_TYPE,
                            NO = stop.NO,
                            COMMAND_ID = 3,
                            MESSAGE_TYPE = "control",
                            time_stamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                        };
                        GlobalData.MQ.SentMessage(JsonConvert.SerializeObject(ctlMsg));//启动专机
                        FujiaoCount++;
                        Console.WriteLine("放行至复校2 -- " + FujiaoCount);
                    }
                }
                else
                {
                    if (c2.CurrMsg?.PALLET_COUNT == 8)//初调2的8个到位后可切换至复校3
                    {
                        FujiaoCount = 0;
                        CurrFujiaoNo = 3;
                    }
                }
            }

            if (CurrFujiaoNo == 3)//当前放行至复校3
            {
                if (FujiaoCount < 8)//尚未放满8个
                {
                    if (stop.CurrMsg?.STATUS == 1)//有待放行的表
                    {
                        if ((DateTime.Now - LastFujiaoPassTime).Seconds < 5)
                        {
                            return;
                        }
                        LastFujiaoPassTime = DateTime.Now;
                        //放行至复校3
                        ControlMsg ctlMsg = new ControlMsg()
                        {
                            DEVICE_TYPE = stop.DEVICE_TYPE,
                            NO = stop.NO,
                            COMMAND_ID = 4,
                            MESSAGE_TYPE = "control",
                            time_stamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                        };
                        GlobalData.MQ.SentMessage(JsonConvert.SerializeObject(ctlMsg));//启动专机
                        FujiaoCount++;
                        Console.WriteLine("放行至复校3 -- " + FujiaoCount);
                    }
                }
                else
                {
                    if (c3.CurrMsg?.PALLET_COUNT == 8)//初调3的8个到位后可切换至复校4
                    {
                        FujiaoCount = 0;
                        CurrFujiaoNo = 4;
                    }
                }
            }

            if (CurrFujiaoNo == 4)//当前放行至复校4
            {
                if (FujiaoCount < 8)//尚未放满8个
                {
                    if (stop.CurrMsg?.STATUS == 1)//有待放行的表
                    {
                        if ((DateTime.Now - LastFujiaoPassTime).Seconds < 5)
                        {
                            return;
                        }
                        LastFujiaoPassTime = DateTime.Now;
                        //放行至复校4
                        ControlMsg ctlMsg = new ControlMsg()
                        {
                            DEVICE_TYPE = stop.DEVICE_TYPE,
                            NO = stop.NO,
                            COMMAND_ID = 5,
                            MESSAGE_TYPE = "control",
                            time_stamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                        };
                        GlobalData.MQ.SentMessage(JsonConvert.SerializeObject(ctlMsg));//启动专机
                        FujiaoCount++;
                        Console.WriteLine("放行至复校4 -- " + FujiaoCount);
                    }
                }
                else
                {
                    if (c4.CurrMsg?.PALLET_COUNT == 8)//初调4的8个到位后可切换至复校1
                    {
                        FujiaoCount = 0;
                        CurrFujiaoNo = 1;
                    }
                }
            }
        }

        /// <summary>
        /// 设备编号
        /// </summary>
        public string NO { get; set; }

        public string NAME { get; set; }

        /// <summary>
        /// 当前接收到的消息
        /// </summary>
        public PLCMsg CurrMsg { get; set; }

        /// <summary>
        /// 上次放行时间
        /// </summary>
        public DateTime? LAST_PASS_TIME { get; set; } = null;
        /// <summary>
        /// 设备类型
        /// </summary>
        public string DEVICE_TYPE { get; set; }

        public abstract void DoWork(PLCMsg msg);

        public static IDevice GetDevice(string no)
        {
            var d = GlobalData.Devices.FirstOrDefault(p => p.NO == no);
            if (d == null)
            {
                d = (IDevice)System.Reflection.Assembly.GetExecutingAssembly().CreateInstance($"YQServer.Device.{no}");
                GlobalData.Devices.Add(d);
                d.NO = no;
                d.DEVICE_TYPE = no.Substring(0, no.Length - 2);
            }
            return d;
        }
    }
}
