using WallpaperGenerator.Config;
using WallpaperGenerator.Controllers;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Collections.Generic;

namespace WallpaperGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("config.json", optional: false);
            IConfiguration config = builder.Build();

            // TODO: make array of object js section
            var cs1 = config.GetSection("colorSet1").Get<ColorSet>();
            var cs2 = config.GetSection("colorSet2").Get<ColorSet>();

            var contextConfig = ContextConfig.GetInstance();
            contextConfig.ColorsSets = new ColorsSets() { Sets = new List<ColorSet>() { cs1, cs2 } };



            MainController controller = new MainController();
            controller.ExecuteCommand("/g -m p -s");
        }
    }
}
