using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using WeirdWallpaperGenerator.Configuration;
using WeirdWallpaperGenerator.Constants;
using WeirdWallpaperGenerator.DTO;
using WeirdWallpaperGenerator.Enums.Drawers;
using WeirdWallpaperGenerator.Helpers;
using WeirdWallpaperGenerator.Models.CommandLineParts;
using WeirdWallpaperGenerator.Services.Drawers;

namespace WeirdWallpaperGenerator.Services.Configurers
{
    [Description("math billiards generation method")]
    class MathBilliardsConfigurer : IDrawerConfigurer<MathBilliardsDrawer>, IHaveHelper
    {
        readonly ColorService _colorService;
        readonly Random _rnd;
        readonly ContextConfig _contextConfig;

        [Description(
            "sets the height of output picture. " +
            "Do not specify to set your main monitor height. " +
            "If specified without width, then width = height. " +
            "Usage: \n" +
            "  -h {integer number}")]
        readonly string[] heightFlag = new string[] { "h", "height" };

        [Description(
            "sets the width of output picture. " +
            "Do not specify to set your main monitor width. " +
            "If specified without height, then height = width. " +
            "Usage: \n" +
            "  -w {integer number}")]
        readonly string[] widthFlag = new string[] { "w", "width" };

        [Description(
            "sets the brush size - minimal pattern square. " +
            "Width must can be divided by brush size without remainder. " +
            "brush value must be less than width and height " +
            "Usage: \n" +
            "  -b {integer number}")]
        readonly string[] brushFlag = new string[] { "b", "brush" };

        [Description(
            "sets colors for generating image. " +
            "Do not specify to get random colors. " +
            "Colors must be in hex format. Alpha channel allowed. " +
            "If specified, at least 1 color required.\n" +
            "Common usage:\n" +
            "  -c #d648d7 - first color would be as specified, second color would be some from " +
            "files listed in config.json\n\n" +
            "You can specify two colors, so they be as they are:\n" +
            "  -c #29ab87 #282c35 - both colors will be exactrly as specified\n\n" +
            "You can specify several colors as one argument for random picking them. For one argument, " +
            "both colors whould be picked from specified list:\n" +
            "  -c \"#4b0082, #ff6700, #645452, #c8aca9\" - both colors would be some from that list.\n\n" +
            "If you want second color would be set different from first color list, you can use this flag " +
            "as example above, but put two arguments:\n" +
            "  -c \"#eab76a, #2a52be, #de5d83\" #1f1f1f - first color would be random from the list, " +
            "second color would be exactly as specified\n" +
            "...or you can specify another list as second argument so the second color would be random from " +
            "second list:\n" +
            "  -c \"#00009c, #645452\" \"#0f0f0f, #e75480, #e48400\" - first color would be random from first list, " +
            "second color would be random from second list\n\n" +

            "You can also create a file with hex colors listed, one line - one color, " +
            "and fill ColorsSets group in config.json by example. So you can use titles " +
            "to specify files with colors for this flag.\n" +
            "Specifying title as file with colors:\n" +
            "  -c black - first color would be random from file which title in config.json is \"black\", " +
            "second color would be some from files listed in config.json\n\n" +
            "You can use titles same ways as hex colors from above. " +
            "You can even combine hex colors and color sets' titles:\n" +
            "  -с \"red, oragne, #0abab5, #ffd7e9\" \"#4d4dff, brown, white\" - if first random choise " +
            "would be for example \"red\" - then first color would be set as some of colors from title \"red\" " +
            "from config.json\n" +
            "Each part of argument has same chance to be picked, either it title or hex color\n\n" +
            "If you want first color be random from list, but second color be random from whole titles, " +
            "you can achieve it specifying second argument as empty: \n" +
            "  -c \"black, brown\" \"\"")]
        readonly string[] colorsFlag = new string[] { "c", "colors" };


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
            _contextConfig = ContextConfig.GetInstance();
            _colorService = new ColorService();
            _rnd = new Random();
        }

