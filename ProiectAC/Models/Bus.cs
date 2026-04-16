using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProiectAC.Models
{
    public class Bus
    {
        public string Name { get; }
        public ushort Value { get; set; }

        public Bus(string name)
        {
            Name = name;
            Value = 0;
        }
    }
}
