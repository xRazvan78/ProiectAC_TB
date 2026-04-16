using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using ProiectAC.Models; // Aici importăm clasa Microinstruction

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

                    // Folosim separatorul punct și virgulă conform fișierului tău
                    string[] columns = line.Split(';');

                    if (columns.Length >= 10)
                    {
                        try
                        {
                            Microinstruction mi = new Microinstruction();

                            // 1. Eticheta (ex: IFCH)
                            mi.Label = columns[0].Replace(":", "").Trim();

                            // 2. Adresa Hexa (ex: 0x0)
                            string hexAddress = columns[2].Replace("0x", "").Trim();
                            if (!string.IsNullOrEmpty(hexAddress))
                            {
                                mi.Address = Convert.ToInt32(hexAddress, 16);
                            }

                            // 3. Extragem doar textul operațiilor (înainte de ":")
                            mi.SbusSource = ExtractText(columns[4]);
                            mi.DbusSource = ExtractText(columns[5]);
                            mi.AluOperation = ExtractText(columns[6]);
                            mi.RbusDestination = ExtractText(columns[7]);
                            mi.MemoryOperation = ExtractText(columns[8]);
                            mi.OtherOperations = ExtractText(columns[9]);
                            mi.Successor = ExtractText(columns[10]);
                            mi.IndexSelection = ExtractText(columns[11]);

                            // 4. Condiția True/False (T sau F)
                            string tf = ExtractText(columns[12]);
                            mi.TrueFalse = (tf == "T");

                            // 5. Adresa de salt (text)
                            mi.JumpAddressText = ExtractText(columns[13]);

                            // 6. Codul binar lung (cel de 36 biți)
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
                        catch { /* Ignorăm liniile care nu pot fi parsate */ }
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