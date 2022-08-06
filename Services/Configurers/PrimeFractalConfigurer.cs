using WeirdWallpaperGenerator.DTO;
using WeirdWallpaperGenerator.Helpers;
using WeirdWallpaperGenerator.Services.BackgroundDrawers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using WeirdWallpaperGenerator.Config;
using System.ComponentModel;

namespace WeirdWallpaperGenerator.Services.Configurers
{
    class PrimeFractalConfigurer : IDrawerConfigurer<PrimeFractalDrawer>, IHaveHelper
    {
        readonly ColorService _colorService;
        readonly Random _rnd;
        readonly ContextConfig _contextConfig;

        [Description(
            "sets the height of output picture. " +
            "Do not specify to set your main monitor height. " +
            "If specified without width, then width = height. " +
            "Usage: -h {integer number}")]
        readonly string[] heightFlag = new string[] { "h", "height" };

        [Description(
            "sets the width of output picture. " +
            "Do not specify to set your main monitor width. " +
            "If specified without height, then height = width. " +
            "Usage: -w {integer number}")]
        readonly string[] widthFlag = new string[] { "w", "width" };

        [Description(
            "sets the brush size - minimal pattern square. " +
            "Width must can be divided by brush size without remainder. " +
            "brush value must be less than width and height" +
            "Usage: -b {integer number}")]
        readonly string[] brushFlag = new string[] { "b", "brush" };

        [Description(
            "sets colors for generating image. " +
            "Do not specify to get random colors. " +
            "Colors must be in hex format. Opacity allowed. " +
            "Need to specify two colors/ " +
            "Usage: -с \"{hex colors separated by commas}\"")]
        readonly string[] colorsFlag = new string[] { "c", "colors" };


        [Description(
            "sets generation's start corner. " +
            "Actualy, flips an image. " +
            "0 is for left up corner. " +
            "1 is for right up corner. " +
            "2 is for right down corner. " +
            "3 is for left down corner. " +
            "Usage: -sp {number according to corner}")]
        readonly string[] setCornerFlag = new string[] { "sp", "setcorner" };

        [Description(
            "generates a fractal picture according to prime numbers properties. " +
            "This algorythm also called \"arithmetic billiards\". " +
            "The width and height must be coprime numbers, although this particular implementation " +
            "selects an area smaller than picture resolution (skipping pixel lines from top and bottom) " +
            "to print pattern if width and height aren't coprime. " +
            "More about this algorythm: https://en.wikipedia.org/wiki/Arithmetic_billiards")]
        public string[] method = new string[] { "p", "prime" };

        public PrimeFractalConfigurer()
        {
            _contextConfig = ContextConfig.GetInstance();
            _colorService = new ColorService();
            _rnd = new Random();
        }

        public PrimeFractalDrawer Configure(List<string> commandsList)
        {
            PrimeFractalConfigDTO config = new PrimeFractalConfigDTO();

            // determining width and height
            if (commandsList.ContainsFlag(heightFlag) && !commandsList.ContainsFlag(widthFlag))
            {
                config.Height = int.Parse(commandsList.GetFlagValue(heightFlag));
                config.Width = config.Height;
            }
            else if (commandsList.ContainsFlag(widthFlag) && !commandsList.ContainsFlag(heightFlag))
            {
                config.Width = int.Parse(commandsList.GetFlagValue(widthFlag));
                config.Height = config.Width;
            }
            else if (commandsList.ContainsFlag(heightFlag) && commandsList.ContainsFlag(widthFlag))
            {
                config.Height = int.Parse(commandsList.GetFlagValue(heightFlag));
                config.Width  = int.Parse(commandsList.GetFlagValue(widthFlag));
            }
            else
            {
                // use this monitor resolution
                config.Width = Screen.PrimaryScreen.Bounds.Width;
                config.Height = Screen.PrimaryScreen.Bounds.Height;
            }

            // determining brush possible sizes
            if (commandsList.ContainsFlag(brushFlag))
            {
                config.BrushSize = int.Parse(commandsList.GetFlagValue(brushFlag));
            }
            else
            {
                List<int> brushSizes = config.Width.Value.GetAllDivisors().ToList();
                config.BrushSize = brushSizes[_rnd.Next(0, brushSizes.Count)];
            }

            // determining colors
            Color color1, color2;
            if (commandsList.ContainsFlag(colorsFlag))
            {
                string c = commandsList.GetFlagValue(colorsFlag);
                List<string> colors = c.Replace(" ", "").Split(',').ToList();
                // TODO check if its color or color set title or file path. Now its only color
                color1 = colors[0].ToColor();
                color2 = colors[1].ToColor();
            }
            else
            {
                // TODO get from all color sets if does not specified
                color1 = _colorService.GetRandomColor(
                    _colorService.GetColorsFromFile(_contextConfig.ColorsSets.Sets[0].Path));
                color2 = _colorService.GetRandomColor(
                   _colorService.GetColorsFromFile(_contextConfig.ColorsSets.Sets[1].Path));
                if (_rnd.Next(0, 2) == 0)
                {
                    Color temp = color1;
                    color1 = color2;
                    color2 = temp;
                }
            }
            config.FillInsideColor = color1;
            config.FillOutsideColor = color2;

            // determining start corner position
            if (commandsList.ContainsFlag(setCornerFlag))
            {
                config.StartPosition = (PrimeFractalDrawer.CornerPosition)int.Parse(commandsList.GetFlagValue(setCornerFlag));
            }
            else
            {
                var cornerPositions = Enum.GetValues(typeof(PrimeFractalDrawer.CornerPosition))
                    .OfType<PrimeFractalDrawer.CornerPosition>().ToList();
                config.StartPosition = cornerPositions[_rnd.Next(0, cornerPositions.Count)];
            }

            // configure drawer
            return new PrimeFractalDrawer(config);
        }

