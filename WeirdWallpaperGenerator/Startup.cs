using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.IO;
using WeirdWallpaperGenerator.Config;

namespace WeirdWallpaperGenerator
{
    public class Startup
    {
        public Startup()
        {
            Configure();
        }

        private void Configure()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("config.json", optional: false);
            IConfiguration config = builder.Build();

            // about section
            var about = config.GetSection("About").Get<About>();

            // colors section
            // TODO: make array of object js section for color sets
            var cs1 = config.GetSection("colorSet1").Get<ColorSet>();
            var cs2 = config.GetSection("colorSet2").Get<ColorSet>();

            var contextConfig = ContextConfig.GetInstance();
            contextConfig.ColorsSets = new ColorsSets() { Sets = new List<ColorSet>() { cs1, cs2 } };
            contextConfig.About = about;
        }
    }
}
