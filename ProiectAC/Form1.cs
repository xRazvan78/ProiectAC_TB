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
