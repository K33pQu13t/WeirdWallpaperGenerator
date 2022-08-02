using WallpaperGenerator.Helpers;
using WallpaperGenerator.Services.BackgroundDrawers;
using WallpaperGenerator.Services.Configurers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace WallpaperGenerator.Controllers
{
    public class MainController
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int SystemParametersInfo(
           uint action, uint uParam, string vParam, uint winIni);

        private readonly uint SPI_SETDESKWALLPAPER = 0x14;
        private readonly uint SPIF_UPDATEINIFILE = 0x01;
        private readonly uint SPIF_SENDWININICHANGE = 0x02;

        private PrimeFractalConfigurer _primeFractalConfigurer;

        private readonly string[] commandSetBg = new string[] { "setbg", "sbg" };
        private readonly string[] commandGenerate = new string[] { "generate", "gen", "g" };
        private readonly string[] commandHelp = new string[] { "help", "?" };

        private readonly string[] flagShow = new string[] { "s", "show" };
        private readonly string[] flagMethod = new string[] { "m", "method" };

        /// <summary>
        /// a folder to save wallpapers
        /// </summary>
        private const string outputFolder = "backgrounds";

        public MainController()
        {
            _primeFractalConfigurer = new PrimeFractalConfigurer();
        }

        public void ExecuteCommand(string commandLine)
        {
            commandLine = commandLine.ToLower();
            List<string> commandList = commandLine.SplitToArguments().ToList();

            string method = commandList.GetFlagValue(flagMethod); // TODO: check for -method

            if (commandList.IsCommand(commandSetBg))
            {
                if (commandList.ContainsFlag(flagMethod))
                {
                    // for PrimeFractalDrawer
                    if (method == "p" || method == "prime")
                    {
                        PrimeFractalDrawer drawer = _primeFractalConfigurer.Configure(commandList);

                        string pathToWallpaper = GenerateWallpaper(drawer, commandList.ContainsFlag(flagShow));
                        SetWallpaper(pathToWallpaper);
                    }
                    // TODO: else if there for another IDrawers
                }
                else
                {
                    // TODO: random method
                }
            }
            else if (commandList.IsCommand(commandGenerate))
            {
                // for PrimeFractalDrawer
                if (method == "p" || method == "prime")
                {
                    PrimeFractalDrawer drawer = _primeFractalConfigurer.Configure(commandList);
                    GenerateWallpaper(drawer, commandList.ContainsFlag(flagShow));
                }
                // TODO: else if there for another IDrawers
            }
            // TODO: else if another possible commands
            else if (commandList.IsCommand(commandHelp))
            {
                ShowHelp();
            }
        }

        public void ShowHelp()
        {
            // TODO
            Console.WriteLine("Actually I forgot to add any help...");
        }

        /// <summary>
        /// generates and saves wallpaper
        /// </summary>
        /// <param name="drawer">configured IDrawer implementation to draw wallpaper</param>
        /// <param name="openFolder">true if it should open output folder and select created wallpaper after saving</param>
        /// <returns>absolute path to generated picture</returns>
        public string GenerateWallpaper(IDrawer drawer, bool openFolder = false)
        {
            DateTime currentTime = DateTime.Now;
            Bitmap background = drawer.Draw();
            string title = $"{currentTime:MM.dd HH-mm-ss} {drawer.GetConfig()}.png";
            string path = Path.GetFullPath(Path.Combine(outputFolder, title));
            
            background.Save(path);

            // TODO: stop loading, clear
            Console.WriteLine("Wallpaper has been generated!");

            if (openFolder)
            {
                Process.Start("explorer.exe", $"/select, \"{path}\"");
            }
            return path;
        }

        private void SetWallpaper(string path)
        {
            SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, path,
                SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE);

            Console.WriteLine("Wallpaper has been set as background image");
        }
    }
}
