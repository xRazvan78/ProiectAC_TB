using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProiectAC.Compiler
{
    public class Assembler
    {
        private readonly Dictionary<string, ushort> _classAOpcodes = new Dictionary<string, ushort>
        {
            { "MOV", 0x0 },
            { "ADD", 0x1 },
            { "SUB", 0x2 },
            { "CMP", 0x3 },
            { "AND", 0x4 },
            { "OR",  0x5 },
            { "XOR", 0x6 }
        };

        private readonly Dictionary<string, ushort> _classBOpcodes = new Dictionary<string, ushort>
        {
            { "CLR", 0x0 }, { "NEG", 0x1 }, { "INC", 0x2 }, { "DEC", 0x3 },
            { "ASL", 0x4 }, { "ASR", 0x5 }, { "LSR", 0x6 }, { "ROL", 0x7 },
            { "ROR", 0x8 }, { "RLC", 0x9 }, { "RRC", 0xA }, { "PUSH", 0xB }, { "POP", 0xC }
        };

        private readonly Dictionary<string, ushort> _branchOpcodes = new Dictionary<string, ushort>
        {
            { "BEQ", 0x0 }, { "BNE", 0x1 }, { "BMI", 0x2 }, { "BPL", 0x3 },
            { "BCS", 0x4 }, { "BCC", 0x5 }, { "BVS", 0x6 }, { "BVC", 0x7 }
        };

        public ushort AssembleLine(string instructionLine)
        {
            string cleanLine = instructionLine.Replace(",", " ").Trim().ToUpper();
            string[] parts = cleanLine.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length == 0) return 0;

            string mnemonic = parts[0];

            // Verificăm Clasa A (2 operanzi, ex: ADD R1 R2)
            if (_classAOpcodes.ContainsKey(mnemonic) && parts.Length >= 3)
            {
                return AssembleClassA(mnemonic, parts[1], parts[2]);
            }

            // Verificăm Clasa B (1 operand, ex: INC R5)
            if (_classBOpcodes.ContainsKey(mnemonic) && parts.Length >= 2)
            {
                return AssembleClassB(mnemonic, parts[1]);
            }

            if (_branchOpcodes.ContainsKey(mnemonic) && parts.Length >= 2)
            {
                return AssembleBranch(mnemonic, parts[1]);
            }

            if (mnemonic == "HALT") return 0xF003;
            if (mnemonic == "NOP") return 0xF002;
            if (mnemonic == "CLC") return 0xF000;
            if (mnemonic == "SEC") return 0xF001;

            throw new Exception($"Instrucțiune necunoscută sau format invalid: {instructionLine}");
        }

        private ushort AssembleClassA(string mnemonic, string source, string destination)
        {
            ushort opcode = _classAOpcodes[mnemonic];
            ushort srcReg = ParseRegister(source);
            ushort dstReg = ParseRegister(destination);

            ushort machineCode = 0;

            machineCode |= (ushort)(opcode << 12);

            machineCode |= (ushort)(srcReg << 6);

            machineCode |= dstReg;

            return machineCode;
        }

        private ushort ParseRegister(string regString)
        {
            if (regString.StartsWith("R") && ushort.TryParse(regString.Substring(1), out ushort regIndex))
            {
                if (regIndex >= 0 && regIndex <= 15)
                {
                    return regIndex;
                }
            }
            throw new Exception($"Registru invalid: {regString}. Așteptat R0-R15.");
        }

        private ushort AssembleClassB(string mnemonic, string destination)
        {
            ushort mainOpcode = 0x8;
            ushort subOpcode = _classBOpcodes[mnemonic];
            ushort dstReg = ParseRegister(destination);

            ushort machineCode = 0;
            machineCode |= (ushort)(mainOpcode << 12);
            machineCode |= (ushort)(subOpcode << 6);
            machineCode |= dstReg;

            return machineCode;
        }
        private ushort AssembleBranch(string mnemonic, string offsetString)
        {
            ushort mainOpcode = 0xC;
            ushort condition = _branchOpcodes[mnemonic];


            if (!short.TryParse(offsetString, out short offset))
            {
                throw new Exception($"Offset invalid pentru salt: {offsetString}");
            }

            ushort machineCode = 0;
            machineCode |= (ushort)(mainOpcode << 12);
            machineCode |= (ushort)(condition << 8);

            machineCode |= (ushort)(offset & 0x00FF);

            return machineCode;
        }
    }
}