        public string GetHelp(List<string> commandList = null, string helpFor = "")
        {
            // TODO: if commandLine specified then show help only for specified command

            string prefix = $"-m {method.First()}";

            if (string.IsNullOrWhiteSpace(helpFor))
            {
                return $"{string.Join(", ", heightFlag.Select(x => $"-{x}"))}: {DescriptionHelper.GetDescription<PrimeFractalConfigurer>(nameof(heightFlag))}\n" +
                $"{string.Join(", ", widthFlag.Select(x => $"-{x}"))}: {DescriptionHelper.GetDescription<PrimeFractalConfigurer>(nameof(widthFlag))}\n" +
                $"{string.Join(", ", brushFlag.Select(x => $"-{x}"))}: {DescriptionHelper.GetDescription<PrimeFractalConfigurer>(nameof(brushFlag))}\n" +
                $"{string.Join(", ", colorsFlag.Select(x => $"-{x}"))}: {DescriptionHelper.GetDescription<PrimeFractalConfigurer>(nameof(colorsFlag))}\n" +
                $"{string.Join(", ", setCornerFlag.Select(x => $"-{x}"))}: {DescriptionHelper.GetDescription<PrimeFractalConfigurer>(nameof(setCornerFlag))}\n";
            }
            else if (method.Contains(helpFor))
            {
                return  $"-m {helpFor}: {DescriptionHelper.GetDescription<PrimeFractalConfigurer>(nameof(method))}\n" +
                        $"{GetMethodHelp()}";
            }
            else if (heightFlag.Contains(helpFor[1..]))
            {
                return $"{prefix} {helpFor}: {DescriptionHelper.GetDescription<PrimeFractalConfigurer>(nameof(heightFlag))}";
            }
            else if (widthFlag.Contains(helpFor[1..]))
            {
                return $"{prefix} {helpFor}: {DescriptionHelper.GetDescription<PrimeFractalConfigurer>(nameof(widthFlag))}";
            }
            else if (brushFlag.Contains(helpFor[1..]))
            {
                return $"{prefix} {helpFor}: {DescriptionHelper.GetDescription<PrimeFractalConfigurer>(nameof(brushFlag))}";
            }
            else if (colorsFlag.Contains(helpFor[1..]))
            {
                return $"{prefix} {helpFor}: {DescriptionHelper.GetDescription<PrimeFractalConfigurer>(nameof(colorsFlag))}";
            }
            else if (setCornerFlag.Contains(helpFor[1..]))
            {
                return $"{prefix} {helpFor}: {DescriptionHelper.GetDescription<PrimeFractalConfigurer>(nameof(setCornerFlag))}";
            }

            return string.Empty;
        }

        public string GetMethodHelp()
        {
            return  $"\nFlags:\n" +
                    $"{GetHelp()}";
        }
    }
}
