using System;

namespace ProiectAC.Models
{
    public class MicroSequencer
    {
        public int GetNextAddress(Microinstruction currentMir, FlagsRegister flags, ushort irValue)
        {
            if (currentMir == null) return 0; // Protecție pentru rânduri goale

            string successor = currentMir.Successor?.Trim().ToUpper() ?? "STEP";

            // --- REZOLVAREA BUCLEI INFINITE ---
            // 1. După faza de IFCH (adresa 0), trecem mereu la adresa 1
            if (currentMir.Address == 0) return 1;

            // 2. La adresa 1, IGNORĂM saltul greșit din CSV și forțăm decodarea instrucțiunii din IR!
            if (currentMir.Address == 1)
            {
                int mainOpcode = (irValue >> 12) & 0x000F;
                int executeAddress = GetMacroRoutineAddress(mainOpcode, irValue);
                if (executeAddress != 0) return executeAddress;
            }

            // 3. La finalul unei execuții (ex: 30, 31..), forțăm întoarcerea la IFCH (0) 
            // pentru a aduce următoarea instrucțiune, prevenind execuția "din inerție" a rândurilor de dedesubt.
            if (currentMir.Address >= 30 && currentMir.Address <= 80 && (successor == "STEP" || successor == "NONE"))
            {
                return 0;
            }
            // ----------------------------------

            // Logica normală dictată de CSV pentru restul pașilor
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
                case 0x0: return 30; // MOV
                case 0x1: return 31; // ADD
                case 0x2: return 32; // SUB
                case 0x3: return 33; // CMP
                case 0x4: return 34; // AND
                case 0x5: return 35; // OR
                case 0x6: return 36; // XOR
                case 0x7: return 0;

                case 0x8:
                    int subOpcodeB = (irValue >> 6) & 0x000F;
                    switch (subOpcodeB)
                    {
                        case 0x0: return 38; // CLR
                        case 0x1: return 40; // NEG
                        case 0x2: return 39; // INC
                        case 0x3: return 41; // DEC
                        case 0x4: return 42; // ASL
                        case 0x5: return 44; // ASR
                        case 0x6: return 46; // LSR
                        case 0x7: return 48; // ROL
                        case 0x8: return 50; // ROR
                        case 0x9: return 52; // RLC
                        case 0xA: return 54; // RRC
                        case 0xB: return 80; // PUSH
                        case 0xC: return 84; // POP
                        default: return 0;
                    }

                case 0xC:
                    int branchOpcode = (irValue >> 8) & 0x000F;
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

                case 0xD: return 72; // CALL
                case 0xE: return 74; // RET

                case 0xF:
                    int subOpcodeF = irValue & 0x000F;
                    switch (subOpcodeF)
                    {
                        case 0x0: return 90;  // CLC
                        case 0x1: return 92;  // SEC
                        case 0x2: return 94;  // NOP
                        case 0x3: return 96;  // HALT
                        default: return 0;
                    }

                default:
                    return 0;
            }
        }
    }
}