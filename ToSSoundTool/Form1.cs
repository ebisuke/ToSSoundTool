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
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void preferenceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SettingsForm form = new SettingsForm();
            form.ShowDialog(this);
        }

        private void preparationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(MessageBox.Show("Do you want to run the prep process?\r\nThis will take some time.",
                "Preparation",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question)==DialogResult.No)
            {
                return;
            }

            Preparation prep = new Preparation();
            ProgressForm form = new ProgressForm(prep);
            form.ShowDialog(this);
        }
    }
}