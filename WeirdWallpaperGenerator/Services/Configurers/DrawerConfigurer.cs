using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using WeirdWallpaperGenerator.Configuration;
using WeirdWallpaperGenerator.Helpers;
using WeirdWallpaperGenerator.Models.CommandLineParts;
using WeirdWallpaperGenerator.Services.Drawers;

namespace WeirdWallpaperGenerator.Services.Configurers
{
    public abstract class DrawerConfigurer<T> : IHaveHelper where T : IDrawer
    {
        protected readonly ContextConfig _contextConfig;
        protected readonly Random _rnd;
        protected readonly ColorService _colorService;

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
        protected readonly string[] colorsFlag = new string[] { "c", "colors" };

        [Description(
            "sets the height of output picture. " +
            "Do not specify to set your main monitor height. " +
            "If specified without width, then width = height. " +
            "Usage: \n" +
            "  -h {integer number}")]
        protected readonly string[] heightFlag = new string[] { "h", "height" };

        [Description(
            "sets the width of output picture. " +
            "Do not specify to set your main monitor width. " +
            "If specified without height, then height = width. " +
            "Usage: \n" +
            "  -w {integer number}")]
        protected readonly string[] widthFlag = new string[] { "w", "width" };

        public DrawerConfigurer()
        {
            _contextConfig = ContextConfig.GetInstance();
            _rnd = new Random();
            _colorService = new ColorService();
        }

        /// <summary>
        /// configures instance of IDrawer according to specified arguments
        /// </summary>
        /// <param name="command"><see cref="Command"/> instance contained flags for configuring</param>
        /// <returns></returns>
        public abstract T Configure(Command command);

        /// <summary>
        /// returns a help string about this method
        /// </summary>
        /// <returns></returns>
        public virtual string GetMethodHelp()
        {
            throw new NotImplementedException();
        }

        protected virtual void GetWidthAndHeight(Command command, out int width, out int height)
        {
            // determining width and height
            if (command.ContainsFlag(heightFlag) && !command.ContainsFlag(widthFlag))
            {
                height = int.Parse(command.GetFlagValue(heightFlag));
                width = height;
            }
            else if (!command.ContainsFlag(heightFlag) && command.ContainsFlag(widthFlag))
            {
                width = int.Parse(command.GetFlagValue(widthFlag));
                height = width;
            }
            else if (command.ContainsFlag(heightFlag) && command.ContainsFlag(widthFlag))
            {
                height = int.Parse(command.GetFlagValue(heightFlag));
                width = int.Parse(command.GetFlagValue(widthFlag));
            }
            else
            {
                // use this monitor resolution
                width = Screen.PrimaryScreen.Bounds.Width;
                height = Screen.PrimaryScreen.Bounds.Height;
            }
        }

        protected virtual List<Color> GetColors(Command command, bool chanceOfSwap = false)
        {
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

                        GetTwoColors(input1, input2, out color1, out color2, chanceOfSwap);
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

            return new List<Color> { color1, color2 };
        }

        private void GetTwoColors(string[] input1, string[] input2, out Color color1, out Color color2, bool chanceOfSwap = false)
        {
            ColorSet choosenSet = null;
            color1 = Color.Transparent;
            color2 = Color.Transparent;

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

                    CheckChanceOfSwap(ref color1, ref color2);
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

                    CheckChanceOfSwap(ref color1, ref color2);
                }
            }
        }

        protected void CheckChanceOfSwap(ref Color color1, ref Color color2)
        {
            // a chance to swap collors
            if (_rnd.Next(0, 2) == 1)
            {
                Color tempColor = color1;
                color1 = color2;
                color2 = tempColor;
            }
        }

        public abstract string GetHelp(Command command = null);
    }
}
