using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using static WallpaperGenerator.Services.BackgroundDrawers.PrimeFractalDrawer;

namespace WallpaperGenerator.DTO
{
    public class PrimeFractalConfigDTO : ConfigDTO
    {
        public Color FillInsideColor { get; set; }
        public Color FillOutsideColor { get; set; }
        public int BrushSize { get; set; }
        public CornerPosition StartPosition { get; set; }
    }
}
