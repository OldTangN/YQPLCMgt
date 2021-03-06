﻿using Newtonsoft.Json;
using RabbitMQ.YQMsg;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YQServer.Device
{
    /// <summary>
    /// 铅封+铅封刻录前扫码挡停
    /// </summary>
    public class E00221 : DeviceBase
    {
        public override void DoWork(PLCMsg msg)
        {
            CurrMsg = msg;
            if (CurrMsg.STATUS == 1)//判断挡停放行
            {
                //判断挡停后边专机空闲
                var nxtDevice = DeviceBase.GetDevice("E02701");
                if (nxtDevice.CurrMsg?.STATUS == 0)
                {
                    ControlMsg ctlMsg = new ControlMsg()
                    {
                        DEVICE_TYPE = msg.DEVICE_TYPE,
                        NO = msg.NO,
                        COMMAND_ID = 2,
                        MESSAGE_TYPE = "control",
                        time_stamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                    };
                    GlobalData.MQ.SentMessage(JsonConvert.SerializeObject(ctlMsg));//放行挡停
                }
            }
            else
            {

            }
        }
    }
}
