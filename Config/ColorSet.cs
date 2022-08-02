using System;
using System.Collections.Generic;
using System.Text;

namespace WallpaperGenerator.Config
{
    public class ColorSet
    {
        public string Title { get; set; }
        public string Path { get; set; }
    }

    public class ColorsSets
    {
        public List<ColorSet> Sets { get; set; }
    }
}
