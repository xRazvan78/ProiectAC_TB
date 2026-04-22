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
        // General Purpose Registers (R0 - R15)
        public Register[] R { get; private set; }

        // Special Registers
        public Register PC { get; private set; }
        public Register SP { get; private set; }
        public Register ADR { get; private set; }
        public Register MDR { get; private set; }
        public Register IR { get; private set; }
        public Register IVR { get; private set; }
        public Register T { get; private set; }

        // Flags Register
        public FlagsRegister Flags { get; private set; }

        // Internal Buses
        public Bus SBUS { get; private set; }
        public Bus DBUS { get; private set; }
        public Bus RBUS { get; private set; }

        // Functional Units
        public ALU Alu { get; private set; }
        public Memory Ram { get; private set; }

        // Sequencer
        public MicroSequencer Seq { get; private set; }
        public Microinstruction[] MPM { get; private set; } // Memory for Microprograms
        public int CurrentMicroAddress { get; set; }

        public Cpu()
        {
            // Initialize the 16 General Registers
            R = new Register[16];
            for (int i = 0; i < 16; i++)
            {
                R[i] = new Register($"R{i}");
            }

            // Initialize Special Registers
            PC = new Register("PC");
            SP = new Register("SP");
            ADR = new Register("ADR");
            MDR = new Register("MDR");
            IR = new Register("IR");
            IVR = new Register("IVR");
            T = new Register("T");

            Flags = new FlagsRegister();

            // Initialize Buses
            SBUS = new Bus("SBUS");
            DBUS = new Bus("DBUS");
            RBUS = new Bus("RBUS");

            // Initialize Memory and ALU
            Alu = new ALU();
            Ram = new Memory();

            Seq = new MicroSequencer();
            MPM = new Microinstruction[512]; // Adjust size based on your CSV rows
            CurrentMicroAddress = 0;
        }

        public void Reset()
        {
            // Clear all general registers
            for (int i = 0; i < 16; i++)
            {
                R[i].Value = 0;
            }

            // Clear special registers
            PC.Value = 0;
            SP.Value = 0;
            ADR.Value = 0;
            MDR.Value = 0;
            IR.Value = 0;
            IVR.Value = 0;
            T.Value = 0;

            // Clear buses
            SBUS.Value = 0;
            DBUS.Value = 0;
            RBUS.Value = 0;

            // Reset flags and memory
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
            Microinstruction mir = MPM[CurrentMicroAddress];

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
            if (string.IsNullOrEmpty(code) || code.Contains("NONE") || code == "0000") return 0;

            string cleanCode = code.Split(':')[0].Trim().ToUpper();

            switch (cleanCode)
            {
                case "PDPC":
                case "PDPCS":
                case "PDPCD": return PC.Value;

                case "PDSP":
                case "PDSPS": return SP.Value;

                case "PDMDR":
                case "PDMDRS":
                case "PDMDRD": return MDR.Value;

                case "PDIVR":
                case "PDIVRS": return IVR.Value;

                case "PDT":
                case "PDTS": return T.Value;

                case "PDADRS": return ADR.Value;

                case "PDIR": return IR.Value;

                case "PDRGS":
                    return R[GetSourceRegisterIndexFromIR()].Value;
                case "PDRGD":
                    return R[GetDestinationRegisterIndexFromIR()].Value;

                case "PDFLAGS":
                    ushort flagsValue = 0;
                    if (Flags.Z) flagsValue |= 0x0001;
                    if (Flags.N) flagsValue |= 0x0002;
                    if (Flags.C) flagsValue |= 0x0004;
                    if (Flags.V) flagsValue |= 0x0008;
                    return flagsValue;

                case "PD0S":
                case "PD0D": return 0;
                case "PD-1S": return 0xFFFF;

                case "PDTSNEG": return (ushort)~T.Value;
                case "PDMDRDNEG": return (ushort)~MDR.Value;

                case "PDIR [7…0]D":
                    return (ushort)(IR.Value & 0x00FF);

                case "DBUS":
                    return DBUS.Value;

                default: return 0;
            }
        }

        private void SetRegisterValueByCode(string code, ushort value)
        {
            if (string.IsNullOrEmpty(code) || code.Contains("NONE") || code == "00") return;

            string cleanCode = code.Split(':')[0].Trim().ToUpper();

            switch (cleanCode)
            {
                case "PMADR": ADR.Value = value; break;
                case "PMMDR": MDR.Value = value; break;
                case "PMPC": PC.Value = value; break;
                case "PMSP": SP.Value = value; break;
                case "PMIR": IR.Value = value; break;
                case "PMT": T.Value = value; break;

                case "PMRG":
                    int regIndex = GetDestinationRegisterIndexFromIR();
                    R[regIndex].Value = value;
                    break;

                case "PMFLAG":
                    Flags.Z = (value & 0x0001) != 0;
                    Flags.N = (value & 0x0002) != 0;
                    Flags.C = (value & 0x0004) != 0;
                    Flags.V = (value & 0x0008) != 0;
                    break;
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
                    IR.Value = Ram.Read(PC.Value);
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
                // Afișăm în format Zecimal, dar și Hexazecimal (X4 înseamnă 4 caractere hex, ex: 00FF)
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
