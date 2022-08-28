using System.Threading.Tasks;
using WeirdWallpaperGenerator.Configuration;
using WeirdWallpaperGenerator.Controllers;
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
#if !DEBUG
            try
            {
#endif
            // do not need to wait for it now, let it be on a background
            context.UpdateLoading = _updater.CheckUpdates();

            await controller.ExecuteCommand(args);

            await _updater.CheckUpdateBeforeExit();

#if !DEBUG
            }
            catch (Exception ex)
            {
                SystemMessagePrinter.GetInstance().PrintError(ex.Message);
            }
#endif
        }
    }
}

