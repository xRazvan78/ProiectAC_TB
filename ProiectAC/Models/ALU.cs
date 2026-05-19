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

                case "INC": 
                    result = sbusValue + dbusValue + 1;
                    break;

                case "SUB":
                case "CMP":
                    result = sbusValue - dbusValue;
                    flags.C = sbusValue < dbusValue;
                    flags.V = ((sbusValue ^ dbusValue) & (sbusValue ^ result) & 0x8000) != 0; 
                    break;

                case "DEC":
                    result = sbusValue + dbusValue - 1;
                    break;

                case "NEG":
                    result = (~sbusValue) + 1;
                    flags.C = (sbusValue == 0);
                    break;

                case "OR":
                    result = sbusValue | dbusValue;
                    flags.C = false;
                    flags.V = false;
                    break;

                case "XOR":
                    result = sbusValue ^ dbusValue;
                    flags.C = false;
                    flags.V = false;
                    break;

                case "CLR":
                    result = 0;
                    break;

                case "ASL": 
                    flags.C = (sbusValue & 0x8000) != 0; 
                    result = sbusValue << 1;
                    break;

                case "ASR": 
                    flags.C = (sbusValue & 0x0001) != 0;
                    result = (sbusValue >> 1) | (sbusValue & 0x8000);
                    break;

                case "LSR": 
                    flags.C = (sbusValue & 0x0001) != 0;
                    result = sbusValue >> 1;
                    break;

                case "ROL":
                    flags.C = (sbusValue & 0x8000) != 0;
                    result = (sbusValue << 1) | (sbusValue >> 15);
                    break;

                case "ROR": 
                    flags.C = (sbusValue & 0x0001) != 0;
                    result = (sbusValue >> 1) | (sbusValue << 15);
                    break;

                default:
                    result = 0;
                    break;
            }

            ushort finalResult = (ushort)(result & 0xFFFF);

            string op = operation.ToUpper();

            if (op == "SUM" || op == "SUB" || op == "CMP" || op == "INC" || op == "DEC" || op == "NEG")
            {
                flags.Z = (finalResult == 0);
                flags.N = ((finalResult & 0x8000) != 0);
            }
            else if (op == "AND" || op == "OR" || op == "XOR" || op == "CLR")
            {
                flags.Z = (finalResult == 0);
                flags.N = ((finalResult & 0x8000) != 0);
                flags.C = false;
                flags.V = false;
            }
            else if (op == "ASL" || op == "ASR" || op == "LSR" || op == "ROL" || op == "ROR")
            {
                flags.Z = (finalResult == 0);
                flags.N = ((finalResult & 0x8000) != 0);
                flags.V = false;
            }
            
            return finalResult;
        }
    }
}
