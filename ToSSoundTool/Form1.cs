using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ToSSoundTool.Properties;

namespace ToSSoundTool
{
    public partial class Form1 : Form
    {
        private List<string> _originalSounds=new List<string>();
        private List<string> _srcSounds=new List<string>();
        private string currentDir = ".";
        private SoundModifyData _modifyData = new SoundModifyData();
        public Form1()
        {
            InitializeComponent();
        }

        private void preferenceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SettingsForm form = new SettingsForm();
            form.ShowDialog(this);
            UpdateOriginal();
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
            if (form.ShowDialog() == DialogResult.OK)
            {
                MessageBox.Show("Completed");
            }
            UpdateOriginal();
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.SelectedPath = currentDir;
            if (folderBrowserDialog1.ShowDialog(this) == DialogResult.OK)
            {
                currentDir = folderBrowserDialog1.SelectedPath;
                UpdateSrc();
            }
            
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            UpdateOriginal();
            UpdateSrc();
        }

        private void UpdateOriginal()
        {
            string soundDir = Path.Combine(Settings.Default.IntermediatePath, "seoriginal");
            if (Directory.Exists(soundDir))
            {
                var fs=Directory.GetFiles(soundDir, "*.wav");
                _originalSounds = new List<string>(fs.Select(x=>Path.GetFileName(x)));
            }
            else
            {
                _originalSounds = new List<string>();
            }

            listView1.VirtualListSize = _originalSounds.Count;
            listView1.Invalidate();
        }
        private void UpdateSrc()
        {
            string soundDir = this.currentDir;
            if (Directory.Exists(soundDir))
            {
                var fs=Directory.GetFiles(soundDir, "*.wav");
                _srcSounds = new List<string>(fs.Select(x=>Path.GetFullPath(x)));
            }
            else
            {
                _srcSounds = new List<string>();
            }

            listSrc.VirtualListSize = _srcSounds.Count;
            listSrc.Invalidate();
        }

        private void listView1_VirtualItemsSelectionRangeChanged(object sender, ListViewVirtualItemsSelectionRangeChangedEventArgs e)
        {
        
        }

        private void listView1_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            var s = _originalSounds[e.ItemIndex];
            e.Item = new ListViewItem();
            e.Item.Text = s;
            if (_modifyData.ModifyDictionary.ContainsKey(s))
            {
                e.Item.SubItems.Add(_modifyData.ModifyDictionary[s]);
            }else{
                e.Item.SubItems.Add("");
            }
            
        }

        private void listSrc_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            var s = _srcSounds[e.ItemIndex];
            e.Item = new ListViewItem();
            e.Item.Text = Path.GetFileName(s);
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            PlaySoundOriginal();
        }

        private void PlaySoundOriginal()
        {
            string soundDir = Path.Combine(Settings.Default.IntermediatePath, "seoriginal");
            if (listView1.SelectedIndices.Count > 0)
            {
                var sound = _originalSounds[listView1.SelectedIndices[0]];
                if(_modifyData.ModifyDictionary.ContainsKey(sound))
                {
                    sound = _modifyData.ModifyDictionary[sound];
                    SoundPlayer sp = new SoundPlayer(Path.Combine(currentDir,sound));
                    sp.Play();
                }
                else
                {
                    SoundPlayer sp = new SoundPlayer(Path.Combine(soundDir,sound));
                    sp.Play();
                }
              
            }
        }

        private void listSrc_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listSrc.SelectedIndices.Count > 0)
            {
                var sound = _srcSounds[listSrc.SelectedIndices[0]];
                SoundPlayer sp = new SoundPlayer(Path.Combine(currentDir,sound));
                sp.Play();
            }
        }

        private void onMenuNew(object sender, EventArgs e)
        {
            if(MessageBox.Show("Do you want to clear assignment?",
                "New Document",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question)==DialogResult.No)
            {
                return;
            }

            _modifyData = new SoundModifyData();
            UpdateOriginal();
        }

        private void onMenuOpen(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog(this) == DialogResult.OK)
            {
                using (FileStream fs = new FileStream(openFileDialog1.FileName, FileMode.Open, FileAccess.Read))
                {
                    _modifyData.LoadFromStream(fs,openFileDialog1.FileName);
                }
            }
            UpdateOriginal();
        }

        private void onMenuSave(object sender, EventArgs e)
        {
            if (saveFileDialog1.ShowDialog(this) == DialogResult.OK)
            {
                using (FileStream fs = new FileStream(saveFileDialog1.FileName, FileMode.Create, FileAccess.Write))
                {
                    _modifyData.SaveFromStream(fs,saveFileDialog1.FileName);
                }
            }
            UpdateOriginal();
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            if (listSrc.SelectedIndices.Count == 0)
            {
                return;
            }

            var selected = listSrc.SelectedIndices[0];
            foreach (int item in listView1.SelectedIndices)
            {
                _modifyData.ModifyDictionary[this._originalSounds[item]] = this._srcSounds[selected];
            }
            UpdateOriginal();
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            foreach (int item in listView1.SelectedIndices)
            {
                _modifyData.ModifyDictionary.Remove(this._originalSounds[item]);
            }
            UpdateOriginal();
        }

        private void buildToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(MessageBox.Show("Do you want to build?\r\nThis will take some time.",
                "Build",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question)==DialogResult.No)
            {
                return;
            }

            Build b = new Build(_modifyData);
            ProgressForm pf = new ProgressForm(b);
            if (pf.ShowDialog() == DialogResult.OK)
            {
                MessageBox.Show("Completed");
            }
        }


        private void toolStripMenuItem5_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog()
            {
                DefaultExt = "txt",
                Filter = "Text|*.txt|All Files|*"
            };
            if (sfd.ShowDialog(this) == DialogResult.OK)
            {
                using (StreamWriter sw = new StreamWriter(sfd.FileName))
                {
                    foreach (var v in this._originalSounds)
                    {
                        sw.WriteLine(v);
                    }
                }
            }

        }
    }
}