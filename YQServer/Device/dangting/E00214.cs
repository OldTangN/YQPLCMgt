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
        private static DateTime LastCount8 = DateTime.Now;
        private static int count = 0;
        private static int LastNo = 1;//专机号1,2 = no%2+1
        public override void DoWork(PLCMsg msg)
        {
            CurrMsg = msg;
            return;
            if (CurrMsg.STATUS == 1)//判断挡停放行
            {
                bool canpass = true;
                if (count == 8)
                {
                    //判断托盘到位
                    string strLastNo = "E0210" + LastNo;
                    var lastDevice = DeviceBase.GetDevice(strLastNo);
                    if (lastDevice.CurrMsg?.PALLET_COUNT == 8)//8个托盘到位了
                    {
                        //发送3
                        ControlMsg ctlMsg = new ControlMsg()
                        {
                            DEVICE_TYPE = "E021",
                            NO = strLastNo,
                            COMMAND_ID = 3,
                            MESSAGE_TYPE = "control",
                            time_stamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                        };
                        GlobalData.MQ.SentMessage(JsonConvert.SerializeObject(ctlMsg));//启动专机
                        canpass = true;
                        LastNo++;//换下一个专机
                        LastNo = LastNo % 2 + 1;
                        count = 0;
                    }
                    else
                    {
                        canpass = false;
                    }
                }
                if (canpass)
                {
                    count++;
                    var ctlMsg = new ControlMsg()
                    {
                        DEVICE_TYPE = msg.DEVICE_TYPE,
                        NO = msg.NO,
                        COMMAND_ID = (LastNo + 1),//放行命令23 = no + 1
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
