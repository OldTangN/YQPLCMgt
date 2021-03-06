﻿using Newtonsoft.Json;
using RabbitMQ.YQMsg;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YQServer.Device
{
    public class E01401 : DeviceBase
    {
        /// <summary>
        /// 刻录专机1 -- 上壳码
        /// </summary>
        /// <param name="msg"></param>
        public override void DoWork(PLCMsg msg)
        {
            //if (CurrMsg != null && (Convert.ToDateTime(msg.time_stamp) - Convert.ToDateTime(CurrMsg.time_stamp)).Seconds < 3)
            //    return;
            CurrMsg = msg;
            if (CurrMsg.STATUS == 1)//TODO:判断专机启用
            {
                //放行后等5秒再次放行
                //if (LAST_PASS_TIME.HasValue &&  (DateTime.Now - LAST_PASS_TIME.Value).Seconds < 3)
                //{
                //    return;
                //}
                //LAST_PASS_TIME = DateTime.Now;
                ControlMsg ctlMsg = new ControlMsg()
                {
                    DEVICE_TYPE = msg.DEVICE_TYPE,
                    NO = msg.NO,
                    COMMAND_ID = 3,
                    MESSAGE_TYPE = "control",
                    time_stamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                };
                GlobalData.MQ.SentMessage(JsonConvert.SerializeObject(ctlMsg));//启动专机
            }
            else if (CurrMsg.STATUS == 4)//完成
            {
                if (LAST_PASS_TIME.HasValue && (DateTime.Now - LAST_PASS_TIME.Value).Seconds < 3)
                {
                    return;
                }
                LAST_PASS_TIME = DateTime.Now;
                ControlMsg ctlMsg = new ControlMsg()
                {
                    DEVICE_TYPE = msg.DEVICE_TYPE,
                    NO = msg.NO,
                    COMMAND_ID = 2,
                    MESSAGE_TYPE = "control",
                    time_stamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                };
                GlobalData.MQ.SentMessage(JsonConvert.SerializeObject(ctlMsg));//放行专机
            }
            else
            {

            }
        }
    }
}
