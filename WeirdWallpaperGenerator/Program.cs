using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using WeirdWallpaperGenerator.Configuration;
using WeirdWallpaperGenerator.Controllers;
using WeirdWallpaperGenerator.Helpers;
using WeirdWallpaperGenerator.Services;

namespace WeirdWallpaperGenerator
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var startup = new Startup();
            startup.Run();

            var context = ContextConfig.GetInstance();
            UpdateService _updater = new UpdateService();

            MainController controller = new MainController();


            // do not need to wait for it now, let it be on a background
            //context.UpdateLoading = startup.CheckUpdates();

            //controller.ExecuteCommand(args);
            //controller.ExecuteCommand(new string[] { "/g -m p -s" });
            controller.ExecuteCommand(new string[] { "/pb" });

            //await _updater.CheckUpdateBeforeExit();
        }
    }
}
