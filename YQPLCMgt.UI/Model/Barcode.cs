using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YQPLCMgt.UI
{
    public class Barcode
    {
        private string code;
        private int x;
        private int y;

        public int Y { get => y; set => y = value; }
        public int X { get => x; set => x = value; }
        public string Code { get => code; set => code = value; }
    }
}
