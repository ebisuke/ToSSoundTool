using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToSSoundTool.Properties;
using tpIpfTool;

namespace ToSSoundTool
{
    public class Preparation:ILongRunningProcess
    {
        public event EventHandler<string> OnMessage;
        public int Progress { get; }

        private bool _cancel = false;

        public void CheckCancel()
        {
            if (_cancel)
            {
                throw new OperationCanceledException("Cancelled");
            }
        }
        public void Run()
        {
            //listing PATCHES
            List<string> patches = Directory.GetFiles(
                Path.Combine(Properties.Settings.Default.BasePath, "patch"), "*.ipf").ToList();
            string procdir = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);

            //sort by descending
            patches.Sort();
            patches.Reverse();
            patches.Add(  Path.Combine(Properties.Settings.Default.BasePath, "data","sound.ipf"));
            OnMessage?.Invoke(this,"Finding sound.ipf.");
            string toextpatch = null;
            List<string> requires = new List<string>()
            {
                "SE.fsb",
                "skilvoice_kor.fsb",
                "skilvoice_F1.fsb",
                "skilvoice_M1.fsb",
                "skilvoice_jap.fsb",

            };
            string projdir = Path.Combine(Properties.Settings.Default.IntermediatePath, "proj");
            if (!Directory.Exists(projdir))
            {
                Directory.CreateDirectory(projdir);
            }
            //cleanup wavs
            var projfiles = Directory.GetFiles(projdir);
            foreach (string file in projfiles)
            {
                File.Delete(file);
            }

            string patchno = null;
            //find sound.ipf
            foreach (var patch in patches)
            {
                CheckCancel();
                IpfPack ipf = new IpfPack(null);
                OnMessage?.Invoke(this,patch);
                //read the header
               
                using (FileStream fs = new FileStream(patch, FileMode.Open, FileAccess.Read))
                {
                    var result = ipf.CheckIpf(fs);
                    patchno = result.Item2.ipfPkgVer.ToString();
                    if (result.Item1.Any((x) => x.archNm == "sound.ipf"))
                    {
                        //OK
                        toextpatch = patch;
                        //extract sound.ipf
                       
                        var sounds = result.Item1.Where((x) => x.archNm == "sound.ipf");
                        foreach (var f in sounds)
                        {
                           
                            string filename = Path.Combine(projdir, (f.fileNm));
                            string dirname =
                                Path.Combine(projdir, Path.GetDirectoryName(f.fileNm));

                            if (File.Exists(filename))
                            {
                                continue;
                            }
                            
                            if (!Directory.Exists(dirname))
                            {
                                Directory.CreateDirectory(dirname);
                            }

                            requires.Remove(f.fileNm);
                            OnMessage?.Invoke(this, ">"+f.fileNm);
                            using (FileStream fsfile = new FileStream(filename, FileMode.Create, FileAccess.Write))
                            {
                                ipf.Ext1FileToStream(fs, f, fsfile);
                            }

                            
                        }
                    }

                    if (requires.Count == 0)
                    {
                        break;
                    }
                }
               
            }

            if (toextpatch == null)
            {
                //sound.ipf not found in patch
                throw new InvalidDataException("NotFound sound.ipf in patches");
            }
        
            OnMessage?.Invoke(this, "Extract sound.ipf");
           
            string sedir = Path.Combine(Properties.Settings.Default.IntermediatePath, "seoriginal");
            if (!Directory.Exists(sedir))
            {
                Directory.CreateDirectory(sedir);
            }
            //cleanup wavs
            var fileswavs = Directory.GetFiles(sedir);
            foreach (string file in fileswavs)
            {
                File.Delete(file);
            }

      
           

            OnMessage?.Invoke(this, "Extract wav");
            //extract wavs
            string wavdir = Path.Combine(Properties.Settings.Default.IntermediatePath, "tempwav");
            if (!Directory.Exists(wavdir))
            {
                Directory.CreateDirectory(wavdir);
            }
            var filestempwavs = Directory.GetFiles(sedir);
            foreach (string file in filestempwavs)
            {
            }
            string[] fsbs = Directory.GetFiles(projdir, "*.fsb");
            foreach (string fsb in fsbs)
            {
                CheckCancel();
                OnMessage?.Invoke(this, fsb);
                //cleanup wavs
                var files = Directory.GetFiles(wavdir);
                foreach (string file in files)
                {
                    File.Delete(file);
                }

                //copy fsb
                File.Copy(fsb, Path.Combine(wavdir, "tmp.fsb"));

                //run tool
                Process proc = Process.Start(new ProcessStartInfo(Path.Combine(procdir,"fsb_aud_extr.exe"),
                    Path.GetFullPath(Path.Combine(wavdir, "tmp.fsb")))
                {
                    CreateNoWindow = true,
                    WorkingDirectory = wavdir,
                    RedirectStandardOutput = true,
                    RedirectStandardInput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false
                });
                try
                {
                    
                    proc.OutputDataReceived += (o, e) =>
                    {
                        if (e.Data != null)
                        {
                            OnMessage?.Invoke(this, e.Data);
                        }
                    };
                    proc.BeginOutputReadLine();
                    while (!proc.WaitForExit(100))
                    {
                        CheckCancel();
                        proc.StandardInput.WriteLine();
                        
                    }
                }
                catch (Exception)
                {
                    proc.Kill();
                    throw;
                }

                //copy common dir
                var fileswav = Directory.GetFiles(wavdir, "*.wav");
                foreach (var file in fileswav)
                {
                    CheckCancel();
                    OnMessage?.Invoke(this, ">"+file);
                    File.Move(file,Path.Combine(sedir,Path.GetFileName(file)));
                }
            }

            //encoding
            OnMessage?.Invoke(this, "Encode wav to mp3 and ogg");
            string[] wavs = Directory.GetFiles(sedir, "*.wav");
            foreach (string wav in wavs)
            {
                CheckCancel();
                OnMessage?.Invoke(this, wav);
                Process proclame = Process.Start(new ProcessStartInfo(Path.Combine(procdir,"lame.exe"), Path.GetFullPath(wav))
                {
                    CreateNoWindow = true,
                    WorkingDirectory = wavdir,
                    RedirectStandardOutput = true,
                    UseShellExecute = false
                });
                while (!proclame.WaitForExit(100))
                {
                   
                }
                Process procogg = Process.Start(new ProcessStartInfo(Path.Combine(procdir,"oggenc2.exe"), Path.GetFullPath(wav))
                {
                    CreateNoWindow = true,
                    WorkingDirectory = wavdir,
                    RedirectStandardOutput = true,
                    UseShellExecute = false
                    
                });
                while (!procogg.WaitForExit(100))
                {
                }
            }

            //complete
            OnMessage?.Invoke(this, "Complete");
            Settings.Default.PatchVer = patchno;
            Settings.Default.Save();
        }

        public void Cancel()
        {
            _cancel = true;
        }
    }
}
