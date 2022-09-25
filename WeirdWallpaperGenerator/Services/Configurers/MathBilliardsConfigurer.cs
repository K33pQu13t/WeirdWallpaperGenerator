using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using WeirdWallpaperGenerator.Constants;
using WeirdWallpaperGenerator.DTO;
using WeirdWallpaperGenerator.Enums.Drawers;
using WeirdWallpaperGenerator.Helpers;
using WeirdWallpaperGenerator.Models.CommandLineParts;
using WeirdWallpaperGenerator.Services.Drawers;

namespace WeirdWallpaperGenerator.Services.Configurers
{
    [Description("math billiards generation method")]
    public class MathBilliardsConfigurer : DrawerConfigurer<MathBilliardsDrawer>
    {
        [Description(
            "sets the brush size - minimal pattern square. " +
            "Width must can be divided by brush size without remainder. " +
            "Brush value must be less than width and height\n" +
            "Usage: \n" +
            "  -b {integer number}")]
        readonly string[] brushFlag = new string[] { "b", "brush" };

        [Description(
            "sets generation's start corner. " +
            "Actualy, flips an image. " +
            "0 is for left up corner. " +
            "1 is for right up corner. " +
            "2 is for right down corner. " +
            "3 is for left down corner. " +
            "Usage: \n" +
            "  -sс {number according to corner}")]
        readonly string[] setCornerFlag = new string[] { "sс", "setcorner" };

        [Description(
            "generates a fractal picture according to prime number-lenghted sides. " +
            "This algorythm also called \"arithmetic billiards\". " +
            "The width and height must be coprime numbers, although this particular implementation " +
            "selects an area smaller than picture resolution (skipping pixel lines from top and bottom) " +
            "to print pattern if width and height aren't coprime, so you can input any sides. " +
            "More about this algorythm: https://en.wikipedia.org/wiki/Arithmetic_billiards")]
        public string[] methods = new string[] { "mb", "mathbilliards" };

        public MathBilliardsConfigurer()
        {
        }

        public override MathBilliardsDrawer Configure(Command command)
        {
            MathBilliardsConfigDto config = new MathBilliardsConfigDto();

            GetWidthAndHeight(command, out int width, out int height);
            config.Width = width;
            config.Height = height;

            // determining brush possible sizes
            if (command.ContainsFlag(brushFlag))
            {
                config.BrushSize = int.Parse(command.GetFlagValue(brushFlag));
            }
            else
            {
                List<int> brushSizes = config.Width.Value.GetAllDivisors().ToList();
                config.BrushSize = brushSizes[_rnd.Next(0, brushSizes.Count)];
            }

            // determining colors
            var colors = GetColors(command, chanceOfSwap: true);

            config.FillInsideColor = colors[0];
            config.FillOutsideColor = colors[1];

            // determining start corner position
            if (command.ContainsFlag(setCornerFlag))
            {
                config.StartPosition = (CornerPosition)int.Parse(command.GetFlagValue(setCornerFlag));
            }
            else
            {
                var cornerPositions = Enum.GetValues(typeof(CornerPosition))
                    .OfType<CornerPosition>().ToList();
                config.StartPosition = cornerPositions[_rnd.Next(0, cornerPositions.Count)];
            }

            // configure drawer
            return new MathBilliardsDrawer(config);
        }

        public override string GetHelp(Command command = null)
        {
            string helpFor = command?.GetFlagValues(BasicCommandList.commandHelp).Last();
            if (string.IsNullOrWhiteSpace(helpFor))
            {
                return $"{string.Join(", ", heightFlag.Select(x => $"-{x}"))}: {DescriptionHelper.GetDescription<MathBilliardsConfigurer>(nameof(heightFlag))}\n\n" +
                $"{string.Join(", ", widthFlag.Select(x => $"-{x}"))}: {DescriptionHelper.GetDescription<MathBilliardsConfigurer>(nameof(widthFlag))}\n\n" +
                $"{string.Join(", ", brushFlag.Select(x => $"-{x}"))}: {DescriptionHelper.GetDescription<MathBilliardsConfigurer>(nameof(brushFlag))}\n\n" +
                $"{string.Join(", ", colorsFlag.Select(x => $"-{x}"))}: {DescriptionHelper.GetDescription<MathBilliardsConfigurer>(nameof(colorsFlag))}\n\n" +
                $"{string.Join(", ", setCornerFlag.Select(x => $"-{x}"))}: {DescriptionHelper.GetDescription<MathBilliardsConfigurer>(nameof(setCornerFlag))}";
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
                return  $"-m {val}: {DescriptionHelper.GetDescription<MathBilliardsConfigurer>(nameof(methods))}\n" +
                        $"{GetMethodHelp()}";
            }
            else if (heightFlag.Contains(flagForHelp))
            {
                return $"{prefix} -{flagForHelp}: {DescriptionHelper.GetDescription<MathBilliardsConfigurer>(nameof(heightFlag))}";
            }
            else if (widthFlag.Contains(flagForHelp))
            {
                return $"{prefix} -{flagForHelp}: {DescriptionHelper.GetDescription<MathBilliardsConfigurer>(nameof(widthFlag))}";
            }
            else if (brushFlag.Contains(flagForHelp))
            {
                return $"{prefix} -{flagForHelp}: {DescriptionHelper.GetDescription<MathBilliardsConfigurer>(nameof(brushFlag))}";
            }
            else if (colorsFlag.Contains(flagForHelp))
            {
                return $"{prefix} -{flagForHelp}: {DescriptionHelper.GetDescription<MathBilliardsConfigurer>(nameof(colorsFlag))}";
            }
            else if (setCornerFlag.Contains(flagForHelp))
            {
                return $"{prefix} -{flagForHelp}: {DescriptionHelper.GetDescription<MathBilliardsConfigurer>(nameof(setCornerFlag))}";
            }

            return string.Empty;
        }

        public override string GetMethodHelp()
        {
            return  $"\nFlags:\n" +
                    $"{GetHelp()}";
        }
    }
}
