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
    /// 初调检测前扫码挡停
    /// </summary>
    public class E00214 : DeviceBase
    {
        public override void DoWork(PLCMsg msg)
        {
            CurrMsg = msg;
            if (CurrMsg.STATUS == 1)//判断挡停放行
            {
                int cmdid = 0;
                //判断挡停后边专机空闲
                var nxtDevice1 = DeviceBase.GetDevice("E02101");
                var nxtDevice2 = DeviceBase.GetDevice("E02102");
                if (nxtDevice1.CurrMsg?.STATUS == 0)
                {
                    cmdid = 2;
                }
                else if (nxtDevice2.CurrMsg?.STATUS == 0)
                {
                    cmdid = 3;
                }
                else
                {

                }
                if (cmdid != 0)
                {
                    var ctlMsg = new ControlMsg()
                    {
                        DEVICE_TYPE = msg.DEVICE_TYPE,
                        NO = msg.NO,
                        COMMAND_ID = cmdid,
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
