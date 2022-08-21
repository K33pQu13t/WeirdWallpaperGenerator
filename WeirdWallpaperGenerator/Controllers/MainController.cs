using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using WeirdWallpaperGenerator.Configuration;
using WeirdWallpaperGenerator.Helpers;
using WeirdWallpaperGenerator.Services;
using WeirdWallpaperGenerator.Services.Configurers;
using WeirdWallpaperGenerator.Services.Drawers;

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

        private readonly PrimeFractalConfigurer _primeFractalConfigurer = new PrimeFractalConfigurer();
        private readonly SystemMessagePrinter _printer = SystemMessagePrinter.GetInstance();
        private readonly ReleasePreparingService _releasePreparingService = new ReleasePreparingService();
        private readonly UpdateService _updateService = new UpdateService();

        [Description("shows info about assembly")]
        private readonly string about = "about";
        [Description("generates an image and saves it. Usage: /g [flags]")]
        private readonly string[] commandGenerate = new string[] { "g", "gen", "generate" };
        [Description("generates an image, saves it and sets it as background image. Usage: /sw [flags]")]
        private readonly string[] commandSetWallpaper = new string[] { "sw", "setwp" };
        [Description("checks if there is an update, and downloads it. Usage: /u")]
        private readonly string[] commandUpdate = new string[] { "update", "upd", "u" };
        [Description(
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



        public async Task ExecuteCommand(string[] commandLineArray)
        {
            if (commandLineArray.Length == 0)
            {
                Console.WriteLine($"no command specified. Type {commandHelp.First()} to get help");
                return;
            }
            string commandLine = string.Join(" ", commandLineArray);
            List<string> commandList = commandLine.ToLower().SplitToArguments().ToList();

            string methodValue = commandList.GetFlagValue(flagMethod);

            if (commandLine == about)
            {
                Console.WriteLine($"\n{GetAbout()}");
            }
            else if (commandList.Any(c => commandHelp.Contains(c)))
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
            else if (commandList.IsCommand(commandUpdate))
            {
                var context = ContextConfig.GetInstance();
                // if update didn't start automatically
                if (context.UpdaterSettings.AutoCheckUpdates == false)
                {
                    await _updateService.CheckUpdates(isManual: true);
                    await _updateService.CheckUpdateBeforeExit(isManual: true);
                    context.ShouldUpdateOnExit = false;
                }
            }
            // TODO: else if another possible commands

            #if DEBUG
            // prepare build folder for release
            else if (commandList.IsCommand("pb"))
            {
                // TODO: set increment stack from flag
                _releasePreparingService.Prepare(ReleasePreparingService.VersionStack.Patch);
            }
            #endif
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
            string folderPath = Path.GetFullPath(
                ContextConfig.GetInstance().EnvironmentSettings.SaveFolderPath);
            string path = Path.GetFullPath(Path.Combine(folderPath, title));
            
            background.Save(path);

            // TODO: stop loading, clear
            _printer.PrintSuccess("Wallpaper has been generated!");

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

            _printer.PrintSuccess("Wallpaper has been set as background image");
        }

        public string GetHelp(List<string> commandList = null, string helpFor = "")
        {
            if (string.IsNullOrWhiteSpace(helpFor))
            {
                return  $"\n{GetAbout()}\n" +
                        $"\nThis program can generate images different ways, using flags or random, and set it as background image.\n" +
                        $"List of generic commands and flags is presented below. Use \"help\" with parameters to get more.\n" +
                        $"Common usage: {{/command}} [flags]\n\n" +
                        $"Generic commands:\n" +
                        $"{string.Join(", ", commandGenerate.Select(x => $"/{x}"))}: {DescriptionHelper.GetDescription<MainController>(nameof(commandGenerate))}\n" +
                        $"{string.Join(", ", commandSetWallpaper.Select(x => $"/{x}"))}: {DescriptionHelper.GetDescription<MainController>(nameof(commandSetWallpaper))}\n" +
                        $"{string.Join(", ", commandUpdate.Select(x => $"/{x}"))}: {DescriptionHelper.GetDescription<MainController>(nameof(commandUpdate))}\n" +
                        $"\nGeneric flags:\n" +
                        $"{string.Join(", ", flagShow.Select(x => $"-{x}"))}: {DescriptionHelper.GetDescription<MainController>(nameof(flagShow))}\n" +
                        $"{string.Join(", ", flagMethod.Select(x => $"-{x}"))}: {DescriptionHelper.GetDescription<MainController>(nameof(flagMethod))}\n" +
                        $"\n{GetListOfMethods()}\n" +
                        $"\n{string.Join(", ", commandHelp)}: {DescriptionHelper.GetDescription<MainController>(nameof(commandHelp))}\n";
            }
            else
            {
                if (helpFor == about)
                {
                    return $"{about}: {DescriptionHelper.GetDescription<MainController>(nameof(about))}";
                }

                else if (commandGenerate.Contains(helpFor[1..])) 
                {
                    return $"{string.Join(", ", commandGenerate.Select(x => $"/{x}"))}: {DescriptionHelper.GetDescription<MainController>(nameof(commandGenerate))}";
                }
                else if (commandSetWallpaper.Contains(helpFor[1..]))
                {
                    return $"{string.Join(", ", commandSetWallpaper.Select(x => $"/{x}"))}: {DescriptionHelper.GetDescription<MainController>(nameof(commandSetWallpaper))}";
                }
                else if (commandUpdate.Contains(helpFor[1..]))
                {
                    return $"{string.Join(", ", commandUpdate.Select(x => $"/{x}"))}: {DescriptionHelper.GetDescription<MainController>(nameof(commandUpdate))}";
                }
                else if (commandHelp.Contains(helpFor))
                {
                    return $"{helpFor}: {DescriptionHelper.GetDescription<MainController>(nameof(commandHelp))}";
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
                   $"{string.Join(", ", _primeFractalConfigurer.method)}: " +
                   $"{DescriptionHelper.GetDescription<PrimeFractalConfigurer>(nameof(PrimeFractalConfigurer))}";
            // TODO: Add new methods there
        }

        private string GetAbout()
        {
            var about = ContextConfig.GetInstance().About;
            return  $"{about.ProjectName}\n" +
                    $"version: {about.Version}\n" +
                    $"release date: {about.ReleaseDate}\n" +
                    $"author: {about.Author}";
        }
    }
}