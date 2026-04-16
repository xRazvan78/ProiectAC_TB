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
    }
}
