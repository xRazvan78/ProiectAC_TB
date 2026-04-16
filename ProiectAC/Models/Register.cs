using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProiectAC.Models
{
    public class Register
    {
        public string Name { get; }
        public ushort Value { get; set; }

        public Register(string name)
        {
            Name = name;
            Value = 0;
        }
    }
}
