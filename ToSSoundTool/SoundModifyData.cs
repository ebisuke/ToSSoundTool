using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToSSoundTool
{
    public class SoundModifyData
    {
        public Dictionary<string, string> ModifyDictionary { get; } = new Dictionary<string, string>();

        public void LoadFromStream(Stream ss,string path)
        {
            ModifyDictionary.Clear();
            using (StreamReader sr = new StreamReader(ss))
            {
                while (!sr.EndOfStream)
                {
                    var line = sr.ReadLine();
                    string[] spr = line.Split(',');
                    if (spr.Length < 2)
                    {
                        continue;
                    }

                    Uri basePath = new Uri(Path.GetDirectoryName(path));
                    Uri filePath = new Uri(basePath,spr[1]);
                   
                    ModifyDictionary.Add(spr[0],filePath.LocalPath);
                }
            }
        }
        public void SaveFromStream(Stream ss,string path)
        {

            using (StreamWriter sr = new StreamWriter(ss))
            {
                foreach (var v in ModifyDictionary)
                {
                    Uri basePath = new Uri(Path.GetDirectoryName(path));
                    Uri filePath = new Uri(v.Value);
                    var relative = basePath.MakeRelativeUri(filePath);
                    sr.WriteLine(v.Key + "," + filePath.LocalPath);
                }
            }
        }
    }
}
