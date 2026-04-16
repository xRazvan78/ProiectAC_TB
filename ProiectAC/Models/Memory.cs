using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProiectAC.Models
{
    public class Memory
    {

        private readonly ushort[] _data = new ushort[65536];

        public ushort Read(ushort address)
        {
            return _data[address];
        }

        public void Write(ushort address, ushort value)
        {
            _data[address] = value;
        }

        public void Clear()
        {
            for (int i = 0; i < _data.Length; i++)
            {
                _data[i] = 0;
            }
        }
    }
}
