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
    /// 复校专机2
    /// </summary>
    public class E02202 : DeviceBase
    {
        public override void DoWork(PLCMsg msg)
        {
            CurrMsg = msg;
            AskPallet_Fujiao();
            //判断专机满表启动插针
            if (CurrMsg.PALLET_COUNT == 8
                && !(CurrMsg.STATUS == 1 || CurrMsg.STATUS == 2 || CurrMsg.STATUS == 3))
            {
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
            if (CurrMsg.STATUS == 1)//判断插针就绪
            {
                var d1 = DeviceBase.GetDevice("E02201");
                if (d1.CurrMsg?.STATUS == 2)
                {
                    return;
                }
                var d3 = DeviceBase.GetDevice("E02203");
                if (d3.CurrMsg?.STATUS == 2)
                {
                    return;
                }
                var d4 = DeviceBase.GetDevice("E02204");
                if (d4.CurrMsg?.STATUS == 2)
                {
                    return;
                }
                //放行后等10秒再次放行
                if (LAST_PASS_TIME.HasValue &&  (DateTime.Now - LAST_PASS_TIME.Value).Seconds < 10)
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
                Task.Run(() =>
                {
                    //System.Threading.Thread.Sleep(5000);//5s后放行
                    GlobalData.MQ.SentMessage(JsonConvert.SerializeObject(ctlMsg));//放行专机
                });
            }
            else
            {

            }
        }
    }
}
