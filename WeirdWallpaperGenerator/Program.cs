using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using WeirdWallpaperGenerator.Config;
using WeirdWallpaperGenerator.Controllers;
using WeirdWallpaperGenerator.Helpers;
using WeirdWallpaperGenerator.Services;

namespace WeirdWallpaperGenerator
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var context = ContextConfig.GetInstance();
            var startup = new Startup();

            startup.Run();
            // do not need to wait for it now, let it be on a background
            context.UpdateLoading = startup.CheckUpdates();

            MainController controller = new MainController();
            //controller.ExecuteCommand(args);
            controller.ExecuteCommand(new string[] { "/pb" });

            UpdateService _updater = new UpdateService();
            await _updater.CheckUpdateBeforeExit();
        }
    }
}
