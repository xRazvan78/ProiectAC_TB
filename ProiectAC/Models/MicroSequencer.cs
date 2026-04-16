using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProiectAC.Models
{
    public class MicroSequencer
    {
        public int GetNextAddress(Microinstruction currentMir, FlagsRegister flags, ushort irValue)
        {
            switch (currentMir.Successor.ToUpper())
            {
                case "STEP":
                    return currentMir.Address + 1;

                case "JUMPI":
                    return currentMir.JumpAddress;

                case "IF C":
                    return flags.C ? currentMir.JumpAddress : currentMir.Address + 1;

                case "IF Z":
                    return flags.Z ? currentMir.JumpAddress : currentMir.Address + 1;

                default:
                    return currentMir.Address + 1;
            }
        }
    }
}
