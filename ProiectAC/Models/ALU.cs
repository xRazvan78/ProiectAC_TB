using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProiectAC.Models
{
    public class ALU
    {
        public ushort Execute(string operation, ushort sbusValue, ushort dbusValue, FlagsRegister flags)
        {
            int result = 0;

            switch (operation.ToUpper())
            {
                case "NONE":
                    result = 0;
                    break;

                case "SBUS":
                    result = sbusValue;
                    break;

                case "DBUS":
                    result = dbusValue;
                    break;

                case "SUM":
                    result = sbusValue + dbusValue;

                    flags.C = result > 0xFFFF;

                    flags.V = (~(sbusValue ^ dbusValue) & (sbusValue ^ result) & 0x8000) != 0;
                    break;

                case "AND":
                    result = sbusValue & dbusValue;
                    flags.C = false;
                    flags.V = false;
                    break;

                // TODO: Aici vei mai adăuga tu operații pe măsură ce avansăm (ex: SUB, OR, XOR, etc.)
                // Te vei uita în coloana "Operatie ALU" din fisierul tau Microprogram.csv

                default:
                    result = 0;
                    break;
            }

            ushort finalResult = (ushort)(result & 0xFFFF);

            flags.Z = (finalResult == 0);
            flags.N = ((finalResult & 0x8000) != 0);

            return finalResult;
        }
    }
}
