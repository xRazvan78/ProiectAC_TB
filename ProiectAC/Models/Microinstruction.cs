using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProiectAC.Models
{
    public class Microinstruction
    {
        public int Address { get; set; }
        public string SbusSource { get; set; }
        public string DbusSource { get; set; }
        public string AluOp { get; set; }
        public string RbusDest { get; set; }
        public string MemOp { get; set; }
        public string OtherOps { get; set; }

        //SEQ Fields
        public string Successor { get; set; }
        public string IndexSelect { get; set; }
        public int JumpAddress { get; set; }
    }
}
