using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YQPLCMgt.Helper
{
    public class PLCResponse
    {
        public PLCResponse()
        {

        }

        public string Text { get; set; } = "";
        public bool HasError { get; set; } = true;
        public string ErrorMsg { get; set; } = "未处理";
    }
}
