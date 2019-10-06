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
    /// 1号机器人-放上壳
    /// </summary>
    public class E01301:DeviceBase
    {
        public override void DoWork(PLCMsg msg)
        {           
            CurrMsg = msg;
            if (CurrMsg.STATUS ==1)//TODO:判断专机启用
            {
                //放行后等5秒再次放行
                if (LAST_PASS_TIME.HasValue && (DateTime.Now - LAST_PASS_TIME.Value).Seconds < 5)
                {
                    return;
                }
                LAST_PASS_TIME = DateTime.Now;
                ControlMsg ctlMsg = new ControlMsg()
                {
                    DEVICE_TYPE = msg.DEVICE_TYPE,
                    NO = msg.NO,
#if PASS
                    COMMAND_ID = 2,//直接放行
#else 
                    COMMAND_ID = 3,
#endif
                    MESSAGE_TYPE = "control",
                    time_stamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                };
                GlobalData.MQ.SentMessage(JsonConvert.SerializeObject(ctlMsg));//启动专机
            }
            else if (CurrMsg.STATUS == 4)//完成
            {
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
