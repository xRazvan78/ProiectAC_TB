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
        private Models.Cpu myCpu;

        public Form1()
        {
            InitializeComponent();
            myCpu = new Models.Cpu();
        }

        private void UpdateUI()
        {
            txtR0.Text = "0x" + myCpu.R[0].Value.ToString("X4");
            txtR1.Text = "0x" + myCpu.R[1].Value.ToString("X4");
            txtR2.Text = "0x" + myCpu.R[2].Value.ToString("X4");
            txtR3.Text = "0x" + myCpu.R[3].Value.ToString("X4");
            txtR4.Text = "0x" + myCpu.R[4].Value.ToString("X4");
            txtR5.Text = "0x" + myCpu.R[5].Value.ToString("X4");
            txtR6.Text = "0x" + myCpu.R[6].Value.ToString("X4");
            txtR7.Text = "0x" + myCpu.R[7].Value.ToString("X4");
            txtR8.Text = "0x" + myCpu.R[8].Value.ToString("X4");
            txtR9.Text = "0x" + myCpu.R[9].Value.ToString("X4");
            txtR10.Text = "0x" + myCpu.R[10].Value.ToString("X4");
            txtR11.Text = "0x" + myCpu.R[11].Value.ToString("X4");
            txtR12.Text = "0x" + myCpu.R[12].Value.ToString("X4");
            txtR13.Text = "0x" + myCpu.R[13].Value.ToString("X4");
            txtR14.Text = "0x" + myCpu.R[14].Value.ToString("X4");
            txtR15.Text = "0x" + myCpu.R[15].Value.ToString("X4");

            txtPC.Text = "0x" + myCpu.PC.Value.ToString("X4");
            txtSP.Text = "0x" + myCpu.SP.Value.ToString("X4");
            txtADR.Text = "0x" + myCpu.ADR.Value.ToString("X4");
            txtMDR.Text = "0x" + myCpu.MDR.Value.ToString("X4");
            txtIR.Text = "0x" + myCpu.IR.Value.ToString("X4");
            txtIVR.Text = "0x" + myCpu.IVR.Value.ToString("X4");
            txtT.Text = "0x" + myCpu.T.Value.ToString("X4");

            txtZ.BackColor = myCpu.Flags.Z ? Color.Red : Color.White;
            txtN.BackColor = myCpu.Flags.N ? Color.Red : Color.White;
            txtC.BackColor = myCpu.Flags.C ? Color.Red : Color.White;
            txtV.BackColor = myCpu.Flags.V ? Color.Red : Color.White;

            valSBUS.Text = "0x" + myCpu.SBUS.Value.ToString("X4");
            valDBUS.Text = "0x" + myCpu.DBUS.Value.ToString("X4");
            valRBUS.Text = "0x" + myCpu.RBUS.Value.ToString("X4");

            try
            {
                if (myCpu.MPM[myCpu.CurrentMicroAddress] != null)
                {
                    valALU.Text = myCpu.MPM[myCpu.CurrentMicroAddress].AluOp;
                }
                else
                {
                    valALU.Text = "NONE";
                }
            }
            catch
            {
                valALU.Text = "NONE";
            }

            if (listBoxMicroprogram.Items.Count > 0 && myCpu.CurrentMicroAddress < listBoxMicroprogram.Items.Count)
            {
                listBoxMicroprogram.SelectedIndex = myCpu.CurrentMicroAddress;
            }

            if (listBoxProgram.Items.Count > 0)
            {
                int programIndex = myCpu.PC.Value / 2;

                if (myCpu.CurrentMicroAddress != 0 && programIndex > 0)
                {
                    programIndex -= 1;
                }

                if (programIndex < listBoxProgram.Items.Count)
                {
                    listBoxProgram.SelectedIndex = programIndex;
                }
                else
                {
                    listBoxProgram.ClearSelected();
                }
            }

            if (myCpu.IR.Value == 0xF003 && myCpu.CurrentMicroAddress == 96)
            {
                if (timer1.Enabled)
                {
                    timer1.Stop();
                    btnRUN.Text = "RUN";
                    btnRUN.BackColor = Color.White;
                }

                btnSTEP.Enabled = false;
                btnRUN.Enabled = false;

                System.Diagnostics.Debug.WriteLine("Execuție finalizată cu succes. Procesorul s-a oprit la starea de HALT.");
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            myCpu.Reset();
            UpdateUI();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var microprogram = ProiectAC.Utilities.CsvParser.Load("microprogram.csv");

            if (microprogram.Count > 0)
            {
                listBoxMicroprogram.Items.Clear();

                for (int i = 0; i < myCpu.MPM.Length; i++)
                {
                    myCpu.MPM[i] = null;
                    listBoxMicroprogram.Items.Add($"{i}: [Nefolosit]");
                }

                foreach (var mir in microprogram)
                {
                    if (mir.Address >= 0 && mir.Address < myCpu.MPM.Length)
                    {
                        myCpu.MPM[mir.Address] = mir;
                        listBoxMicroprogram.Items[mir.Address] = $"{mir.Address}: {mir.Label} | ALU: {mir.AluOp} | Succ: {mir.Successor}";
                    }
                }

                myCpu.Ram.Clear();
                Assembler asm = new Assembler();

                ushort ramAddress = 0;
                int numarInstructiuni = 0;

                string fisierProgram = "program.txt";

                if (System.IO.File.Exists(fisierProgram))
                {
                    listBoxProgram.Items.Clear();

                    string[] instructiuni = System.IO.File.ReadAllLines(fisierProgram);

                    foreach (string linie in instructiuni)
                    {
                        if (string.IsNullOrWhiteSpace(linie) || linie.Trim().StartsWith(";"))
                            continue;

                        try
                        {
                            ushort machineCode = asm.AssembleLine(linie);
                            myCpu.Ram.Write(ramAddress, machineCode);

                            listBoxProgram.Items.Add($"[{ramAddress:X4}] {linie.Trim()}");

                            ramAddress += 2;      
                            numarInstructiuni++;  
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Eroare în '{fisierProgram}' la linia '{linie}':\n{ex.Message}",
                                            "Eroare Asamblor", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                    }

                    MessageBox.Show($"SUCCES!\nAm citit și asamblat cu succes {numarInstructiuni} instrucțiuni din '{fisierProgram}' în RAM!",
                                    "Sistem Pregătit");
                }
                else
                {
                    MessageBox.Show($"Fișierul '{fisierProgram}' nu a fost găsit!", "Atenție", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            else
            {
                MessageBox.Show("Nu s-a încărcat nimic din CSV. Verificați calea fișierului.", "Eroare");
            }

            myCpu.PC.Value = 0;
            myCpu.CurrentMicroAddress = 0;

            btnSTEP.Enabled = true;
            btnRUN.Enabled = true;

            UpdateUI();
        }

        private void label19_Click(object sender, EventArgs e)
        {
        }

        private void lblADR_Click(object sender, EventArgs e)
        {
        }

        private void btnSTEP_Click(object sender, EventArgs e)
        {
            myCpu.ExecuteClockCycle();
            UpdateUI();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            myCpu.ExecuteClockCycle();
            UpdateUI();
        }

        private void btnRUN_Click(object sender, EventArgs e)
        {
            if (timer1.Enabled == false)
            {
                timer1.Start();
                ((Button)sender).Text = "STOP";
                ((Button)sender).BackColor = Color.Orange;
            }
            else
            {
                timer1.Stop();
                ((Button)sender).Text = "RUN";
                ((Button)sender).BackColor = Color.White; 
            }
        }
    }
}