using System;
using System.Threading.Tasks;
using WeirdWallpaperGenerator.Controllers;

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
                SystemMessagePrinter.GetInstance().PrintError(ex.Message);
            }
#endif
        }
    }
}

