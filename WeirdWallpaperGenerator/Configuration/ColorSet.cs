using System.Collections.Generic;

namespace WeirdWallpaperGenerator.Configuration
{
    public class ColorSet
    {
        public string Title { get; set; }
        public string Path { get; set; }
    }

    public class ColorsSets
    {
        public List<ColorSet> Sets { get; set; } = new List<ColorSet>()
        {
            new ColorSet() {Title = "dark colors 1", Path = "Colors\\darker colors.txt"},
            new ColorSet() {Title = "dark colors 2", Path = "Colors\\lighter colors.txt"}
        };
    }
}
