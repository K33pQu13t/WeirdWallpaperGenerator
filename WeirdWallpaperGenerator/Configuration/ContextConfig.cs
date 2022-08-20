using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using WeirdWallpaperGenerator.Models;
using WeirdWallpaperGenerator.Services;
using WeirdWallpaperGenerator.Services.Serialization;

namespace WeirdWallpaperGenerator.Configuration
{
    /// <summary>
    /// an instance of app config
    /// </summary>
    public class ContextConfig
    {
        static private ContextConfig instance;
        private Config Config { get; }
        private const string configFileName = "config.json";

        static private SystemMessagePrinter _printer;
        static private JsonSerializationService _jsonSerializationService;

        public About About => Config.About;
        public EnvironmentSettings EnvironmentSettings => Config.EnvironmentSettings;
        public UpdaterSettings UpdaterSettings => Config.UpdaterSettings;
        public ColorsSets ColorsSets => Config.ColorsSets;

        public bool ShouldUpdateOnExit { get; set; } = true; //TODO отладка удали true
        public string VersionFromUpdate { get; set; }
        /// <summary>
        /// wait for it to ensure the update is fully downloaded
        /// </summary>
        public Task UpdateLoading { get; set; }

        private ContextConfig(Config config) 
        {
            Config = config;
        }

        public static ContextConfig GetInstance()
        {
            if (instance == null)
            {
                _printer = SystemMessagePrinter.GetInstance();
                _jsonSerializationService = new JsonSerializationService();
                if (!File.Exists(configFileName))
                {
                    instance = new ContextConfig(new Config());
                    Save();
                }
                else
                {
                    try
                    {
                        Config cfg = (Config)_jsonSerializationService.Deserialize(configFileName, typeof(Config));
                        instance = new ContextConfig(cfg);
                    }
                    catch (Exception ex)
                    {
                        _printer.PrintError($"It seems like there is a problem with configuration file. " +
                            $"Try to delete \"config.json\" file from root folder and retry to run app. " +
                            $"It should be generated again with default settings. Or fix it by yourself " +
                            $"if you have an idea what's happend. More information: \n{ex.Message}");
                        Environment.Exit(-1);
                    }
                }
            }

            return instance;
        }

        public static void Save(Config cfg = null, string path = "")
        {
            if (cfg == null)
                cfg = instance.Config;

            if (string.IsNullOrEmpty(path))
                path = configFileName;

            _jsonSerializationService.Serialize(path, cfg);
        }
    }
}
