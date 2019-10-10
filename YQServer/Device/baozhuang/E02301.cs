using Newtonsoft.Json;
using RabbitMQ.YQMsg;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YQServer.Device
{
    /// <summary>
    /// 3号机器人 -- 上下表
    /// </summary>
    public class E02301 : DeviceBase
    {
        /// <summary>
        /// 抓取位置
        /// </summary>
        private static List<int> flags = new List<int> { 11, 12, 13, 14, 15, 16 };
        private static int i = 0;//当前位置
        private static DateTime LastStatus4 = DateTime.Now;
        public override void DoWork(PLCMsg msg)
        {
            CurrMsg = msg;
            if (CurrMsg.STATUS == 4 || CurrMsg.STATUS == 0)//机器人抓完后
            {
                if ((DateTime.Now - LastStatus4).Seconds < 10)//两次状态4之间超过10秒认为机器人抓完
                {
                    return;
                }
                LastStatus4 = DateTime.Now;
                var stopBefore = DeviceBase.GetDevice("E00216");
                var stopAfter = DeviceBase.GetDevice("E00217");
                //判断前面托盘到位 && 判断后边托盘到位
                if (stopBefore.CurrMsg?.STATUS == 1 && stopAfter.CurrMsg?.STATUS == 1)
                {
                    #region 启动机器人
                    ControlMsg pickMsg = new ControlMsg()
                    {
                        DEVICE_TYPE = msg.DEVICE_TYPE,
                        NO = msg.NO,
                        COMMAND_ID = flags[i % flags.Count],
                        MESSAGE_TYPE = "control",
                        time_stamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                    };
                    i++;
                    GlobalData.MQ.SentMessage(JsonConvert.SerializeObject(pickMsg));
                    #endregion

                    #region 放行机器人后挡停 
                    Task.Run(() =>
                    {
                        System.Threading.Thread.Sleep(6000);//等5秒机器人放完
                        ControlMsg ctl_af_Msg = new ControlMsg()
                        {
                            NO = stopAfter.NO,
                            DEVICE_TYPE = stopAfter.DEVICE_TYPE,
                            COMMAND_ID = 2,
                            MESSAGE_TYPE = "control",
                            time_stamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                        };
                        GlobalData.MQ.SentMessage(JsonConvert.SerializeObject(ctl_af_Msg));
                    });
                    #endregion

                    #region 放行机器人前挡停

                    if (i > 0 && i % flags.Count == 0)//判断抓完6个
                    {
                        Task.Run(() =>
                        {
                            System.Threading.Thread.Sleep(2000);//等2秒机器人抓走
                            ControlMsg ctl_br_Msg = new ControlMsg()
                            {
                                NO = stopBefore.NO,
                                DEVICE_TYPE = stopBefore.DEVICE_TYPE,
                                COMMAND_ID = 2,
                                MESSAGE_TYPE = "control",
                                time_stamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                            };
                            GlobalData.MQ.SentMessage(JsonConvert.SerializeObject(ctl_br_Msg));
                        });
                    }
                    #endregion
                }
            }
        }
    }
}
