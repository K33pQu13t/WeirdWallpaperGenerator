using System.Drawing;
using static WeirdWallpaperGenerator.Services.Drawers.PrimeFractalDrawer;

namespace WeirdWallpaperGenerator.DTO
{
    public class PrimeFractalConfigDTO : ConfigDTO
    {
        public Color FillInsideColor { get; set; }
        public Color FillOutsideColor { get; set; }
        public int BrushSize { get; set; }
        public CornerPosition StartPosition { get; set; }
    }
}
