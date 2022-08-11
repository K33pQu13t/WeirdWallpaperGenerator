using System;
using System.Linq;
using System.Threading.Tasks;
using WeirdWallpaperGenerator.Config;
using WeirdWallpaperGenerator.Controllers;
using WeirdWallpaperGenerator.Helpers;

namespace WeirdWallpaperGenerator
{
    class Program
    {
        static string[] positiveAnswers = new string[] { "y", "yes" };
        static string[] negativeAnswers = new string[] { "n", "no" };

        static async Task Main(string[] args)
        {
            var context = ContextConfig.GetInstance();
            var startup = new Startup();

            startup.Run();
            // do not need to wait for it now, let it be on a background
            context.UpdateLoading = startup.CheckUpdates();

            MainController controller = new MainController();
            //controller.ExecuteCommand(args);

            // now wait for update to download
            await context.UpdateLoading;

            if (context.ShouldUpdateOnExit)
            {
                if (context.UpdaterConfig.AskBeforeUpdate)
                {
                    Console.WriteLine("A new version of the programm is ready. Update it? (y/n)");
                    string answer = string.Empty;
                    while (!(positiveAnswers.Contains(answer) || negativeAnswers.Contains(answer)))
                    {
                        answer = Console.ReadLine();
                    }

                    if (negativeAnswers.Contains(answer))
                        return;
                }

                // TODO: start cmd process of cutting-pasting-deleting-running procces of updation here
                Console.WriteLine("update started");
            }
        }
    }
}
