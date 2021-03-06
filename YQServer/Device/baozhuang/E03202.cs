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
    /// 不良品夹抱分流
    /// </summary>
    public class E03202 : DeviceBase
    {
        public override void DoWork(PLCMsg msg)
        {
            CurrMsg = msg;
            if (CurrMsg.STATUS == 1)//
            {
                ControlMsg ctlMsg = new ControlMsg()
                {
                    DEVICE_TYPE = msg.DEVICE_TYPE,
                    NO = msg.NO,
                    COMMAND_ID = 2,
                    MESSAGE_TYPE = "control",
                    time_stamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                };
                GlobalData.MQ.SentMessage(JsonConvert.SerializeObject(ctlMsg));//启动专机
            }
            else
            {

            }
        }
    }
}
