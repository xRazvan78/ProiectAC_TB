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

                case "INC": // <--- ADAUGĂ ACEST BLOC!
                            // Incrementăm valoarea care vine pe magistrală (poate veni pe SBUS sau DBUS)
                    result = sbusValue + dbusValue + 1;
                    break;

                // TODO: Aici vei mai adăuga tu operații pe măsură ce avansăm (ex: SUB, OR, XOR, etc.)
                // Te vei uita în coloana "Operatie ALU" din fisierul tau Microprogram.csv
                case "SUB":
                case "CMP": // CMP face exact o scădere, dar nu salvează rezultatul (doar modifică flag-urile)
                    result = sbusValue - dbusValue;
                    flags.C = sbusValue < dbusValue; // Flag de împrumut (Borrow)
                    flags.V = ((sbusValue ^ dbusValue) & (sbusValue ^ result) & 0x8000) != 0; // Overflow
                    break;

                case "DEC":
                    result = sbusValue + dbusValue - 1;
                    break;

                case "NEG": // Complement față de 2 (Schimbarea semnului)
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

                case "CLR": // Ștergere (pune zero)
                    result = 0;
                    break;

                case "ASL": // Arithmetic Shift Left (Înmulțire cu 2)
                    flags.C = (sbusValue & 0x8000) != 0; // Ultimul bit iese în Carry
                    result = sbusValue << 1;
                    break;

                case "ASR": // Arithmetic Shift Right (Împărțire cu 2, păstrează semnul)
                    flags.C = (sbusValue & 0x0001) != 0; // Primul bit iese în Carry
                    result = (sbusValue >> 1) | (sbusValue & 0x8000);
                    break;

                case "LSR": // Logical Shift Right (Bagă zero prin stânga)
                    flags.C = (sbusValue & 0x0001) != 0;
                    result = sbusValue >> 1;
                    break;

                case "ROL": // Rotate Left (Ce iese prin stânga, intră prin dreapta)
                    flags.C = (sbusValue & 0x8000) != 0;
                    result = (sbusValue << 1) | (sbusValue >> 15);
                    break;

                case "ROR": // Rotate Right (Ce iese prin dreapta, intră prin stânga)
                    flags.C = (sbusValue & 0x0001) != 0;
                    result = (sbusValue >> 1) | (sbusValue << 15);
                    break;

                default:
                    result = 0;
                    break;
            }

            ushort finalResult = (ushort)(result & 0xFFFF);

            string op = operation.ToUpper();

            // 1. Operații Aritmetice (Ele modifică absolut toate cele 4 flag-uri)
            if (op == "SUM" || op == "SUB" || op == "CMP" || op == "INC" || op == "DEC" || op == "NEG")
            {
                flags.Z = (finalResult == 0);
                flags.N = ((finalResult & 0x8000) != 0);
                // C și V sunt deja calculate corect în interiorul case-urilor din switch
            }
            // 2. Operații Logice (Ele modifică doar Z și N, iar C și V sunt FORȚATE la false)
            else if (op == "AND" || op == "OR" || op == "XOR" || op == "CLR")
            {
                flags.Z = (finalResult == 0);
                flags.N = ((finalResult & 0x8000) != 0);
                flags.C = false;
                flags.V = false;
            }
            // 3. Operații de Deplasare/Rotație (Ele modifică Z, N și C, iar V este resetat)
            else if (op == "ASL" || op == "ASR" || op == "LSR" || op == "ROL" || op == "ROR")
            {
                flags.Z = (finalResult == 0);
                flags.N = ((finalResult & 0x8000) != 0);
                flags.V = false;
                // C este deja calculat corect în interiorul fiecărui case de shift/rotate
            }
            // 4. Dacă este "NONE", "SBUS", "DBUS" (Faze administrative gen IFCH)
            // Toate cele 4 flag-uri își păstrează valoarea anterioară, NU fac flash și NU se modifică!

            // ---------------
            return finalResult;
        }
    }
}
