using ProiectAC.Compiler;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProiectAC.Models
{
    public class Cpu
    {
        public Register[] R { get; private set; }

        public Register PC { get; private set; }
        public Register SP { get; private set; }
        public Register ADR { get; private set; }
        public Register MDR { get; private set; }
        public Register IR { get; private set; }
        public Register IVR { get; private set; }
        public Register T { get; private set; }

        public FlagsRegister Flags { get; private set; }

        public Bus SBUS { get; private set; }
        public Bus DBUS { get; private set; }
        public Bus RBUS { get; private set; }

        public ALU Alu { get; private set; }
        public Memory Ram { get; private set; }

        public MicroSequencer Seq { get; private set; }
        public Microinstruction[] MPM { get; private set; } 
        public int CurrentMicroAddress { get; set; }

        public Cpu()
        {
            R = new Register[16];
            for (int i = 0; i < 16; i++)
            {
                R[i] = new Register($"R{i}");
            }

            PC = new Register("PC");
            SP = new Register("SP");
            ADR = new Register("ADR");
            MDR = new Register("MDR");
            IR = new Register("IR");
            IVR = new Register("IVR");
            T = new Register("T");

            Flags = new FlagsRegister();

            SBUS = new Bus("SBUS");
            DBUS = new Bus("DBUS");
            RBUS = new Bus("RBUS");

            Alu = new ALU();
            Ram = new Memory();

            Seq = new MicroSequencer();
            MPM = new Microinstruction[512];
            CurrentMicroAddress = 0;
        }

        public void Reset()
        {
            for (int i = 0; i < 16; i++)
            {
                R[i].Value = 0;
            }

            PC.Value = 0;
            SP.Value = 0;
            ADR.Value = 0;
            MDR.Value = 0;
            IR.Value = 0;
            IVR.Value = 0;
            T.Value = 0;

            SBUS.Value = 0;
            DBUS.Value = 0;
            RBUS.Value = 0;

            Flags.Reset();
            Ram.Clear();
        }

        public void LoadSingleInstruction(string instructionText)
        {
            Reset();

            Assembler asm = new Assembler();
            ushort machineCode = asm.AssembleLine(instructionText);

            Ram.Write(0, machineCode);

            PC.Value = 0;
            CurrentMicroAddress = 0;
        }

        public void ExecuteClockCycle()
        {
            if (MPM[CurrentMicroAddress] == null) return;

            Microinstruction mir = MPM[CurrentMicroAddress];

            if (CurrentMicroAddress >= 30 && CurrentMicroAddress <= 80)
            {
                int mainOpcode = (IR.Value >> 12) & 0x000F;

                if (mainOpcode < 0x7) 
                {
                    mir.SbusSource = "PDRGS";
                    mir.DbusSource = "PDRGD"; 
                    mir.RbusDest = (mainOpcode == 0x3) ? "NONE" : "PMRGS";

                    switch (mainOpcode)
                    {
                        case 0x0: mir.AluOp = "DBUS"; break;
                        case 0x1: mir.AluOp = "SUM"; break;  
                        case 0x2: mir.AluOp = "SUB"; break;
                        case 0x3: mir.AluOp = "CMP"; break;
                        case 0x4: mir.AluOp = "AND"; break;
                        case 0x5: mir.AluOp = "OR"; break;
                        case 0x6: mir.AluOp = "XOR"; break;
                    }
                }
                else if (mainOpcode == 0x8) 
                {
                    mir.SbusSource = "PDRGD";
                    mir.DbusSource = "PD0D";
                    mir.RbusDest = "PMRGD";

                    int subOpcodeB = (IR.Value >> 6) & 0x000F;
                    switch (subOpcodeB)
                    {
                        case 0x0: mir.AluOp = "CLR"; break;
                        case 0x1: mir.AluOp = "NEG"; break;
                        case 0x2: mir.AluOp = "INC"; break;
                        case 0x3: mir.AluOp = "DEC"; break;
                        case 0x4: mir.AluOp = "ASL"; break;
                        case 0x5: mir.AluOp = "ASR"; break;
                        case 0x6: mir.AluOp = "LSR"; break;
                        case 0x7: mir.AluOp = "ROL"; break;
                        case 0x8: mir.AluOp = "ROR"; break;
                    }
                }
            }

            SBUS.Value = GetRegisterValueByCode(mir.SbusSource);
            DBUS.Value = GetRegisterValueByCode(mir.DbusSource);

            RBUS.Value = Alu.Execute(mir.AluOp, SBUS.Value, DBUS.Value, Flags);

            SetRegisterValueByCode(mir.RbusDest, RBUS.Value);

            HandleMemoryOperation(mir.MemOp);
            HandleOtherOperations(mir.OtherOps);

            CurrentMicroAddress = Seq.GetNextAddress(mir, Flags, IR.Value);
        }

        private ushort GetRegisterValueByCode(string code)
        {
            if (string.IsNullOrWhiteSpace(code) || code.Contains("NONE") || code.Contains("0000")) return 0;

            string c = code.Split(':')[0].Replace(" ", "").ToUpper();

            if (c.Contains("PDPC")) return PC.Value;
            if (c.Contains("PDSP")) return SP.Value;
            if (c.Contains("PDMDR") && !c.Contains("NEG")) return MDR.Value;
            if (c.Contains("PDIVR")) return IVR.Value;
            if (c.Contains("PDT") && !c.Contains("NEG")) return T.Value;
            if (c.Contains("PDADR")) return ADR.Value;
            if (c.Contains("PDIR") && !c.Contains("[")) return IR.Value;

            if (c.Contains("PDRG"))
            {
                int regIndex = c.Contains("S") ? GetSourceRegisterIndexFromIR() : GetDestinationRegisterIndexFromIR();
                return R[regIndex].Value;
            }

            if (c.Contains("PDFLAG"))
            {
                ushort flagsValue = 0;
                if (Flags.Z) flagsValue |= 0x0001;
                if (Flags.N) flagsValue |= 0x0002;
                if (Flags.C) flagsValue |= 0x0004;
                if (Flags.V) flagsValue |= 0x0008;
                return flagsValue;
            }

            if (c == "PD0S" || c == "PD0D") return 0;
            if (c == "PD-1S") return 0xFFFF;
            if (c.Contains("PDTSNEG")) return (ushort)~T.Value;
            if (c.Contains("PDMDRDNEG")) return (ushort)~MDR.Value;
            if (c.Contains("PDIR[7…0]D")) return (ushort)(IR.Value & 0x00FF);
            if (c.Contains("DBUS")) return DBUS.Value;

            return 0;
        }

        private void SetRegisterValueByCode(string code, ushort value)
        {
            if (string.IsNullOrWhiteSpace(code) || code.Contains("NONE") || code.Contains("00")) return;

            string c = code.Split(':')[0].Replace(" ", "").ToUpper();

            if (c.Contains("PMADR")) { ADR.Value = value; return; }
            if (c.Contains("PMMDR")) { MDR.Value = value; return; }
            if (c.Contains("PMPC")) { PC.Value = value; return; }
            if (c.Contains("PMSP")) { SP.Value = value; return; }
            if (c.Contains("PMIR")) { IR.Value = value; return; }
            if (c.Contains("PMT")) { T.Value = value; return; }

            if (c.Contains("PMRG"))
            {
                int regIndex = c.Contains("S") ? GetSourceRegisterIndexFromIR() : GetDestinationRegisterIndexFromIR();
                if (regIndex >= 0 && regIndex < 16)
                {
                    R[regIndex].Value = value;
                }
                return;
            }

            if (c.Contains("PMFLAG"))
            {
                Flags.Z = (value & 0x0001) != 0;
                Flags.N = (value & 0x0002) != 0;
                Flags.C = (value & 0x0004) != 0;
                Flags.V = (value & 0x0008) != 0;
                return;
            }
        }

        private void HandleMemoryOperation(string op)
        {
            if (string.IsNullOrEmpty(op) || op.Contains("NONE")) return;

            string cleanOp = op.Split(':')[0].Trim().ToUpper();

            switch (cleanOp)
            {
                case "READ":
                    MDR.Value = Ram.Read(ADR.Value);
                    break;

                case "WRITE":
                    Ram.Write(ADR.Value, MDR.Value);
                    break;

                case "IFCH":
                    IR.Value = Ram.Read(ADR.Value);
                    break;
            }
        }

        private void HandleOtherOperations(string op)
        {
            if (string.IsNullOrEmpty(op) || op.Contains("NONE")) return;

            string cleanOp = op.Split(':')[0].Trim().ToUpper();

            switch (cleanOp)
            {
                case "+2PC": PC.Value += 2; break;
                case "+2SP": SP.Value += 2; break;
                case "-2SP": SP.Value -= 2; break;
            }
        }

        private int GetSourceRegisterIndexFromIR()
        {
            return (IR.Value >> 6) & 0x000F;
        }

        private int GetDestinationRegisterIndexFromIR()
        {
            return IR.Value & 0x000F;
        }

        public void PrintCpuState()
        {
            Debug.WriteLine("\n========== STARE CURENTĂ PROCESOR ==========");

            Debug.WriteLine("--- Registre Generale ---");
            for (int i = 0; i < 16; i++)
            {
                Debug.WriteLine($"R{i,-2}: {R[i].Value,5} (0x{R[i].Value:X4})");
            }

            Debug.WriteLine("\n--- Registre Speciale ---");
            Debug.WriteLine($"PC : {PC.Value,5} (0x{PC.Value:X4})");
            Debug.WriteLine($"SP : {SP.Value,5} (0x{SP.Value:X4})");
            Debug.WriteLine($"IR : {IR.Value,5} (0x{IR.Value:X4})");
            Debug.WriteLine($"ADR: {ADR.Value,5} (0x{ADR.Value:X4})");
            Debug.WriteLine($"MDR: {MDR.Value,5} (0x{MDR.Value:X4})");
            Debug.WriteLine($"T  : {T.Value,5} (0x{T.Value:X4})");
            Debug.WriteLine($"IVR: {IVR.Value,5} (0x{IVR.Value:X4})");

            Debug.WriteLine("\n--- Magistrale ---");
            Debug.WriteLine($"SBUS: {SBUS.Value,5} (0x{SBUS.Value:X4})");
            Debug.WriteLine($"DBUS: {DBUS.Value,5} (0x{DBUS.Value:X4})");
            Debug.WriteLine($"RBUS: {RBUS.Value,5} (0x{RBUS.Value:X4})");

            Debug.WriteLine("\n--- Flag-uri (Condiții) ---");
            Debug.WriteLine($"Z (Zero)     : {Flags.Z}");
            Debug.WriteLine($"N (Negativ)  : {Flags.N}");
            Debug.WriteLine($"C (Carry)    : {Flags.C}");
            Debug.WriteLine($"V (Overflow) : {Flags.V}");

            Debug.WriteLine("============================================\n");
        }
    }
}