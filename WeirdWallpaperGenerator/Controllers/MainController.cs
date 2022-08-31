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
using WeirdWallpaperGenerator.Constants;
using WeirdWallpaperGenerator.Helpers;
using WeirdWallpaperGenerator.Models.CommandLineParts;
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

        private readonly MathBilliardsConfigurer _mathBilliardConfigurer = new MathBilliardsConfigurer();
        private readonly SystemMessagePrinter _printer = SystemMessagePrinter.GetInstance();
        private readonly ReleasePreparingService _releasePreparingService = new ReleasePreparingService();
        private readonly UpdateService _updateService = new UpdateService();
        private readonly CommandService _commandService = new CommandService();

        [Description("shows info about assembly")]
        private readonly string about = "about";
        [Description("generates an image and saves it. Usage: \n" +
            "  /g [flags]")]
        private readonly string[] commandGenerate = new string[] { "g", "gen", "generate" };
        [Description("generates an image, saves it and sets it as background image. Usage: \n" +
            "  /sw [flags]")]
        private readonly string[] commandSetWallpaper = new string[] { "sw", "setwp" };
        [Description("checks if there is an update, and downloads it. Usage: \n" +
            "  /u")]
        private readonly string[] commandUpdate = new string[] { "update", "upd", "u" };

        [Description("opens explorer's instance where generated picture was saved")]
        private readonly string[] flagShow = new string[] { "s", "show" };
        [Description("opens generated image by default system image viewer")]
        private readonly string[] flagOpen = new string[] { "o", "open" };
        [Description(
            "specifies a generation method. If not specified, then random method will be choosen, and some unique " +
            "configuration flags are not allowed. " +
            "Usage: -m {one of methods} [common generation flags like -w -h]")]
        private readonly string[] flagMethod = new string[] { "m", "method" };

        public async Task ExecuteCommand(string[] commandLineArray)
        {
            if (commandLineArray.Length == 0)
            {
                Console.WriteLine($"no command specified. Type {BasicCommandList.commandHelp.First()} to get help");
                return;
            }
            string commandLine = string.Join(" ", commandLineArray);
            Command command = _commandService.SplitToArguments(commandLine);

            Flag methodFlag = command.Flags.FirstOrDefault(x => flagMethod.Contains(x.Value));

            if (commandLine == about)
            {
                Console.WriteLine($"\n{GetAbout()}");
            }
            else if (command.IsHelpCommand) 
            {
                Console.WriteLine(GetHelp(command));
            }
            else if (commandSetWallpaper.Contains(command.Value))
            {
                // for random method
                if (methodFlag == null)
                {
                    // TODO: make it real random
                    methodFlag = new Flag { Value = "m", Arguments = new List<Argument>() { new Argument { Value = "mb" } } };
                }

                // for MathBilliardsDrawer
                if (methodFlag.IsValue(_mathBilliardConfigurer.methods))
                {
                    MathBilliardsDrawer drawer = _mathBilliardConfigurer.Configure(command);
                    string pathToWallpaper = GenerateWallpaper(
                        drawer, 
                        command.ContainsFlag(flagShow),
                        command.ContainsFlag(flagOpen));
                    SetWallpaper(pathToWallpaper);
                }
                // TODO: else if code there for another methods
                else
                {
                    throw ExceptionHelper.GetException(nameof(MainController), nameof(ExecuteCommand),
                        $"Unknown method \"{methodFlag.SingleArgumentValue}\" specified. Type -m ? to find out about available methods");
                }
            }
            else if (commandGenerate.Contains(command.Value))
            {
                // for random method
                if (methodFlag == null)
                {
                    // TODO: make it real random
                    methodFlag = new Flag { Value = "m", Arguments = new List<Argument>() { new Argument { Value = "mb" } } };
                }

                // for MathBilliardsDrawer
                if (methodFlag.IsValue(_mathBilliardConfigurer.methods))
                {
                    MathBilliardsDrawer drawer = _mathBilliardConfigurer.Configure(command);
                    GenerateWallpaper(
                        drawer, 
                        command.ContainsFlag(flagShow),
                        command.ContainsFlag(flagOpen));
                }
                // TODO: else if code there for another methods
                else
                {
                    throw ExceptionHelper.GetException(nameof(MainController), nameof(ExecuteCommand),
                        $"Unknown method \"{methodFlag.SingleArgumentValue}\" specified. Type -m ? to find out about available methods");
                }
            }
            else if (commandUpdate.Contains(command.Value))
            {
                var context = ContextConfig.GetInstance();

                await _updateService.CheckUpdates(isManual: true);
                await _updateService.CheckUpdateBeforeExit(isManual: true);
                context.ShouldUpdateOnExit = false;
                
            }
            // TODO: else if another possible commands

            #if DEBUG
            // prepare build folder for release
            else if (command.Value == "pb")
            {
                // TODO: set increment stack from flag
                _releasePreparingService.Prepare(ReleasePreparingService.VersionStack.Patch);
            }
            #endif

            else
            {
                throw ExceptionHelper.GetException(nameof(MainController), nameof(ExecuteCommand),
                       $"Unknown command specified. Type ? to find out about avaible commands. " +
                       $"Type WeirdWallpaperGenerator.exe /g -o for fast result if you don't want to " +
                       $"get into the syntax");
            }
        }

        /// <summary>
        /// generates and saves wallpaper
        /// </summary>
        /// <param name="drawer">configured IDrawer implementation to draw wallpaper</param>
        /// <param name="showFolder">true if it should open output folder and select created wallpaper after saving</param>
        /// <returns>absolute path to generated picture</returns>
        public string GenerateWallpaper(IDrawer drawer, bool showFolder = false, bool openImage = false)
        {
            DateTime currentTime = DateTime.Now;
            Bitmap background = drawer.Draw();
            string title = $"{currentTime:MM.dd HH-mm-ss} {drawer.GetArguments()}.png";
            string folderPath = Path.GetFullPath(
                ContextConfig.GetInstance().EnvironmentSettings.SaveFolderPath);
            Directory.CreateDirectory(folderPath);
            string path = Path.GetFullPath(Path.Combine(folderPath, title));
            
            background.Save(path);

            _printer.PrintSuccess("Wallpaper has been generated!");

            if (showFolder)
            {
                Process.Start("explorer.exe", $"/select, \"{path}\"");
            }
            if (openImage)
            {
                Process.Start(new ProcessStartInfo($"\"{Path.GetFullPath(path)}\"") { UseShellExecute = true });
            }

            return path;
        }

        private void SetWallpaper(string path)
        {
            SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, path,
                SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE);

            _printer.PrintSuccess("Wallpaper has been set as background image");
        }

        public string GetHelp(Command command = null)
        {
            string helpFor = string.Empty;
            if (command.Flags.Find(x => BasicCommandList.commandHelp.Contains(x.Value)).Arguments.Count() > 0)
                helpFor = command.GetFlagValues(BasicCommandList.commandHelp).Last();

            if (string.IsNullOrWhiteSpace(helpFor))
            {
                return  $"\n{GetAbout()}\n" +
                        $"\nThis program can generate images different ways, using flags to configure or random, " +
                        $"and set it as background image.\n" +
                        $"List of generic commands and flags is presented below. Use \"help\" with parameters to get more.\n" +
                        $"Common usage: {{/command}} [flags with arguments]\n\n" +
                        $"Generic commands:\n" +
                        $"{string.Join(", ", commandGenerate.Select(x => $"/{x}"))}: {DescriptionHelper.GetDescription<MainController>(nameof(commandGenerate))}\n" +
                        $"{string.Join(", ", commandSetWallpaper.Select(x => $"/{x}"))}: {DescriptionHelper.GetDescription<MainController>(nameof(commandSetWallpaper))}\n" +
                        $"{string.Join(", ", commandUpdate.Select(x => $"/{x}"))}: {DescriptionHelper.GetDescription<MainController>(nameof(commandUpdate))}\n" +
                        $"\nGeneric flags:\n" +
                        $"{string.Join(", ", flagShow.Select(x => $"-{x}"))}: {DescriptionHelper.GetDescription<MainController>(nameof(flagShow))}\n" +
                        $"{string.Join(", ", flagOpen.Select(x => $"-{x}"))}: {DescriptionHelper.GetDescription<MainController>(nameof(flagOpen))}\n" +
                        $"{string.Join(", ", flagMethod.Select(x => $"-{x}"))}: {DescriptionHelper.GetDescription<MainController>(nameof(flagMethod))}\n" +
                        $"\n{GetListOfMethods()}\n" +
                        $"\n{string.Join(", ", BasicCommandList.commandHelp)}: {DescriptionHelper.GetDescription<BasicCommandList>(nameof(Constants.BasicCommandList.commandHelp))}\n";
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
                else if (BasicCommandList.commandHelp.Contains(helpFor))
                {
                    return $"{string.Join(", ", BasicCommandList.commandHelp)}: {DescriptionHelper.GetDescription<BasicCommandList>(nameof(Constants.BasicCommandList.commandHelp))}";
                }

                else if (flagShow.Contains(helpFor[1..]))
                {
                    return $"{string.Join(", ", flagShow.Select(x => $"-{x}"))}: {DescriptionHelper.GetDescription<MainController>(nameof(flagShow))}";
                }
                else if (flagOpen.Contains(helpFor[1..]))
                {
                    return $"{string.Join(", ", flagOpen.Select(x => $"-{x}"))}: {DescriptionHelper.GetDescription<MainController>(nameof(flagOpen))}";
                }
                else if (flagMethod.Contains(helpFor[1..]))
                {
                    return $"{helpFor}: {DescriptionHelper.GetDescription<MainController>(nameof(flagMethod))}\n" +
                           $"\n{GetListOfMethods()}";
                }
                else if (command.ContainsFlag(flagMethod))
                {
                    var method = command.GetFlagValue(flagMethod);
                    if (_mathBilliardConfigurer.methods.Contains(method))
                    {
                        return _mathBilliardConfigurer.GetHelp(command);
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
                   $"{string.Join(", ", _mathBilliardConfigurer.methods)}: " +
                   $"{DescriptionHelper.GetDescription<MathBilliardsConfigurer>(nameof(MathBilliardsConfigurer))}";
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