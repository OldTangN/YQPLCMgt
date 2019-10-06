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
    /// 复校检测前扫码挡停
    /// </summary>
    public class E00215 : DeviceBase
    {
        private static DateTime LastCount8 = DateTime.Now;
        private static int count = 0;
        public override void DoWork(PLCMsg msg)
        {
            CurrMsg = msg;
            if (CurrMsg.STATUS == 1)//判断挡停放行
            {

                int cmdid = 0;
                //判断挡停后边专机空闲
                var nxtDevice1 = DeviceBase.GetDevice("E02201");
                var nxtDevice2 = DeviceBase.GetDevice("E02202");
                var nxtDevice3 = DeviceBase.GetDevice("E02203");
                var nxtDevice4 = DeviceBase.GetDevice("E02204");
                if (nxtDevice1.CurrMsg?.PALLET_COUNT < 8)
                {
                    cmdid = 2;
                }
                else if (nxtDevice2.CurrMsg?.STATUS < 8)
                {
                    cmdid = 3;
                }
                else if (nxtDevice3.CurrMsg?.STATUS < 8)
                {
                    cmdid = 4;
                }
                else if (nxtDevice4.CurrMsg?.STATUS < 8)
                {
                    cmdid = 5;
                }
                else
                {

                }
                if (cmdid != 0 && (DateTime.Now - LastCount8).Seconds > 20)//每放8个等20秒
                {
                    count++;
                    if (count % 8 == 0)
                        LastCount8 = DateTime.Now;
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
