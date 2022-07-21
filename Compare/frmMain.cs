using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Collections;

namespace Compare
{
    public partial class frmMain : Form
    {
        private Color invalidFileCol = Color.Gray;
        private Color validFileCol = Color.Black;

        private string[] alphabet = { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };
        
        private char splitChar = '^';
        private ArrayList file1, file2;

        public frmMain()
        {
            InitializeComponent();
        }

        private void compareFiles()
        {
            dataGridView1.Columns.Clear();
            loadCSV(txtFile1.Text, txtFile2.Text);
        }

        private string[] split(string line)
        {
            string[] lineArray;
            string newLine = "";
            bool inString = false;

            if (line == null)
                return new string[0];

            for (int i = 0; i < line.Length; i++)
            {
                if (line[i] == '"')
                {
                    inString = !inString;
                    if (i < line.Length - 1 && line[i + 1] == '"')
                    {
                        newLine += line[i];
                    }
                }
                else if (!inString && line[i] == ',')
                {
                    newLine += splitChar;

                }
                else
                {
                    newLine += line[i];
                }
            }
            lineArray = newLine.Split(splitChar);
            return lineArray;
        }

        private void loadCSV(string fileName1, string fileName2)
        {
            int lineNum = -1;
            int filelineNum = 0;
            try
            {
                using (TextReader reader1 = new StreamReader(fileName1))
                {
                    using (TextReader reader2 = new StreamReader(fileName2))
                    {
                        string line1, line2;
                        int numColumns = 0;

                        while ((line1 = reader1.ReadLine()) != null | (line2 = reader2.ReadLine()) != null)
                        {
                            filelineNum++;

                            string[] line1Array = split(line1);
                            string[] line2Array = split(line2);
                            if (lineNum == -1)
                            {
                                for (numColumns = 0; numColumns < Math.Max(line1Array.Length, line2Array.Length); numColumns++)
                                {
                                    string colName = getColumnName(numColumns);
                                    dataGridView1.Columns.Add(colName, colName);
                                }
                            }

                            dataGridView1.Rows.Add(line1Array);
                            lineNum++;
                            dataGridView1.Rows[lineNum].HeaderCell.Value = filelineNum.ToString();
                            dataGridView1.Rows[lineNum].DefaultCellStyle.BackColor =
                                System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));

                            if (!chkHeader.Checked || lineNum != 0)
                            {
                                dataGridView1.Rows.Add(line2Array);
                                lineNum++;
                                dataGridView1.Rows[lineNum].HeaderCell.Value = filelineNum.ToString();
                                dataGridView1.Rows[lineNum].DefaultCellStyle.BackColor =
                                    System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
                            }

                            if (lineNum == 0)
                            {
                                // set the header font
                                dataGridView1.Rows[lineNum].DefaultCellStyle.BackColor = Color.Gray;
                            }
                            else
                            {
                                for (int i = 0; i < Math.Max(line1Array.Length, line2Array.Length); i++)
                                {
                                    if (i >= line1Array.Length || i >= line2Array.Length)
                                    {
                                        dataGridView1.Rows[lineNum - 1].Cells[i].Style.ForeColor = Color.Red;
                                        dataGridView1.Rows[lineNum - 1].Cells[i].ToolTipText = txtFile1.Text;
                                        dataGridView1.Rows[lineNum].Cells[i].Style.ForeColor = Color.Red;
                                        dataGridView1.Rows[lineNum].Cells[i].ToolTipText = txtFile2.Text;
                                    }
                                    else if (line1Array[i].CompareTo(line2Array[i]) != 0)
                                    {
                                        dataGridView1.Rows[lineNum - 1].Cells[i].Style.BackColor = Color.Red;
                                        dataGridView1.Rows[lineNum - 1].Cells[i].ToolTipText = txtFile1.Text;
                                        dataGridView1.Rows[lineNum].Cells[i].Style.BackColor = Color.Red;
                                        dataGridView1.Rows[lineNum].Cells[i].ToolTipText = txtFile2.Text;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (FileNotFoundException fnfe)
            {
                MessageBox.Show(fnfe.Message);
            }
            catch (Exception e)
            {
                MessageBox.Show("Could not open\n " + e.Message);
            }
        }

        private string getColumnName(int i)
        {
            string name = (i + 1) + ":";

            int factor = i / alphabet.Length - 1;

            if (factor >= 0)
            {
                name += alphabet[factor];
            }
            name += alphabet[i % alphabet.Length];

            return name;
        }

        private bool canCompare()
        {
            return
                validFile(txtFile1.Text)
                && validFile(txtFile2.Text);
        }

        private bool validFile(string s)
        {
            return
                s != string.Empty &&
                File.Exists(s);
        }

        #region Events (Button Clicks etc...)
        private void txtFile1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && canCompare())
                compareFiles();            
        }

        private void txtFile2_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && canCompare())
                compareFiles();

        }

        private void btnFile1_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                txtFile1.Text = openFileDialog.FileName;
            }
        }

        private void btnFile2_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                txtFile2.Text = openFileDialog.FileName;
            }
        }
        #endregion
        #region Drag and Drop stuff
        private void txtFile1_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.Text))
                txtFile1.Text = e.Data.GetData(DataFormats.Text).ToString();
            else if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] s = (string[])e.Data.GetData(DataFormats.FileDrop, false);
                txtFile1.Text = s[0];
                txtFile1.Focus();
                this.Activate();
            }
        }

        private void txtFile1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.Text))
                e.Effect = DragDropEffects.Copy;
            else if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.All;
            else
                e.Effect = DragDropEffects.None;
        }

        private void txtFile2_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.Text))
                txtFile2.Text = e.Data.GetData(DataFormats.Text).ToString();
            else if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] s = (string[])e.Data.GetData(DataFormats.FileDrop, false);
                txtFile2.Text = s[0];
                txtFile2.Focus();
                this.Activate();
            }
        }

        private void txtFile2_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.Text))
                e.Effect = DragDropEffects.Copy;
            else if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.All;
            else
                e.Effect = DragDropEffects.None;
        }
        #endregion

        private void button1_Click(object sender, EventArgs e)
        {
            search();
        }

        private void search()
        {
            if (txtSearch.Text == string.Empty)
                return;

            for (int j = 0; j < dataGridView1.Rows.Count; j++)
            {
                for (int i = 0; i < dataGridView1.Columns.Count; i++)
                {
                    dataGridView1.Rows[j].Cells[i].Selected = false;

                    if (dataGridView1.Rows[j].Cells[i].Value.ToString().Contains(txtSearch.Text))
                    {
                        dataGridView1.Rows[j].Cells[i].Selected = false;
                        dataGridView1.Rows[j].Cells[i].Style.ForeColor = Color.Green;
                    }
                }
            }
        }

        private void chkHeader_CheckedChanged(object sender, EventArgs e)
        {
            compareFiles();
        }

        private void txtFile1_TextChanged(object sender, EventArgs e)
        {

            if (validFile(txtFile1.Text))
                txtFile1.ForeColor = validFileCol;
            else
                txtFile1.ForeColor = invalidFileCol;
        }

        private void txtFile2_TextChanged(object sender, EventArgs e)
        {
            if (validFile(txtFile2.Text))
                txtFile2.ForeColor = validFileCol;
            else
                txtFile2.ForeColor = invalidFileCol;
        }

        private void insertRowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("" + dataGridView1.SelectedRows[0]);
        }
    }
}
