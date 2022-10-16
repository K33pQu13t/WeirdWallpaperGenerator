using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using WeirdWallpaperGenerator.Constants;
using WeirdWallpaperGenerator.DTO;
using WeirdWallpaperGenerator.Helpers;
using WeirdWallpaperGenerator.Models.CommandLineParts;
using WeirdWallpaperGenerator.Services.Drawers;

namespace WeirdWallpaperGenerator.Services.Configurers
{
    public class ChaosNoiseConfigurer : DrawerConfigurer<ChaosNoiseDrawer>
    {
        [Description(
           "sets the brush size - minimal pattern rectangle. " +
           "Width and height must can be divided by brush size without remainder. " +
           "You can specify x and y size of brush separately. " +
           "Brush value must be less than width and height\n" +
           "Usage: \n" +
           "  -b {integer number}\n" +
           "  -b {x length} {y length}")]
        private readonly string[] brushFlag = new string[] { "b", "brush" };
        
        [Description(
          "generates chaose non-pretected noise picture. " +
          "Does not depend on datetime, so it can be " +
          "safely ran at the same time regurally, " +
          "and you would get different pictures each time")]
        public string[] methods = new string[] { "cn", "chaosnoise" };

        public override ChaosNoiseDrawer Configure(Command command)
        {
            ChaosNoiseConfigDto config = new ChaosNoiseConfigDto();

            GetWidthAndHeight(command, out int width, out int height);
            config.Width = width;
            config.Height = height;

            var colors = GetColors(command);
            config.ColoredColor = colors[0];
            config.BackgroundColor = colors[1];
           
            List<string> args = command.GetFlagValues(brushFlag).ToList();
            if (args.Count == 0)
            {
                List<int> brushSizes = config.Width.Value.GetAllDivisors().ToList();
                config.BrushSizeX = brushSizes[_rnd.Next(0, brushSizes.Count)];
                brushSizes = config.Height.Value.GetAllDivisors().ToList();
                config.BrushSizeY = brushSizes[_rnd.Next(0, brushSizes.Count)];
            }
            else
            {
                config.BrushSizeX = Convert.ToInt32(args[0]);
                if (args.Count == 2)
                {
                    config.BrushSizeY = Convert.ToInt32(args[1]);
                }
                else
                {
                    config.BrushSizeY = config.BrushSizeX;
                }

                if (config.Width % config.BrushSizeX != 0
                    || config.Height % config.BrushSizeY != 0)
                {
                    throw ExceptionHelper.GetException(nameof(ChaosNoiseConfigurer), nameof(Configure),
                        "X size of brush must can be divided by Width without remainder. " +
                        "Y size of brush must can be divided by Height without remainder");
                }
            }

            return new ChaosNoiseDrawer(config);
        }

        public override string GetHelp(Command command = null)
        {
            string helpFor = command?.GetFlagValues(BasicCommandList.commandHelp).Last();
            if (string.IsNullOrWhiteSpace(helpFor))
            {
                return $"{string.Join(", ", heightFlag.Select(x => $"-{x}"))}: {DescriptionHelper.GetDescription<ChaosNoiseConfigurer>(nameof(heightFlag))}\n\n" +
                $"{string.Join(", ", widthFlag.Select(x => $"-{x}"))}: {DescriptionHelper.GetDescription<ChaosNoiseConfigurer>(nameof(widthFlag))}\n\n" +
                $"{string.Join(", ", brushFlag.Select(x => $"-{x}"))}: {DescriptionHelper.GetDescription<ChaosNoiseConfigurer>(nameof(brushFlag))}\n\n" +
                $"{string.Join(", ", colorsFlag.Select(x => $"-{x}"))}: {DescriptionHelper.GetDescription<ChaosNoiseConfigurer>(nameof(colorsFlag))}\n\n";
            }

            string prefix =
                $"-{command.Flags.Find(x => BasicCommandList.flagMethod.Any(v => v == x.Value)).Value} " +
                $"{command.GetFlagValue(BasicCommandList.flagMethod)}";
            string[] line = helpFor.Split(' ').Select(x => x.Replace("\"", "")).ToArray();

            string flagForHelp = line.First();
            if (line.Length > 1)
                flagForHelp = line[^2]; // pre last
            var val = line.Last();

            if (flagForHelp.StartsWith('-'))
                flagForHelp = flagForHelp[1..];

            if (methods.Contains(val))
            {
                return $"-m {val}: {DescriptionHelper.GetDescription<ChaosNoiseConfigurer>(nameof(methods))}\n" +
                        $"{GetMethodHelp()}";
            }
            else if (heightFlag.Contains(flagForHelp))
            {
                return $"{prefix} -{flagForHelp}: {DescriptionHelper.GetDescription<ChaosNoiseConfigurer>(nameof(heightFlag))}";
            }
            else if (widthFlag.Contains(flagForHelp))
            {
                return $"{prefix} -{flagForHelp}: {DescriptionHelper.GetDescription<ChaosNoiseConfigurer>(nameof(widthFlag))}";
            }
            else if (brushFlag.Contains(flagForHelp))
            {
                return $"{prefix} -{flagForHelp}: {DescriptionHelper.GetDescription<ChaosNoiseConfigurer>(nameof(brushFlag))}";
            }
            else if (colorsFlag.Contains(flagForHelp))
            {
                return $"{prefix} -{flagForHelp}: {DescriptionHelper.GetDescription<ChaosNoiseConfigurer>(nameof(colorsFlag))}";
            }

            return string.Empty;
        }

        public override string GetMethodHelp()
        {
            return $"\nFlags:\n" +
                    $"{GetHelp()}";
        }
    }
}
