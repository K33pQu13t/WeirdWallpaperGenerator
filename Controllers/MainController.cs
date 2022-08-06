using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using WeirdWallpaperGenerator.Helpers;
using WeirdWallpaperGenerator.Services;
using WeirdWallpaperGenerator.Services.BackgroundDrawers;
using WeirdWallpaperGenerator.Services.Configurers;

namespace WeirdWallpaperGenerator.Controllers
{
    public class MainController : IHaveHelper
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int SystemParametersInfo(
           uint action, uint uParam, string vParam, uint winIni);

        private readonly uint SPI_SETDESKWALLPAPER = 0x14;
        private readonly uint SPIF_UPDATEINIFILE = 0x01;
        private readonly uint SPIF_SENDWININICHANGE = 0x02;

        private PrimeFractalConfigurer _primeFractalConfigurer;

        [Description("generates an image and saves it. Usage: /g [flags]")]
        private readonly string[] commandGenerate = new string[] { "g", "gen", "generate" };
        [Description("generates an image, saves it and sets it as background image. Usage: /sw [flags]")]
        private readonly string[] commandSetWallpaper = new string[] { "sw", "setwp" };
        [Description("" +
            "shows the help about generic commands.\n" +
            "Specify a command or flag to get the exactly help. Usage: {command or flag} ? or ? {command or flag}\n" +
            "Or you can put it in the end of command line to get help about last element in command line. " +
            "Usage: [any command line] ?\n" +
            "Like \"/g -m p ?\" gets the help about prime method generation")]
        private readonly string[] commandHelp = new string[] { "help", "?" };

        [Description("if specified, opens explorer's instance where generated picture was saved")]
        private readonly string[] flagShow = new string[] { "s", "show" };
        [Description(
            "specifies a generation method. If not specified, then random method will be choosen. " +
            "Usage: -m {one of methods}")]
        private readonly string[] flagMethod = new string[] { "m", "method" };

        /// <summary>
        /// a folder to save wallpapers
        /// </summary>
        private const string outputFolder = "backgrounds";

        public MainController()
        {
            _primeFractalConfigurer = new PrimeFractalConfigurer();
        }

        public void ExecuteCommand(string[] commandLineArray)
        {
            if (commandLineArray.Length == 0) 
            {
                Console.WriteLine("no command specified. Type ? to get help");
                return;
            }
            string commandLine = string.Join(" ", commandLineArray);
            List<string> commandList = commandLine.ToLower().SplitToArguments().ToList();

            string methodValue = commandList.GetFlagValue(flagMethod); // TODO: check for -method

            if (commandList.Any(c => commandHelp.Contains(c)))
            {
                var value = commandList.GetHelpValue(commandHelp);
                Console.WriteLine(GetHelp(commandList, value));
            }
            else if (commandList.IsCommand(commandSetWallpaper))
            {
                if (commandList.ContainsFlag(flagMethod))
                {
                    // for PrimeFractalDrawer
                    if (commandList.ContainsFlag(flagMethod))
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
                if (methodValue.IsFlag(_primeFractalConfigurer.method))
                {
                    PrimeFractalDrawer drawer = _primeFractalConfigurer.Configure(commandList);
                    GenerateWallpaper(drawer, commandList.ContainsFlag(flagShow));
                }
                // TODO: else if code there for another methods
            }
            // TODO: else if another possible commands
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

        public string GetHelp(List<string> commandList = null, string helpFor = "")
        {
            if (string.IsNullOrWhiteSpace(helpFor))
            {
                // TODO: get assembly info there
                return  $"This program can generate images different ways, using flags or random, and set it as background image.\n" +
                        $"List of generic commands and flags is presented below. Use \"help\" with parameters to get more.\n" +
                        $"Common usage: {{/command}} [flags]\n\n" +
                        $"Generic commands:\n" +
                        $"{string.Join(", ", commandGenerate.Select(x => $"/{x}"))}: {DescriptionHelper.GetDescription<MainController>(nameof(commandGenerate))}\n" +
                        $"{string.Join(", ", commandSetWallpaper.Select(x => $"/{x}"))}: {DescriptionHelper.GetDescription<MainController>(nameof(commandSetWallpaper))}\n" +
                        $"\nGeneric flags:\n" +
                        $"{string.Join(", ", flagShow.Select(x => $"-{x}"))}: {DescriptionHelper.GetDescription<MainController>(nameof(flagShow))}\n" +
                        $"{string.Join(", ", flagMethod.Select(x => $"-{x}"))}: {DescriptionHelper.GetDescription<MainController>(nameof(flagMethod))}\n" +
                        $"\n{GetListOfMethods()}" +
                        $"\n{string.Join(", ", commandHelp)}: {DescriptionHelper.GetDescription<MainController>(nameof(commandHelp))}\n";
            }
            else
            {
                if (commandGenerate.Contains(helpFor[1..])) 
                {
                    return $"{string.Join(", ", commandGenerate.Select(x => $"/{x}"))}: {DescriptionHelper.GetDescription<MainController>(nameof(commandGenerate))}";
                }
                else if (commandSetWallpaper.Contains(helpFor[1..]))
                {
                    return $"{string.Join(", ", commandSetWallpaper.Select(x => $"/{x}"))}: {DescriptionHelper.GetDescription<MainController>(nameof(commandSetWallpaper))}";
                }
                else if (flagShow.Contains(helpFor[1..]))
                {
                    return $"{string.Join(", ", flagShow.Select(x => $"-{x}"))}: {DescriptionHelper.GetDescription<MainController>(nameof(flagShow))}";
                }
                else if (flagMethod.Contains(helpFor[1..]))
                {
                    return $"{helpFor}: {DescriptionHelper.GetDescription<MainController>(nameof(flagMethod))}\n" +
                           $"\n{GetListOfMethods()}";
                }
                else if (commandHelp.Contains(helpFor))
                {
                    return $"{helpFor}: {DescriptionHelper.GetDescription<MainController>(nameof(commandHelp))}";
                }
                else if (commandList.ContainsFlag(flagMethod))
                {
                    var method = commandList.GetFlagValue(flagMethod);
                    if (_primeFractalConfigurer.method.Any(x => method == x))
                    {
                        return _primeFractalConfigurer.GetHelp(helpFor: helpFor);
                    }
                    // TODO: else if for another methods
                    else
                    {
                        return $"unrecognised method \"{helpFor}\". Type \"-m ?\" to get the list of avaible methods";
                    }
                }

                return $"unrecognised element \"{helpFor}\" to get help for";
            }
        }

        private string GetListOfMethods()
        {
            return $"List of available methods:\n" +
                   $"{string.Join(", ", _primeFractalConfigurer.method)}: prime fractal generation method";
            // TODO: Add new methods there
        }
    }
}