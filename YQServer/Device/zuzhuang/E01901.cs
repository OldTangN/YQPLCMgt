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
    /// 2号机器人 -- 上下表
    /// </summary>
    public class E01901 : DeviceBase
    {
        /// <summary>
        /// 抓取位置
        /// </summary>
        private static List<int> flags = new List<int> { 11, 12, 13, 14, 15, 16 };
        private static int i = 0;//当前位置
        public override void DoWork(PLCMsg msg)
        {
            CurrMsg = msg;
            if (CurrMsg.STATUS == 4)//机器人抓完后
            {
                var stopBefore = DeviceBase.GetDevice("E00211");
                var stopAfter = DeviceBase.GetDevice("E00212");
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

                    #region 放行机器人前挡停 
                    System.Threading.Thread.Sleep(500);//等抓走
                    ControlMsg ctl_b_Msg = new ControlMsg()
                    {
                        NO = stopBefore.NO,
                        DEVICE_TYPE = stopBefore.DEVICE_TYPE,
                        COMMAND_ID = 2,
                        MESSAGE_TYPE = "control",
                        time_stamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                    };
                    GlobalData.MQ.SentMessage(JsonConvert.SerializeObject(ctl_b_Msg));
                    #endregion

                    #region 放行机器人后挡停
                    if (i > 0 && i % flags.Count == 0)//判断放完6个
                    {
                        System.Threading.Thread.Sleep(1000);//等放下
                        ControlMsg ctl_af_Msg = new ControlMsg()
                        {
                            NO = stopAfter.NO,
                            DEVICE_TYPE = stopAfter.DEVICE_TYPE,
                            COMMAND_ID = 2,
                            MESSAGE_TYPE = "control",
                            time_stamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                        };
                        GlobalData.MQ.SentMessage(JsonConvert.SerializeObject(ctl_af_Msg));
                    }
                    #endregion
                }
            }
        }
    }
}
