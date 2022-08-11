using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using WeirdWallpaperGenerator.Config;
using WeirdWallpaperGenerator.Services;

namespace WeirdWallpaperGenerator
{
    public class Startup
    {
        UpdateService _updater = new UpdateService();

        public void Run()
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

            // updater config section
            var updaterConfig = config.GetSection("UpdaterConfig").Get<UpdaterConfig>();

            // colors section
            // TODO: make array of object json section for color sets
            var cs1 = config.GetSection("colorSet1").Get<ColorSet>();
            var cs2 = config.GetSection("colorSet2").Get<ColorSet>();

            var contextConfig = ContextConfig.GetInstance();
            contextConfig.ColorsSets = new ColorsSets() { Sets = new List<ColorSet>() { cs1, cs2 } };
            contextConfig.About = about;
            contextConfig.UpdaterConfig = updaterConfig;
        }

        public async Task CheckUpdates()
        {
            if (await _updater.ShouldUpdate())
            {
                await _updater.GetUpdate(_updater.ReleaseFolder, _updater.TempPath);
            }
            ShouldUpdateOnExit();
        }

        public void ShouldUpdateOnExit()
        {
            if (_updater.IsUpdateReady()) {
                ContextConfig.GetInstance().ShouldUpdateOnExit = true;
            }
        }
    }
}
