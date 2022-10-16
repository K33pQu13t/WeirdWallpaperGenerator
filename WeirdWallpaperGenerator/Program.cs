using System;
using System.Threading.Tasks;
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

            MainController controller = new MainController();
#if !DEBUG
            try
            {
#endif
            await controller.ExecuteCommand(args);

#if !DEBUG
            }
            catch (Exception ex)
            {
                MessagePrinterService.GetInstance().PrintError(ex.Message);
            }
#endif
        }
    }
}

