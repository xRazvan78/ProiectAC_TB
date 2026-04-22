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

                case "IF S":
                    return flags.N ? currentMir.JumpAddress : currentMir.Address + 1;

                case "IF V":
                    return flags.V ? currentMir.JumpAddress : currentMir.Address + 1;

                case "INDEX":
                    int mainOpcode = (irValue >> 12) & 0x000F;
                    return GetMacroRoutineAddress(mainOpcode, irValue);

                default:
                    return currentMir.Address + 1;
            }
        }

        private int GetMacroRoutineAddress(int mainOpcode, ushort irValue)
        {
            switch (mainOpcode)
            {
                case 0x0: return 30; // MOV
                case 0x1: return 31; // ADD
                case 0x2: return 32; // SUB
                case 0x3: return 33; // CMP
                case 0x4: return 34; // AND
                case 0x5: return 35; // OR
                case 0x6: return 36; // XOR

                case 0x8:
                    int subOpcodeB = irValue & 0x000F;
                    switch (subOpcodeB)
                    {
                        case 0x0: return 37; // CLR
                        case 0x1: return 38; // NEG
                        case 0x2: return 39; // INC
                        case 0x3: return 40; // DEC
                        case 0x4: return 41; // ASL
                        case 0x5: return 42; // ASR
                        case 0x6: return 43; // LSR
                        case 0x7: return 44; // ROL
                        case 0x8: return 45; // ROR
                        case 0x9: return 46; // RLC
                        case 0xA: return 47; // RRC
                        case 0xB: return 48; // PUSH
                        case 0xC: return 49; // POP
                        default: return 0;   // IFCH fallback
                    }

                case 0xC:
                    int branchOpcode = irValue & 0x000F;
                    switch (branchOpcode)
                    {
                        case 0x0: return 56; // BEQ
                        case 0x1: return 58; // BNE
                        case 0x2: return 60; // BMI
                        case 0x3: return 62; // BPL
                        case 0x4: return 64; // BCS
                        case 0x5: return 66; // BCC
                        case 0x6: return 68; // BVS
                        case 0x7: return 70; // BVC
                        default: return 0;
                    }

                case 0xD: return 72;
                case 0xE: return 74;

                case 0xF:
                    int subOpcodeF = irValue & 0x000F;
                    switch (subOpcodeF)
                    {
                        case 0x0: return 90;  // CLC
                        case 0x1: return 92;  // SEC
                        case 0x2: return 94;  // NOP
                        case 0x3: return 96;  // HALT
                        case 0x4: return 98;  // EI
                        case 0x5: return 100; // DI
                        case 0x6: return 110; // RET
                        case 0x7: return 112; // IRET
                        default: return 0;
                    }

                default:
                    return 0;
            }
        }
    }
    
}