        public MathBilliardsDrawer Configure(Command command)
        {
            MathBilliardsConfigDTO config = new MathBilliardsConfigDTO();

            // determining width and height
            if (command.ContainsFlag(heightFlag) && !command.ContainsFlag(widthFlag))
            {
                config.Height = int.Parse(command.GetFlagValue(heightFlag));
                config.Width = config.Height;
            }
            else if (!command.ContainsFlag(heightFlag) && command.ContainsFlag(widthFlag))
            {
                config.Height = config.Width;
                config.Width = int.Parse(command.GetFlagValue(widthFlag));
            }
            else if (command.ContainsFlag(heightFlag) && command.ContainsFlag(widthFlag))
            {
                config.Height = int.Parse(command.GetFlagValue(heightFlag));
                config.Width  = int.Parse(command.GetFlagValue(widthFlag));
            }
            else
            {
                // use this monitor resolution
                config.Width = Screen.PrimaryScreen.Bounds.Width;
                config.Height = Screen.PrimaryScreen.Bounds.Height;
            }

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
            ColorSet choosenSet = null;
            Color color1 = Color.Transparent, color2 = Color.Transparent;
            try
            {
                if (command.ContainsFlag(colorsFlag))
                {
                    List<string> args = command.GetFlagValues(colorsFlag).ToList();
                    if (args.Count > 2)
                        throw ExceptionHelper.GetException(
                            nameof(MathBilliardsConfigurer), 
                            nameof(Configure),
                            $"For this method maximum colors is 2, while you specified {args.Count}");
                    if (args.Count == 0)
                        throw ExceptionHelper.GetException(
                            nameof(MathBilliardsConfigurer), 
                            nameof(Configure),
                            "Error while trying to get collors. If flag -c is used, then at least one color must be specified");

                    try
                    {
                        var input1 = args[0].Split(", ");
                        string[] input2 = null;
                        if (args.Count > 1)
                        {
                            input2 = args[1].Split(", ");
                        }

                        GetColor(input1, input2, out color1, out color2);
                    }
                    catch (ArgumentException)
                    {
                        throw ExceptionHelper.GetException(
                           nameof(MathBilliardsConfigurer).ToString(),
                           nameof(Configure).ToString(),
                           $"Wrong colors format. Colors must be specified as hex, like: #1b1b1b or #77c3c3c3 (with alpha-channel)");
                    }
                }
                else
                {
                    // get from all color sets if does not specified any
                    color1 = _colorService.GetRandomColorFromSets(_contextConfig.ColorsSets.Sets, out choosenSet);
                    color2 = _colorService.GetRandomColorFromSets(
                        _contextConfig.ColorsSets.Sets.Where(s => s != choosenSet).ToList(), out ColorSet _);
                }
            }
            catch (FileNotFoundException ex)
            {
                throw ExceptionHelper.GetException(
                     nameof(MathBilliardsConfigurer),
                     nameof(Configure),
                     $"Error trying to get collors from file. " +
                     $"\"{ex.FileName}\" file was not found. Check config.json and make sure colors paths exists");
            }

            config.FillInsideColor = color1;
            config.FillOutsideColor = color2;

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

        private void GetColor(string[] input1, string[] input2, out Color color1, out Color color2)
        {
            ColorSet choosenSet = null;
            color1 = Color.Transparent;
            color2 = Color.Transparent;

            void CheckChanceOfSwap(Color color1, Color color2)
            {
                // a chance to swap collors
                if (_rnd.Next(0, 2) == 1)
                {
                    Color tempColor = color1;
                    color1 = color2;
                    color2 = tempColor;
                }
            }

            // if value is color, returns Color from it, or if it isn't, tries to get a color from file with title as value
            Color GetColorOrColorFromFile(string value, out ColorSet _choosenSet)
            {
                _choosenSet = null;
                if (value.IsColorString())
                {
                    return value.ToColor();
                }
                else
                {
                    var set = _contextConfig.ColorsSets.Sets.FirstOrDefault(s => s.Title.ToLower() == value.ToLower());
                    if (set == null)
                    {
                        throw ExceptionHelper.GetException(
                           nameof(MathBilliardsConfigurer).ToString(),
                           nameof(Configure).ToString(),
                           $"Can't find such set like \"{value}\" in config.json. " +
                           $"Probably there is a spelling mistake, or you forgot to set this color set in config.json. " +
                           $"If you meant an exactly color, then notice what colors must be specified as hex, " +
                           $"like: #1b1b1b or #77c3c3c3 (with alpha-channel)");
                    }

                    return _colorService.GetRandomColorFromSets(new List<ColorSet> { set }, out _choosenSet);
                }
            }

            // if only one argument specified
            if (input2 == null)
            {
                if (input1.Length == 0)
                {
                    throw ExceptionHelper.GetException(
                        nameof(MathBilliardsConfigurer),
                        nameof(Configure),
                        "Error while trying to get collors. If flag -c is used, then at least one color must be specified");
                }
                if (input1.Length == 1)
                {
                    var value = input1.First();
                    color1 = GetColorOrColorFromFile(value, out choosenSet);

                    color2 = _colorService.GetRandomColorFromSets(
                        _contextConfig.ColorsSets.Sets.Where(s => s != choosenSet).ToList(), out ColorSet _);

                    CheckChanceOfSwap(color1, color2);
                }
                else
                {
                    var value1 = input1[_rnd.Next(0, input1.Length)];
                    input1 = input1.Where(x => x != value1).ToArray();
                    var value2 = input1[_rnd.Next(0, input1.Length)];

                    color1 = GetColorOrColorFromFile(value1, out choosenSet);
                    color2 = GetColorOrColorFromFile(value2, out _);
                }
            }
            else
            {
                var value1 = input1[_rnd.Next(0, input1.Length)];
                color1 = GetColorOrColorFromFile(value1, out choosenSet);

                if (input2.Length != 0 && !string.IsNullOrEmpty(input2.First()))
                {
                    var value2 = input2[_rnd.Next(0, input2.Length)];
                    color2 = GetColorOrColorFromFile(value2, out _);
                }
                else
                {
                    color2 = _colorService.GetRandomColorFromSets(
                        _contextConfig.ColorsSets.Sets.Where(s => s != choosenSet).ToList(), out _);

                    CheckChanceOfSwap(color1, color2);
                }
            }
        }

        public string GetHelp(Command command = null)
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

            if (methods.Contains(val))
            {
                return  $"-m {val}: {DescriptionHelper.GetDescription<MathBilliardsConfigurer>(nameof(methods))}\n" +
                        $"{GetMethodHelp()}";
            }
            else if (heightFlag.Contains(flagForHelp))
            {
                return $"{prefix} {flagForHelp}: {DescriptionHelper.GetDescription<MathBilliardsConfigurer>(nameof(heightFlag))}";
            }
            else if (widthFlag.Contains(flagForHelp))
            {
                return $"{prefix} {flagForHelp}: {DescriptionHelper.GetDescription<MathBilliardsConfigurer>(nameof(widthFlag))}";
            }
            else if (brushFlag.Contains(flagForHelp))
            {
                return $"{prefix} {flagForHelp}: {DescriptionHelper.GetDescription<MathBilliardsConfigurer>(nameof(brushFlag))}";
            }
            else if (colorsFlag.Contains(flagForHelp))
            {
                return $"{prefix} {flagForHelp}: {DescriptionHelper.GetDescription<MathBilliardsConfigurer>(nameof(colorsFlag))}";
            }
            else if (setCornerFlag.Contains(flagForHelp))
            {
                return $"{prefix} {flagForHelp}: {DescriptionHelper.GetDescription<MathBilliardsConfigurer>(nameof(setCornerFlag))}";
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
