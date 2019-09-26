using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YQPLCMgt.UI
{
    public class TestMsg
    {
        /// <summary>
        /// 设备类型
        /// </summary>
        public string DEVICE_TYPE { get; set; }

        /// <summary>
        /// 设备编号
        /// </summary>
        public string NO { get; set; }

        /// <summary>
        /// 消息类型
        /// </summary>
        public string MESSAGE_TYPE { get; set; }

        /// <summary>
        /// 时间戳
        /// </summary>
        public string time_stamp { get; set; } = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        public List<Data> Data { get; set; } = new List<Data>();
    }

    public class Data
    {
        /// <summary>
        /// 工位号1、2、3、4、5、6......
        /// </summary>
        public string WORK_ID { get; set; }
        public string Bar_code { get; set; }
        /// <summary>
        /// 之前的结果
        /// <para>0=合格，1=不合格</para>
        /// </summary>
        public string result { get; set; }
    }
}
