using FractalGenerator.BackgroundDrawers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Linq;

namespace FractalGenerator.Controllers
{
    public class MainController
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int SystemParametersInfo(
           uint action, uint uParam, string vParam, uint winIni);

        private readonly uint SPI_SETDESKWALLPAPER = 0x14;
        private readonly uint SPIF_UPDATEINIFILE = 0x01;
        private readonly uint SPIF_SENDWININICHANGE = 0x02;

        private const string outputFolder = "backgrounds";

        public void ExecuteCommand(string[] commands)
        {
            Array.ForEach(commands, s => s.ToLower());

            if (commands[0] == "/setbg")
            {

            }
            else if (commands[0] == "/help" || commands[0] == "?")
            {

            }
        }

        public void ShowHelp()
        {
            Console.WriteLine("Actually I forgot to add any help...");
        }

        public void SetBackground(IDrawer drawer)
        {
            DateTime currentTime = DateTime.Now;
            Bitmap background = drawer.Draw();
            string title = $"{currentTime.ToString("MM.dd HH-mm-ss")} {nameof(drawer)} {drawer.GetConfig()}";

        }

        private void SetWallpaper(string path)
        {
            SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, path,
                SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE);
        }
    }
}
