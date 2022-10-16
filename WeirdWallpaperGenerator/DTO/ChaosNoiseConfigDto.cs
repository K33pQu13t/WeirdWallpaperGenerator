using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace WeirdWallpaperGenerator.DTO
{
    public class ChaosNoiseConfigDto : DrawerConfigDto
    {
        public Color ColoredColor { get; set; }
        public Color BackgroundColor { get; set; }
        public int BrushSizeX { get; set; }

        public int BrushSizeY { get; set; }
    }
}
