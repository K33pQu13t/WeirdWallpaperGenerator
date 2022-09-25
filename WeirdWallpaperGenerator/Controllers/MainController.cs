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
        private readonly ChaosNoiseConfigurer _chaosNoiseConfigurer = new ChaosNoiseConfigurer();

        private readonly MessagePrinterService _printer = MessagePrinterService.GetInstance();
        private readonly ReleasePreparingService _releasePreparingService = new ReleasePreparingService();
        private readonly UpdateService _updateService = new UpdateService();
        private readonly CommandService _commandService = new CommandService();

        [Description("shows info about assembly")]
        private readonly string about = "about";
      
        [Description("opens explorer's instance where generated picture was saved")]
        private readonly string[] flagShow = new string[] { "s", "show" };
        [Description("opens generated image by default system image viewer")]
        private readonly string[] flagOpen = new string[] { "o", "open" };

        SecureRandomService _rnd = new SecureRandomService();

        public async Task ExecuteCommand(string[] commandLineArray)
        {
            if (commandLineArray.Length == 0)
            {
                Console.WriteLine($"no command specified. Type {BasicCommandList.commandHelp.First()} to get help");
                return;
            }
            string commandLine = string.Join(" ", commandLineArray);
            Command command = _commandService.SplitToArguments(commandLine);

            Flag methodFlag = command.Flags.FirstOrDefault(x => BasicCommandList.flagMethod.Contains(x.Value));

            if (commandLine == about)
            {
                Console.WriteLine($"\n{GetAbout()}");
            }
            else if (command.IsHelpCommand) 
            {
                Console.WriteLine(GetHelp(command));
            }
            else if (BasicCommandList.commandSetWallpaper.Contains(command.Value))
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
            else if (BasicCommandList.commandGenerate.Contains(command.Value))
            {
                // for random method
                if (methodFlag == null)
                {
                    int decision = _rnd.Next(0, 2);
                    switch (decision)
                    {
                        case 0:
                            methodFlag = new Flag { Value = "m", Arguments = new List<Argument>() { new Argument { Value = "mb" } } };
                            _printer.Print($"Math billiards method was randomly choosen");
                            break;
                        case 1:
                            methodFlag = new Flag { Value = "m", Arguments = new List<Argument>() { new Argument { Value = "cn" } } };
                            _printer.Print($"Chaos noise method was randomly choosen");
                            break;
                    }
                }

                if (methodFlag.IsValue(_mathBilliardConfigurer.methods))
                {
                    MathBilliardsDrawer drawer = _mathBilliardConfigurer.Configure(command);
                    GenerateWallpaper(
                        drawer, 
                        command.ContainsFlag(flagShow),
                        command.ContainsFlag(flagOpen));
                }
                else if (methodFlag.IsValue(_chaosNoiseConfigurer.methods))
                {
                    ChaosNoiseDrawer drawer = _chaosNoiseConfigurer.Configure(command);
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
            else if (BasicCommandList.commandUpdate.Contains(command.Value))
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
            else if (command.Value == "pb")
            {
                // TODO: set increment stack from flag
                _releasePreparingService.Prepare(ReleasePreparingService.VersionStack.Patch);
            }
            #endif

            else
            {
                throw ExceptionHelper.GetException(nameof(MainController), nameof(ExecuteCommand),
                       $"Unknown command specified. Type ? to find out about available commands");
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
            if (!string.IsNullOrEmpty(command.Value) &&
                !_commandService.IsKnownCommand(command))
            {
                throw ExceptionHelper.GetException(nameof(MainController), nameof(GetHelp),
                       $"Unknown command specified. Type ? to find out about available commands");
            }

            string helpFor = string.Empty;
            if (command.Flags.Find(x => BasicCommandList.commandHelp.Contains(x.Value)).Arguments.Count() > 0)
                helpFor = command.GetFlagValues(BasicCommandList.commandHelp).Last().Trim();

            if (string.IsNullOrWhiteSpace(helpFor))
            {
                return  $"\n{GetAbout()}\n" +
                        $"\nThis program can generate images different ways, using flags to configure or random, " +
                        $"and set it as background image.\n" +
                        $"List of generic commands and flags is presented below. Use \"help\" with parameters to get more.\n" +
                        $"Common usage: {{/command}} [flags with arguments]\n\n" +
                        $"Generic commands:\n" +
                        $"{string.Join(", ", BasicCommandList.commandGenerate.Select(x => $"/{x}"))}: {DescriptionHelper.GetDescription< BasicCommandList> (nameof(BasicCommandList.commandGenerate))}\n" +
                        $"{string.Join(", ", BasicCommandList.commandSetWallpaper.Select(x => $"/{x}"))}: {DescriptionHelper.GetDescription< BasicCommandList> (nameof(BasicCommandList.commandSetWallpaper))}\n" +
                        $"{string.Join(", ", BasicCommandList.commandUpdate.Select(x => $"/{x}"))}: {DescriptionHelper.GetDescription< BasicCommandList> (nameof(BasicCommandList.commandUpdate))}\n" +
                        $"\nGeneric flags:\n" +
                        $"{string.Join(", ", flagShow.Select(x => $"-{x}"))}: {DescriptionHelper.GetDescription<MainController>(nameof(flagShow))}\n" +
                        $"{string.Join(", ", flagOpen.Select(x => $"-{x}"))}: {DescriptionHelper.GetDescription<MainController>(nameof(flagOpen))}\n" +
                        $"{string.Join(", ", BasicCommandList.flagMethod.Select(x => $"-{x}"))}: {DescriptionHelper.GetDescription< BasicCommandList> (nameof(BasicCommandList.flagMethod))}\n" +
                        $"\n{GetListOfMethods()}\n" +
                        $"\n{string.Join(", ", BasicCommandList.commandHelp)}: {DescriptionHelper.GetDescription<BasicCommandList>(nameof(BasicCommandList.commandHelp))}\n";
            }
            else
            {
                if (helpFor == about)
                {
                    return $"{about}: {DescriptionHelper.GetDescription<MainController>(nameof(about))}";
                }

                else if (BasicCommandList.commandGenerate.Contains(helpFor[1..])) 
                {
                    return $"{string.Join(", ", BasicCommandList.commandGenerate.Select(x => $"/{x}"))}: {DescriptionHelper.GetDescription< BasicCommandList> (nameof(BasicCommandList.commandGenerate))}";
                }
                else if (BasicCommandList.commandSetWallpaper.Contains(helpFor[1..]))
                {
                    return $"{string.Join(", ", BasicCommandList.commandSetWallpaper.Select(x => $"/{x}"))}: {DescriptionHelper.GetDescription< BasicCommandList> (nameof(BasicCommandList.commandSetWallpaper))}";
                }
                else if (BasicCommandList.commandUpdate.Contains(helpFor[1..]))
                {
                    return $"{string.Join(", ", BasicCommandList.commandUpdate.Select(x => $"/{x}"))}: {DescriptionHelper.GetDescription< BasicCommandList> (nameof(BasicCommandList.commandUpdate))}";
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
                else if (BasicCommandList.flagMethod.Contains(helpFor[1..]))
                {
                    return $"{helpFor}: {DescriptionHelper.GetDescription< BasicCommandList> (nameof(BasicCommandList.flagMethod))}\n" +
                           $"\n{GetListOfMethods()}\n" +
                           $"Type -m {{method_name}} ? to get more information about concrete method and it's flags";
                }
                else if (command.ContainsFlag(BasicCommandList.flagMethod))
                {
                    var method = command.GetFlagValue(BasicCommandList.flagMethod);
                    if (_mathBilliardConfigurer.methods.Contains(method))
                    {
                        return _mathBilliardConfigurer.GetHelp(command);
                    }
                    else if (_chaosNoiseConfigurer.methods.Contains(method))
                    {
                        return _chaosNoiseConfigurer.GetHelp(command);
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