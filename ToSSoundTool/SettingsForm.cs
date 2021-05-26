using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ToSSoundTool
{
    public partial class SettingsForm : Form
    {
        public SettingsForm()
        {
            InitializeComponent();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.Reload();
            Close();
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.Save();
            Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog()
            {
                SelectedPath = textBox1.Text,
                ShowNewFolderButton = false
            };
            if (fbd.ShowDialog(this) == DialogResult.OK)
            {
                textBox1.Text = fbd.SelectedPath;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog()
            {
                SelectedPath = textBox2.Text,
                ShowNewFolderButton = false
            };
            if (fbd.ShowDialog(this) == DialogResult.OK)
            {
                textBox2.Text = fbd.SelectedPath;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            SaveFileDialog fbd = new SaveFileDialog()
            {
                AddExtension = true,
                FileName=textBox3.Text,
                Filter="IPF|*.ipf|All Files|*"
            };
            if (fbd.ShowDialog(this) == DialogResult.OK)
            {
                textBox3.Text = fbd.FileName;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            OpenFileDialog fbd = new OpenFileDialog()
            {
                AddExtension = true,
                FileName = textBox3.Text,
                Filter = "FMod DesignerCl|fmod_designercl.exe|All Files|*"
            };
            if (fbd.ShowDialog(this) == DialogResult.OK)
            {
                textBox4.Text = fbd.FileName;
            }
        }
    }
}
