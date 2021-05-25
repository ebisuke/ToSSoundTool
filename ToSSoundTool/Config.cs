using System.Configuration;

namespace ToSSoundTool
{
    public static class Config
    {
        private static Configuration _config;
        public static string BasePath
        {
            get
            {
                return _config.AppSettings.Settings["BasePath"].Value;
          
            }
            set
            {
                _config.AppSettings.Settings["BasePath"].Value=value;
            }
        }

        public static void Load()
        {
            _config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoaming);
            
        }
        public static void Save(){
            _config.Save();
        }
    }
}