using WeirdWallpaperGenerator.Controllers;

namespace WeirdWallpaperGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            new Startup();

            MainController controller = new MainController();
            controller.ExecuteCommand(args);
        }
    }
}
