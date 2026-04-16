using System;
using System.Collections.Generic;
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

        public void ExecuteClockCycle()
        {
            Microinstruction mir = MPM[CurrentMicroAddress];

            SBUS.Value = GetRegisterValueByCode(mir.SbusSource);
            DBUS.Value = GetRegisterValueByCode(mir.DbusSource);

            RBUS.Value = Alu.Execute(mir.AluOp, SBUS.Value, DBUS.Value, Flags);

            SetRegisterValueByCode(mir.RbusDest, RBUS.Value);
            HandleMemoryOperation(mir.MemOp);

            CurrentMicroAddress = Seq.GetNextAddress(mir, Flags, IR.Value);
        }

        private ushort GetRegisterValueByCode(string code)
        {
            if (string.IsNullOrEmpty(code) || code.Contains("NONE")) return 0;

            string cleanCode = code.Split(':')[0].Trim().ToUpper();

            switch (cleanCode)
            {
                case "PDPC":
                case "PDPCS": return PC.Value;
                case "PDSP":
                case "PDSPS": return SP.Value;
                case "PDMDR":
                case "PDMDRS": return MDR.Value;
                case "PDIVR":
                case "PDIVRS": return IVR.Value;
                case "PDT":
                case "PDTS": return T.Value;
                case "PDIR": return IR.Value;
                case "PDRGS":
                    int regIndex = (IR.Value >> 8) & 0xF;
                    return R[regIndex].Value;

                default: return 0;
            }
        }

        private void SetRegisterValueByCode(string code, ushort value)
        {
            if (string.IsNullOrEmpty(code) || code.Contains("NONE")) return;

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
                    int regIndex = (IR.Value >> 4) & 0xF;
                    R[regIndex].Value = value;
                    break;

                case "PMFLAG":
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
    }
}
