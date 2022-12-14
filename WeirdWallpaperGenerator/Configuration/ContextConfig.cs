using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
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
        /// <summary>
        /// Application exe's directory
        /// </summary>
        public static string AppDirectory { get; private set; }
        private static string ConfigFilePath => Path.Combine(AppDirectory, "config.json");

        static private MessagePrinterService _printer;
        static private JsonSerializationService _jsonSerializationService;

        public About About => Config.About;
        public EnvironmentSettings EnvironmentSettings => Config.EnvironmentSettings;
        public UpdaterSettings UpdaterSettings => Config.UpdaterSettings;
        public ColorsSets ColorsSets => Config.ColorsSets;

        public bool ShouldUpdateOnExit { get; set; }
        public bool UpdateCorrupted { get; set; }
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
                AppDirectory = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);

                _printer = MessagePrinterService.GetInstance();
                _jsonSerializationService = new JsonSerializationService();
                if (!File.Exists(ConfigFilePath))
                {
                    instance = new ContextConfig(new Config());
                    Save();
                }
                else
                {
                    try
                    {
                        var config = GetConfig();
                        instance = new ContextConfig(config);
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

        private static Config GetConfig()
        {
            Config config = (Config)_jsonSerializationService.Deserialize(ConfigFilePath, typeof(Config));

            // guarantee of exact path
            foreach (var colorSet in config.ColorsSets.Sets)
            {
                if (!Path.IsPathRooted(colorSet.Path))
                    colorSet.Path = Path.Combine(AppDirectory, colorSet.Path);
            }
            if (!Path.IsPathRooted(config.EnvironmentSettings.SaveFolderPath))
            {
                config.EnvironmentSettings.SaveFolderPath = 
                    Path.Combine(AppDirectory, config.EnvironmentSettings.SaveFolderPath);
            }

            return config;
        }

        public static void Save(Config cfg = null, string path = "")
        {
            if (cfg == null)
                cfg = instance.Config;

            if (string.IsNullOrEmpty(path))
                path = ConfigFilePath;

            _jsonSerializationService.Serialize(path, cfg);
        }
    }
}
