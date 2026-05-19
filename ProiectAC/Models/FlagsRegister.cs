using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProiectAC.Models
{
    public class FlagsRegister
    {
        public bool Z { get; set; } 
        public bool N { get; set; }
        public bool C { get; set; }
        public bool V { get; set; }

        public void Reset()
        {
            Z = N = C = V = false;
        }
    }
}
