using System.ComponentModel;

namespace WeirdWallpaperGenerator.Constants
{
    public class BasicCommandList
    {
        [Description(
            "shows the help about generic commands.\n" +
            "Specify a command or flag to get the exactly help. Usage: {command or flag} ? or ? {command or flag}\n" +
            "Or you can put it in the end of command line to get help about last element in command line. " +
            "Usage: \n" +
            "  [any command line] ?\n" +
            "Like \"/g -m mb ?\" gets the help about math billiards method generation")]
        public static readonly string[] commandHelp = new string[] { "?", "help" };

        [Description("generates an image and saves it. Usage: \n" +
          "  /g [flags]")]
        public static readonly string[] commandGenerate = new string[] { "g", "gen", "generate" };

        [Description("generates an image, saves it and sets it as background image. Usage: \n" +
            "  /sw [flags]")]
        public static readonly string[] commandSetWallpaper = new string[] { "sw", "setwp" };

        [Description("checks if there is an update, and downloads it. Usage: \n" +
            "  /u")]
        public static readonly string[] commandUpdate = new string[] { "update", "upd", "u" };


        [Description(
           "specifies a generation method. If not specified, then random method will be choosen, and some unique " +
           "configuration flags are not allowed. " +
           "Usage: -m {one of methods} [common generation flags like -w -h]")]
        public static readonly string[] flagMethod = new string[] { "m", "method" };
    }
}
