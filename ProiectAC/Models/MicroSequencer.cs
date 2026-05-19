using System;

namespace ProiectAC.Models
{
    public class MicroSequencer
    {
        public int GetNextAddress(Microinstruction currentMir, FlagsRegister flags, ushort irValue)
        {
            if (currentMir == null) return 0;

            string successor = currentMir.Successor?.Trim().ToUpper() ?? "STEP";

            if (currentMir.Address == 0) return 1;

            if (currentMir.Address == 1)
            {
                int mainOpcode = (irValue >> 12) & 0x000F;
                int executeAddress = GetMacroRoutineAddress(mainOpcode, irValue);
                if (executeAddress != 0) return executeAddress;
            }

            if (currentMir.Address >= 30 && currentMir.Address <= 80 && (successor == "STEP" || successor == "NONE"))
            {
                return 0;
            }

            if (successor.Contains("ACLOW")) return currentMir.Address + 1;

            switch (successor)
            {
                case "STEP":
                case "NONE":
                    return currentMir.Address + 1;

                case "JUMP":
                case "JUMPI":
                    return currentMir.JumpAddress;

                case "IF C": return flags.C ? currentMir.JumpAddress : currentMir.Address + 1;
                case "IF Z": return flags.Z ? currentMir.JumpAddress : currentMir.Address + 1;
                case "IF N":
                case "IF S": return flags.N ? currentMir.JumpAddress : currentMir.Address + 1;
                case "IF V": return flags.V ? currentMir.JumpAddress : currentMir.Address + 1;

                case "INDEX":
                    int mOpcode = (irValue >> 12) & 0x000F;
                    return GetMacroRoutineAddress(mOpcode, irValue);

                default:
                    return currentMir.Address + 1;
            }
        }

        private int GetMacroRoutineAddress(int mainOpcode, ushort irValue)
        {
            switch (mainOpcode)
            {
                case 0x0: return 30; 
                case 0x1: return 31; 
                case 0x2: return 32; 
                case 0x3: return 33; 
                case 0x4: return 34; 
                case 0x5: return 35; 
                case 0x6: return 36; 
                case 0x7: return 0;

                case 0x8:
                    int subOpcodeB = (irValue >> 6) & 0x000F;
                    switch (subOpcodeB)
                    {
                        case 0x0: return 38; 
                        case 0x1: return 40; 
                        case 0x2: return 39; 
                        case 0x3: return 41; 
                        case 0x4: return 42; 
                        case 0x5: return 44; 
                        case 0x6: return 46; 
                        case 0x7: return 48; 
                        case 0x8: return 50; 
                        case 0x9: return 52; 
                        case 0xA: return 54; 
                        case 0xB: return 80; 
                        case 0xC: return 84; 
                        default: return 0;
                    }

                case 0xC:
                    int branchOpcode = (irValue >> 8) & 0x000F;
                    switch (branchOpcode)
                    {
                        case 0x0: return 56; 
                        case 0x1: return 58; 
                        case 0x2: return 60; 
                        case 0x3: return 62; 
                        case 0x4: return 64; 
                        case 0x5: return 66; 
                        case 0x6: return 68; 
                        case 0x7: return 70; 
                        default: return 0;
                    }

                case 0xD: return 72; 
                case 0xE: return 74; 

                case 0xF:
                    int subOpcodeF = irValue & 0x000F;
                    switch (subOpcodeF)
                    {
                        case 0x0: return 90;  
                        case 0x1: return 92;  
                        case 0x2: return 94;  
                        case 0x3: return 96;  
                        default: return 0;
                    }

                default:
                    return 0;
            }
        }
    }
}