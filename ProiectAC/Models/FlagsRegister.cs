using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProiectAC.Models
{
    public class FlagsRegister
    {
        public bool Z { get; set; } // Zero
        public bool N { get; set; } // Negative
        public bool C { get; set; } // Carry
        public bool V { get; set; } // Overflow

        public void Reset()
        {
            Z = N = C = V = false;
        }
    }
}
