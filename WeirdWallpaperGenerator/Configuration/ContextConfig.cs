using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
//using System.Text.Json;
using System.Threading.Tasks;
using WeirdWallpaperGenerator.Services;

namespace WeirdWallpaperGenerator.Configuration
{
    /// <summary>
    /// an instance of app config
    /// </summary>
    public class ContextConfig
    {
        static private ContextConfig instance;
        private Config Config { get; }
        private const string fileName = "config.json";

        static private SystemMessagePrinter _printer;

        public About About => Config.About;
        public EnvironmentSettings EnvironmentSettings => Config.EnvironmentSettings;
        public UpdaterSettings UpdaterSettings => Config.UpdaterSettings;
        public ColorsSets ColorsSets => Config.ColorsSets;

        public bool ShouldUpdateOnExit { get; set; }
        public Task UpdateLoading { get; set; }

        private ContextConfig(Config config) 
        {
            Config = config;
            _printer = SystemMessagePrinter.GetInstance();
        }

        public static ContextConfig GetInstance()
        {
            if (instance == null)
            {
                _printer = SystemMessagePrinter.GetInstance();
                if (!File.Exists(fileName))
                {
                    instance = new ContextConfig(new Config());
                    Save();
                }
                else
                {
                    try
                    {
                        using (StreamReader file = File.OpenText(fileName))
                        {
                            JsonSerializer serializer = new JsonSerializer() { 
                                DateFormatString = "dd.MM.yyyy"
                            };
                            Config cfg = (Config)serializer.Deserialize(file, typeof(Config));
                            instance = new ContextConfig(cfg);
                        }
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

        public static void Save(Config cfg = null)
        {
            if (cfg == null)
                cfg = instance.Config;

            using (FileStream fs = new FileStream(fileName, FileMode.Create))
            {
                var settings = new JsonSerializerSettings()
                {
                    DateFormatString = "dd.MM.yyyy",
                    ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
                    Formatting = Formatting.Indented
                    //NullValueHandling = NullValueHandling.Ignore
                };
                string json = JsonConvert.SerializeObject(cfg, settings);
                fs.Write(new UTF8Encoding(true).GetBytes(json));
            }
        }
    }
}
