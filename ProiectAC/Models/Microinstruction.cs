using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProiectAC.Models
{
    public class Microinstruction
    {
        // Câmpurile scrise de colegul tău (Studentul A)
        public int Address { get; set; }
        public string SbusSource { get; set; }
        public string DbusSource { get; set; }
        public string AluOp { get; set; }
        public string RbusDest { get; set; }
        public string MemOp { get; set; }
        public string OtherOps { get; set; }
        public string Successor { get; set; }
        public string IndexSelect { get; set; }
        public int JumpAddress { get; set; }

        // Câmpurile adăugate pentru tine (Studentul B / Parser)
        public string Label { get; set; }
        public bool TrueFalse { get; set; }
        public string JumpAddressText { get; set; }
        public string BinaryCode { get; set; }
    }
}