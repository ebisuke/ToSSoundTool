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
    public partial class ProgressForm : Form
    {
        public ILongRunningProcess Proc { get; private set; }
        private bool _complete = false;
        private Exception _capturedException;
        public ProgressForm(ILongRunningProcess proc)
        {
            Proc = proc;
            Proc.OnMessage += Proc_OnMessage;
            InitializeComponent();
        }

        private void Proc_OnMessage(object sender, string e)
        {
            try
            {
                if (_complete == false)
                {
                    backgroundWorker1.ReportProgress(0, e);
                }
            }
            catch (Exception)
            {
                
            }
        }

     
        private void ProgressForm_Load(object sender, EventArgs e)
        {
            backgroundWorker1.RunWorkerAsync();
        }

        private void ProgressForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_complete==false && e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Cancel();
            }
        }

        private void Cancel()
        {
            Proc.Cancel();
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                Proc.Run();
                DialogResult = DialogResult.OK;
            }
            catch (Exception exception)
            {
                _capturedException = exception;
                DialogResult = DialogResult.Cancel;
            }
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            _complete = true;
            if (_capturedException != null)
            {
                MessageBox.Show(_capturedException.ToString(), "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            this.Close();
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (_complete == false)
            {
                string[] spr = ((string) e.UserState).Split(new string[] {"\n"}, StringSplitOptions.None);
                foreach (string s in spr)
                {
                    listBox1.Items.Add(s);
                }

                listBox1.SelectedIndex = listBox1.Items.Count - 1;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Cancel();
        }
    }
}
