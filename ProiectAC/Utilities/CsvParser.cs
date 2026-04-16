using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using ProiectAC.Models;

namespace ProiectAC.Utilities
{
    public static class CsvParser
    {
        public static List<Microinstruction> Load(string filePath)
        {
            List<Microinstruction> instructions = new List<Microinstruction>();

            try
            {
                if (!File.Exists(filePath))
                {
                    MessageBox.Show($"Fișierul nu a fost găsit: {filePath}", "Eroare", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return instructions;
                }

                string[] lines = File.ReadAllLines(filePath);

                foreach (string line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    string[] columns = line.Split(';');

                    if (columns.Length >= 10)
                    {
                        try
                        {
                            Microinstruction mi = new Microinstruction();

                            // Folosim variabilele adăugate pentru tine
                            mi.Label = columns[0].Replace(":", "").Trim();

                            string hexAddress = columns[2].Replace("0x", "").Trim();
                            if (!string.IsNullOrEmpty(hexAddress))
                            {
                                mi.Address = Convert.ToInt32(hexAddress, 16);
                            }

                            // Folosim numele scurte preferate de colegul tău
                            mi.SbusSource = ExtractText(columns[4]);
                            mi.DbusSource = ExtractText(columns[5]);
                            mi.AluOp = ExtractText(columns[6]);
                            mi.RbusDest = ExtractText(columns[7]);
                            mi.MemOp = ExtractText(columns[8]);
                            mi.OtherOps = ExtractText(columns[9]);
                            mi.Successor = ExtractText(columns[10]);
                            mi.IndexSelect = ExtractText(columns[11]);

                            string tf = ExtractText(columns[12]);
                            mi.TrueFalse = (tf == "T");

                            mi.JumpAddressText = ExtractText(columns[13]);

                            for (int i = columns.Length - 1; i >= 0; i--)
                            {
                                if (columns[i].Length == 36 && IsBinary(columns[i]))
                                {
                                    mi.BinaryCode = columns[i];
                                    break;
                                }
                            }

                            instructions.Add(mi);
                        }
                        catch { /* Ignorăm erorile pe linii individuale */ }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Eroare critică: {ex.Message}");
            }

            return instructions;
        }

        private static string ExtractText(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return "NONE";
            return raw.Split(':')[0].Trim();
        }

        private static bool IsBinary(string data)
        {
            foreach (char c in data) if (c != '0' && c != '1') return false;
            return true;
        }
    }
}