using System;
using System.Diagnostics;
using System.IO;
using ToSSoundTool.Properties;
using tpIpfTool;

namespace ToSSoundTool
{
    public class Build:ILongRunningProcess
    {
        public event EventHandler<string> OnMessage;
        public int Progress { get; }
        private bool _cancel = false;
        private SoundModifyData _modifyData;

        public Build(SoundModifyData modifyData)
        {
            _modifyData = modifyData;
        }
        public void CheckCancel()
        {
            if (_cancel)
            {
                throw new OperationCanceledException("Cancelled");
            }
        }
        public void Run()
        {
            string procdir = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
            string complementarydir = Path.Combine(procdir,"complementary");

            string projdir = Path.Combine(Properties.Settings.Default.IntermediatePath, "proj");
            if (!Directory.Exists(projdir))
            {
                Directory.CreateDirectory(projdir);
            }
            string sedir = Path.Combine(Properties.Settings.Default.IntermediatePath, "se");
            if (!Directory.Exists(sedir))
            {
                Directory.CreateDirectory(sedir);
            }
            string seoriginaldir = Path.Combine(Properties.Settings.Default.IntermediatePath, "seoriginal");
            if (!Directory.Exists(seoriginaldir))
            {
                throw new InvalidOperationException("Please preparation first.");
            }
            string ipfdir = Path.Combine(Properties.Settings.Default.IntermediatePath, "sound.ipf");
            if (!Directory.Exists(ipfdir))
            {
                Directory.CreateDirectory(ipfdir);
            }
            string genedir = Path.Combine(Properties.Settings.Default.IntermediatePath, "gen");
            if (!Directory.Exists(genedir))
            {
                Directory.CreateDirectory(genedir);
            }
            //cleanup wavs
            var genfiles = Directory.GetFiles(genedir);
            foreach (string file in genfiles)
            {
                File.Delete(file);
            }
            string[] duplicatedirs = new[]
            {
                "battle",
                "misc",
                "mon",
                "pc",
                "skilvoice",
                "skilvoice\\Japanese",
            };
            OnMessage?.Invoke(this, "Copy Modified Sounds");
            foreach (var v in _modifyData.ModifyDictionary)
            {
                CheckCancel();
                OnMessage?.Invoke(this,  Path.GetFileName( v.Value)+">"+Path.GetFileName(v.Key));
                File.Copy(v.Key,Path.Combine(genedir, v.Value));
                
            }
            OnMessage?.Invoke(this, "Encode wav to mp3 and ogg");
            string[] wavs = Directory.GetFiles(genedir, "*.wav");
            foreach (string wav in wavs)
            {
                CheckCancel();
                OnMessage?.Invoke(this, wav);
                Process proclame = Process.Start(new ProcessStartInfo(Path.Combine(procdir,"lame.exe"), Path.GetFullPath(wav))
                {
                    CreateNoWindow = true,
                    WorkingDirectory = genedir,
                    RedirectStandardOutput = true,
                    UseShellExecute = false
                });
                while (!proclame.WaitForExit(100))
                {
                   
                }
                Process procogg = Process.Start(new ProcessStartInfo(Path.Combine(procdir,"oggenc2.exe"), Path.GetFullPath(wav))
                {
                    CreateNoWindow = true,
                    WorkingDirectory = genedir,
                    RedirectStandardOutput = true,
                    UseShellExecute = false
                    
                });
                while (!procogg.WaitForExit(100))
                {
                }
            }
            OnMessage?.Invoke(this, "Clear Previous Symlink");
            foreach (var s in duplicatedirs)
            {
                var ss = Path.Combine(sedir, s);
                CheckCancel();
                if (!Directory.Exists(ss))
                {
                    Directory.CreateDirectory(ss);
                }

                var ff = Directory.GetFiles(ss);
                foreach (var f in ff)
                {
                    File.Delete(f);
                }
            }

            OnMessage?.Invoke(this, "Creating Symlink");
            string[] compfiles = Directory.GetFiles(complementarydir, "*");
            foreach (string file in compfiles)
            {
                CheckCancel();
                OnMessage?.Invoke(this, file);
                foreach (var s in duplicatedirs)
                {
                    var ss = Path.Combine(sedir, s);
                    
                    PInvoke.CreateSymbolicLink(
                        Path.GetFullPath(Path.Combine(ss, Path.GetFileName(file))),
                        Path.GetFullPath(Path.Combine(genedir, Path.GetFileName(file))),
                        PInvoke.SYMBOLIC_LINK_FLAG.File |
                        PInvoke.SYMBOLIC_LINK_FLAG.AllowUnprivilegedCreate
                    );
                }
            }
            string[] files = Directory.GetFiles(genedir, "*");
            foreach (string file in files)
            {
                CheckCancel();
                OnMessage?.Invoke(this, file);
                foreach (var s in duplicatedirs)
                {
                    var ss = Path.Combine(sedir, s);
                    
                    PInvoke.CreateSymbolicLink(
                        Path.GetFullPath(Path.Combine(ss, Path.GetFileName(file))),
                            Path.GetFullPath(Path.Combine(genedir, Path.GetFileName(file))),
                        PInvoke.SYMBOLIC_LINK_FLAG.File |
                        PInvoke.SYMBOLIC_LINK_FLAG.AllowUnprivilegedCreate
                    );
                }
            }
            string[] filesOriginal = Directory.GetFiles(seoriginaldir, "*");
            foreach (string file in filesOriginal)
            {
                CheckCancel();
                OnMessage?.Invoke(this, file);
                foreach (var s in duplicatedirs)
                {
                    var ss = Path.Combine(sedir, s);
                    if (!File.Exists(Path.Combine(ss, Path.GetFileName(file))))
                    {
                        PInvoke.CreateSymbolicLink(
                            Path.GetFullPath(Path.Combine(ss, Path.GetFileName(file))),
                            Path.GetFullPath(Path.Combine(seoriginaldir, Path.GetFileName(file))),
                            PInvoke.SYMBOLIC_LINK_FLAG.File |
                            PInvoke.SYMBOLIC_LINK_FLAG.AllowUnprivilegedCreate
                        );
                    }
                }
            }
            OnMessage?.Invoke(this, "Clear Previous FSBs");
            
 
            var fsbs = Directory.GetFiles(projdir,"*.fsb");
            foreach (var f in fsbs)
            {
                File.Delete(f);
            }
        

            OnMessage?.Invoke(this, "Building FSB.");
            Process proc = Process.Start(new ProcessStartInfo(Settings.Default.FModclPath,
                $"-L japanese -pc \"{Path.GetFullPath(Path.Combine(projdir, "R1.fdp"))}\"")
            {
                CreateNoWindow = true,
                WorkingDirectory = projdir,
                RedirectStandardOutput = true,
                RedirectStandardInput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            });
            try
            {
                    
                proc.ErrorDataReceived += (o, e) =>
                {
                    if (e.Data != null)
                    {
                        OnMessage?.Invoke(this, e.Data);
                    }
                };
                proc.BeginErrorReadLine();
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
            
            
            OnMessage?.Invoke(this, "Building Ipf.");
            var ipffiles = Directory.GetFiles(ipfdir,"*");
            foreach (var f in ipffiles)
            {
                File.Delete(f);
            }
            var projfiles = Directory.GetFiles(projdir,"*");
            foreach (var f in projfiles)
            {
                File.Copy(f,Path.Combine(ipfdir,Path.GetFileName(f)));
            }

            ipffiles = Directory.GetFiles(ipfdir,"*");
            IpfPack ipf = new IpfPack();
            ipf.PacIpf(ipffiles, uint.Parse(Settings.Default.PatchVer), uint.Parse(Settings.Default.PatchVer), 
                Path.Combine(Settings.Default.IntermediatePath, "tmp.ipf"));
            File.Copy(Path.Combine(Settings.Default.IntermediatePath, "tmp.ipf"),Settings.Default.IpfName,true);
            

            OnMessage?.Invoke(this, "Complete.");

        }

        public void Cancel()
        {
            _cancel = true;
        }
    }
}