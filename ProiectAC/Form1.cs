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
                int startIndex = microprogram.FindIndex(m => m.Label == "IFCH");
                if (startIndex == -1) startIndex = 0;

                int adresaCurenta = 0;
                for (int i = startIndex; i < microprogram.Count; i++)
                {
                    myCpu.MPM[adresaCurenta] = microprogram[i];
                    adresaCurenta++;
                }

                myCpu.Ram.Write(0, 0x1234);
                myCpu.Ram.Write(1, 0xABCD);

                myCpu.R[5].Value = 0x00FF;
            }
            else
            {
                MessageBox.Show("Error");
            }

            myCpu.CurrentMicroAddress = 0;
            myCpu.PC.Value = 0;
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