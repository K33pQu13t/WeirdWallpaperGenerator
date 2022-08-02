using WallpaperGenerator.DTO;
using WallpaperGenerator.Helpers;
using WallpaperGenerator.Services.BackgroundDrawers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using WallpaperGenerator.Config;

namespace WallpaperGenerator.Services.Configurers
{
    class PrimeFractalConfigurer : IDrawerConfigurer<PrimeFractalDrawer>
    {
        readonly ColorService _colorService;
        readonly Random _rnd;
        readonly ContextConfig _contextConfig;

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
            if (commandsList.Contains("-h") && !commandsList.Contains("-w"))
            {
                config.Height = int.Parse(commandsList.GetFlagValue("-h"));
                config.Width = config.Height;
            }
            else if (commandsList.Contains("-w") && !commandsList.Contains("-h"))
            {
                config.Width = int.Parse(commandsList.GetFlagValue("-w"));
                config.Height = config.Width;
            }
            else if (commandsList.Contains("-h") && commandsList.Contains("-w"))
            {
                config.Height = int.Parse(commandsList.GetFlagValue("-h"));
                config.Width  = int.Parse(commandsList.GetFlagValue("-w"));
            }
            else
            {
                // use this monitor resolution
                config.Width = Screen.PrimaryScreen.Bounds.Width;
                config.Height = Screen.PrimaryScreen.Bounds.Height;
            }

            // determining brush possible sizes
            if (commandsList.Contains("-b"))
            {
                config.BrushSize = int.Parse(commandsList.GetFlagValue("-b"));
            }
            else
            {
                List<int> brushSizes = MathExtension.GetAllDivisors(config.Width.Value).ToList();
                config.BrushSize = brushSizes[_rnd.Next(0, brushSizes.Count)];
            }

            // determining colors
            Color color1, color2;
            if (commandsList.Contains("-c"))
            {
                string c = commandsList.GetFlagValue("-c");
                List<string> colors = c.Replace(" ", "").Split(',').ToList();
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
            if (commandsList.Contains("-sp"))
            {
                config.StartPosition = (PrimeFractalDrawer.CornerPosition)int.Parse(commandsList.GetFlagValue("-sp"));
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

        public string GetHelp()
        {
            throw new NotImplementedException();
        }
    }
}
