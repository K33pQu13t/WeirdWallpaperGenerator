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
        public static readonly string[] commandHelp = new string[] { "help", "?" };
    }
}
