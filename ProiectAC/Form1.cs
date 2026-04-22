using ProiectAC.Compiler;
using ProiectAC.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ProiectAC
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Cpu simulatorCpu = new Cpu();
            simulatorCpu.Reset();

            simulatorCpu.R[1].Value = 10;
            simulatorCpu.R[2].Value = 25;

            simulatorCpu.IR.Value = 0x0042;

            simulatorCpu.MPM[0] = new Microinstruction
            {
                Address = 0,
                SbusSource = "PDRGS",
                DbusSource = "PDRGD",
                AluOp = "SUM",  
                RbusDest = "PMRG",
                MemOp = "NONE",
                OtherOps = "NONE",
                Successor = "STEP" 
            };

            simulatorCpu.CurrentMicroAddress = 0;
            simulatorCpu.ExecuteClockCycle();

            simulatorCpu.PrintCpuState();

            Assembler asm = new Assembler();


            ushort cod1 = asm.AssembleLine("ADD R1, R2");
            ushort cod2 = asm.AssembleLine("MOV R10, R15");

            System.Diagnostics.Debug.WriteLine($"ADD R1, R2 -> 0x{cod1:X4}");
            System.Diagnostics.Debug.WriteLine($"MOV R10, R15 -> 0x{cod2:X4}");
            System.Diagnostics.Debug.WriteLine($"INC R5 -> 0x{asm.AssembleLine("INC R5"):X4}");
            System.Diagnostics.Debug.WriteLine($"BEQ 10 -> 0x{asm.AssembleLine("BEQ 10"):X4}");
            System.Diagnostics.Debug.WriteLine($"HALT -> 0x{asm.AssembleLine("HALT"):X4}");

        }

        private void button1_Click(object sender, EventArgs e)
        {
            var microprogram = ProiectAC.Utilities.CsvParser.Load("microprogram.csv");

            if (microprogram.Count > 0)
            {
                var prima = microprogram[0];

                MessageBox.Show($"SUCCES! Am încărcat {microprogram.Count} instrucțiuni.\n\n" +
                                $"Test prima instrucțiune:\n" +
                                $"- Etichetă: {prima.Label}\n" +
                                $"- Adresă: {prima.Address}\n" +
                                $"- Operație ALU: {prima.AluOp}\n" +
                                $"- Adresă de Salt: {prima.JumpAddressText}",
                                "Test Reușit");
            }
            else
            {
                MessageBox.Show("Nu s-a încărcat nimic. Ceva nu e în regulă cu fișierul CSV.", "Eroare");
            }
        }
    }
}
