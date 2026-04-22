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

            // 2. Simulăm o stare inițială
            // Punem manual valorile 10 și 25 în registrele generale R1 și R2
            simulatorCpu.R[1].Value = 10;
            simulatorCpu.R[2].Value = 25;

            // 3. Păcălim Instruction Register-ul (IR)
            // Vom simula că am extras din RAM instrucțiunea "ADD R1, R2"
            // Conform măștilor noastre: Biții 9-6 indică Sursa (R1), iar 3-0 indică Destinația (R2).
            // Binar: 0000 0000 0100 0010 => Hexazecimal: 0x0042
            simulatorCpu.IR.Value = 0x0042;

            // 4. Inserăm un microprogram "fals" la adresa 0 în MPM pentru a testa handlerele
            simulatorCpu.MPM[0] = new Microinstruction
            {
                Address = 0,
                SbusSource = "PDRGS",  // Ar trebui să citească R1 pe SBUS (valoarea 10)
                DbusSource = "PDRGD",  // Ar trebui să citească R2 pe DBUS (valoarea 25)
                AluOp = "SUM",         // ALU trebuie să le adune
                RbusDest = "PMRG",     // Rezultatul de pe RBUS trebuie să ajungă în Destinație (R2)
                MemOp = "NONE",
                OtherOps = "NONE",
                Successor = "STEP"     // Următorul pas (nu contează aici)
            };

            // 5. TESTUL SUPREM: Executăm un ciclu de ceas (Un "Tick")
            simulatorCpu.CurrentMicroAddress = 0;
            simulatorCpu.ExecuteClockCycle();

            simulatorCpu.PrintCpuState();

            // 6. Verificăm dacă hardware-ul și-a făcut treaba
            if (simulatorCpu.R[2].Value == 35)
            {
                MessageBox.Show("SUCCES TOTAL! Procesorul tău a decodificat instrucțiunea, a extras registrele, a adunat 10 cu 25 și a salvat 35 în R2. Inima hardware funcționează perfect!",
                                "Test Procesor", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show($"Ceva a mers greșit. Valoarea în R2 este: {simulatorCpu.R[2].Value}",
                                "Eroare", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Apelează parser-ul scris de tine
            var microprogram = ProiectAC.Utilities.CsvParser.Load("microprogram.csv");

            if (microprogram.Count > 0)
            {
                // Luăm prima instrucțiune ca să verificăm că a citit corect datele
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
